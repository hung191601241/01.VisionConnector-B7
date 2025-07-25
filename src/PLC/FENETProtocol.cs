using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VisionInspection
{
    public delegate void DlOneParam(EndPoint remoteEP, Boolean isConnected);

    public delegate void LogCallback(String msg);
    public class FENETProtocol
    {
        // Cmd & Feedback
        private readonly byte[] CMD_READ = { 0x54, 0x00 }; // CMD Read Data PLC
        private readonly byte[] CMD_FEEDBACK_READ = { 0x55, 0x00 }; // CMD Read Data Feedback From PLC

        private readonly byte[] CMD_WRITE = { 0x58, 0x00 }; // CMD Write Data PLC
        private readonly byte[] CMD_FEEDBACK_WRITE = { 0x59, 0x00 }; // CMD Write Data Feedback From PLC

        private readonly byte[] BYTE_WRITE_TRUE = { 0x01, 0x00 }; // Data Add When Write True
        private readonly byte[] BYTE_WRITE_FALSE = { 0x00, 0x00 }; // Data Add When Write Flase

        private readonly byte[] BYTE_TRUE = { 0x01 }; // Data Byte True

        private readonly byte[] BYTE_NO_ERROR = { 0x00, 0x00 }; // Data Feedback If No Error

        private readonly byte[] BYTE_LSIS = { 0x4C, 0x53, 0x49, 0x53, 0x2D, 0x58, 0x47, 0x54, 0x00, 0x00 }; // Name Company: LSIS-XGT/n/n
        private readonly byte[] BYTE_LGIS = { 0x4C, 0x47, 0x49, 0x53, 0x2D, 0x47, 0x4C, 0x4F, 0x46, 0x41 }; // Name Company: LGIS-GLOFA

        private const byte PLC_INFOR = 0x00;
        private const byte SOURCE_OF_FRAME = 0x33;
        private const byte SOURCE_OF_FRAME_FEEDBACK = 0x11;
        private const byte INVOKE_ID = 0x00;
        private const byte RESERVED = 0x00;

        private MyLogger Logger; // Logger
        private FENETProtocolSettings Settings; // Settings Of FENETProtocol
        private Socket TcpClient;
        private bool IsRunning = false;
        private static object PlcLocker = new object();

        private LogCallback LogCallback; // Write Log

        private Thread ThreadMonitor;
        private event DlOneParam ConnectionChanged; // Conenction Change
        private String lastLog = "";

        public static FENETProtocol PLC;
        private static Object LockerPLC = new object();

        #region Check Connect PC To PLC
        public Boolean IsConnected
        {
            get
            {
                if (this.TcpClient != null)
                {
                    if (this.TcpClient.Connected)
                    {
                        if (this.TcpClient.Poll(1, SelectMode.SelectRead) && (this.TcpClient.Available == 0))
                        {
                            try
                            {
                                byte[] tmp = new byte[1];
                                this.TcpClient.Send(tmp, 0, 0);
                                return true;
                            }
                            catch (SocketException e)
                            {
                                if (e.NativeErrorCode.Equals(10035)) { return true; }
                                else { return false; }
                            }
                        }
                        else { return true; }
                    }
                    else { return false; }
                }
                else { return false; }
            }
        }
        #endregion

        public FENETProtocol(FENETProtocolSettings settings, DlOneParam connectionChanged, LogCallback callBack = null)
        {
            this.Settings = settings;
            this.Logger = new MyLogger(String.Format("FENETProtocol {0}", this.Settings.PLCName));
            this.ConnectionChanged = connectionChanged;
            this.LogCallback = callBack;
        }
        private async void managerSocketFENETProtocol()
        {
            try
            {
                var remoteEp = new IPEndPoint(IPAddress.Parse(this.Settings.PLCIp), this.Settings.PLCPort);
                // Add log
                Logger.Create(string.Format("  ==> Connecting to {0} ...", remoteEp));
                this.CreateLog(string.Format("  ==> Connecting to {0} ...", remoteEp));
                while (this.IsRunning)
                {
                    this.TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    Task tsk1 = this.TcpClient.ConnectAsync(remoteEp.Address, remoteEp.Port);
                    Task tsk2 = Task.Delay(100);
                    await Task.WhenAny(new List<Task> { tsk1, tsk2 });
                    if (this.TcpClient.Connected)
                    {
                        Logger.Create(String.Format(" ==> Connected To PLC {0}.", remoteEp));
                        this.CreateLog(String.Format(" ==> Connected To PLC {0}.", remoteEp));
                    }
                    else
                    {
                        Logger.Create(String.Format(" ==> Connect To PLC {0} Failed.", remoteEp));
                        this.CreateLog(String.Format(" ==> Connect To PLC {0} Failed.", remoteEp));

                        this.TcpClient.Close();
                        this.TcpClient.Dispose();

                        Thread.Sleep(10);

                        continue;
                    }
                    if (this.ConnectionChanged != null)
                    {
                        this.ConnectionChanged(this.TcpClient.RemoteEndPoint, true);
                    }
                    while (this.IsRunning && this.IsConnected)
                    {
                        await Task.Delay(1);
                    }
                    if (!this.IsRunning)
                    {
                        Logger.Create(String.Format(" -> User STOP Connect To {0} !!!", remoteEp));
                        this.CreateLog(String.Format(" -> User STOP Connect To {0} !!!", remoteEp));

                        if (this.ConnectionChanged != null)
                        {
                            this.ConnectionChanged(this.TcpClient.RemoteEndPoint, false);
                        }
                        this.TcpClient.Shutdown(SocketShutdown.Both);
                        this.TcpClient.Close();
                        this.TcpClient.Dispose();

                        Thread.Sleep(100);
                        continue;
                    }
                    if (!this.IsConnected)
                    {
                        Logger.Create(String.Format(" -> FENET Protocol Tcp Close Socket (Disconnect To {0}) - IsConnected = False!!!", remoteEp));
                        this.CreateLog(String.Format(" -> FENET Protocol Tcp Close Socket (Disconnect To {0}) - IsConnected = False!!!", remoteEp));

                        if (this.ConnectionChanged != null)
                        {
                            this.ConnectionChanged(this.TcpClient.RemoteEndPoint, false);
                        }
                        this.TcpClient.Shutdown(SocketShutdown.Both);
                        this.TcpClient.Close();
                        this.TcpClient.Dispose();

                        Thread.Sleep(100);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Create(String.Format("Manager Socket FENET Protocol Error: " + ex.Message));
            }
        }
        public void Start()
        {
            try
            {
                this.IsRunning = true;
                this.ThreadMonitor = new Thread(new ThreadStart(this.managerSocketFENETProtocol));
                this.ThreadMonitor.IsBackground = true;
                this.ThreadMonitor.Start();
            }
            catch (Exception ex)
            {
                Logger.Create(String.Format("Start Manager Socket FENET Protocol Error: " + ex.Message));
            }
        }
        public void Stop()
        {
            try
            {
                this.IsRunning = false;
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                this.Logger.Create(String.Format("Stop Error: " + ex.Message));
            }
        }

        private bool checkHexaDecimaInput(String input)
        {
            return string.IsNullOrEmpty(input) ? false : int.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out _);
        }
        private bool checkStringAddr(String inputAddr)   // Example: 10A ==> True, 1AA => False
        {
            if (inputAddr.Length == 1)
            {
                return true;
            }
            else
            {
                UInt32 data = 0;
                var subStringWord = inputAddr.Substring(0, inputAddr.Length - 1);
                return UInt32.TryParse(subStringWord, out data);
            }
        }
        private byte[] getAddrBit(string addrInput, int index = 0)
        {
            var subStringWord = addrInput.Substring(0, addrInput.Length - 1).Trim();
            UInt32 addrAct = 0;
            if (!string.IsNullOrEmpty(subStringWord))
            {
                addrAct = UInt32.Parse(subStringWord);
            }

            var subString = addrInput.Substring(addrInput.Length - 1, 1).Trim();
            UInt32 addrAct1 = 0;
            if (!string.IsNullOrEmpty(subString))
            {
                addrAct1 = Convert.ToUInt32(subString, 16);
            }

            return ASCIIEncoding.ASCII.GetBytes((addrAct * 16 + addrAct1 + index).ToString());
        }
        private byte[] getAddr(string addrInput)
        {
            UInt32 addrAct = 0;
            addrAct = UInt32.Parse(addrInput);
            return ASCIIEncoding.ASCII.GetBytes(addrAct.ToString());
        }
        private byte[] getAddr_Continuous(string addrInput)
        {
            UInt32 addrAct = 0;
            addrAct = UInt32.Parse(addrInput);
            var addr = addrAct * 2;
            return ASCIIEncoding.ASCII.GetBytes(addr.ToString());
        }

        #region Block Transfer To PLC
        private List<byte[]> FcReadMultiBlock(byte deviceCodes, List<FENETProtocolBlock> numberBlock)
        {
            var txBuf = this.Header_Company_ID();                             //From ..........[0] To ..........[9]  Add Packet Header_Company ID(10 byte)    

            txBuf.AddRange(this.Header_PLC_Info());                           //From ..........[10] To ..........[11]  Add Packet PLC Information (2 byte)

            txBuf.AddRange(this.Header_CPU_Info());                           //..........[12]  Add Packet CPU Information (1 byte)

            txBuf.AddRange(this.Header_Source_Of_Frame());                    //..........[13]  Add Packet Source Of Frame/ Frame Direction (1 byte)

            txBuf.AddRange(this.Header_Invoke_ID());                          //From ..........[14] To ..........[15]  Add Invoke ID (2 byte) 

            int Length = 0;                                                   //From ..........[16] To ..........[17]  Add Length (2 byte)
            Length = numberBlock.Sum(item => 2 + item.DeviceName.Length + item.Address.Length);

            int REQUEST_DATA_LENGTH = 2 + 2 + 2 + 2 + Length;
            byte REQUEST_DATA_LENGTH_L = (Byte)(REQUEST_DATA_LENGTH & 0xFF);
            byte REQUEST_DATA_LENGTH_H = (Byte)(REQUEST_DATA_LENGTH >> 8);
            txBuf.Add(REQUEST_DATA_LENGTH_L);
            txBuf.Add(REQUEST_DATA_LENGTH_H);

            txBuf.Add(this.Header_FEnet_Position());                          //..........[18]  Add  FEnet Position (1 byte)

            var checkSum = FENETProtocolLib.ByteCheckSum(txBuf.ToArray(), 0, 18);       //..........[19]  Add  Add Check Sum (1 byte)
            txBuf.Add(checkSum);

            txBuf.AddRange(this.CMD_READ);                                    //From ..........[20] To ..........[21]  Add Command (2 byte)

            txBuf.AddRange(FENETProtocolDeviceName.getDeviceAddrType(deviceCodes));                               //From ..........[22] To ..........[23]  Add Data Type (2 byte)  

            txBuf.AddRange(this.Header_Reserved());                           //From ..........[24] To ..........[25]  Add Reserved (2 byte)  

            byte REQUEST_BLOCK_L = (Byte)(numberBlock.Count & 0xFF);  //From ..........[26] To ..........[27]  Add Number Of Block (2 byte)  
            byte REQUEST_BLOCK_H = (Byte)(numberBlock.Count >> 8);
            txBuf.Add(REQUEST_BLOCK_L);
            txBuf.Add(REQUEST_BLOCK_H);

            var listBlock = new List<FENETProtocolBlock_Frame>();
            listBlock.AddRange(Enumerable.Range(0, numberBlock.Count).Select(i => new FENETProtocolBlock_Frame
            {
                DeciveNameAddress = numberBlock[i],
                LengthDeviceAndAddress = FENETProtocolLib.GetLength(numberBlock[i].DeviceName.Length + numberBlock[i].Address.Length)
            }));
            var byteList = listBlock.SelectMany(frame =>
            {
                var result = new List<byte>();
                result.AddRange(frame.LengthDeviceAndAddress); // Length
                result.AddRange(frame.DeciveNameAddress.DeviceName); // Device Name
                result.AddRange(frame.DeciveNameAddress.Address); // Address
                return result;
            }).ToList();
            txBuf.AddRange(byteList);
            this.Send(txBuf.ToArray());

            // Get response:
            var rxPacket = this.ReceivePacketMultiBlock(txBuf, 20 + 2 + 2 + 2 + 2 + 2 + 2 * numberBlock.Count + numberBlock.Count); // Header (20 Byte) + Command (2 Byte) + Data Type (2 Byte) + Reserved Area (2 Byte) + Error Status (2 Byte) + Variable_Length (2 Byte) + Data_Count (2 Byte) + Data (1 Byte)
            if (rxPacket == null) { return null; }
            return rxPacket.List_Data; // Return Data When Read Data By MultiBlock
        }
        private List<byte[]> FcReadMultiBlock_Continuous(byte deviceCodes, FENETProtocolBlock numberBlock)
        {
            var txBuf = this.Header_Company_ID();                             //From ..........[0] To ..........[9]  Add Packet Header_Company ID(10 byte)    

            txBuf.AddRange(this.Header_PLC_Info());                           //From ..........[10] To ..........[11]  Add Packet PLC Information (2 byte)

            txBuf.AddRange(this.Header_CPU_Info());                           //..........[12]  Add Packet CPU Information (1 byte)

            txBuf.AddRange(this.Header_Source_Of_Frame());                    //..........[13]  Add Packet Source Of Frame/ Frame Direction (1 byte)

            txBuf.AddRange(this.Header_Invoke_ID());                          //From ..........[14] To ..........[15]  Add Invoke ID (2 byte) 

            //From ..........[16] To ..........[17]  Add Length (2 byte)
            var LengthOneBlock = 2 + numberBlock.DeviceName.Length + numberBlock.Address.Length + numberBlock.NumberOfData.Length; // 2 byte (Variable Length) + n byte (Device Name) + n byte (Address) + n byte (Number Of Data) 

            int REQUEST_DATA_LENGTH = 2 + 2 + 2 + 2 + LengthOneBlock;
            byte REQUEST_DATA_LENGTH_L = (Byte)(REQUEST_DATA_LENGTH & 0xFF);
            byte REQUEST_DATA_LENGTH_H = (Byte)(REQUEST_DATA_LENGTH >> 8);
            txBuf.Add(REQUEST_DATA_LENGTH_L);
            txBuf.Add(REQUEST_DATA_LENGTH_H);

            txBuf.Add(this.Header_FEnet_Position());                          //..........[18]  Add  FEnet Position (1 byte)

            var checkSum = FENETProtocolLib.ByteCheckSum(txBuf.ToArray(), 0, 18);       //..........[19]  Add  Add Check Sum (1 byte)
            txBuf.Add(checkSum);

            txBuf.AddRange(this.CMD_READ);                                    //From ..........[20] To ..........[21]  Add Command (2 byte)

            txBuf.AddRange(FENETProtocolDeviceName.getDeviceAddrType(deviceCodes));                               //From ..........[22] To ..........[23]  Add Data Type (2 byte)  

            txBuf.AddRange(this.Header_Reserved());                           //From ..........[24] To ..........[25]  Add Reserved (2 byte)  

            var numberBlockSentContinuous = 1; // When Read Continuous Mode Number Block Allway 1 Block
            byte REQUEST_BLOCK_L = (Byte)(numberBlockSentContinuous & 0xFF);  //From ..........[26] To ..........[27]  Add Number Of Block (2 byte)  
            byte REQUEST_BLOCK_H = (Byte)(numberBlockSentContinuous >> 8);
            txBuf.Add(REQUEST_BLOCK_L);
            txBuf.Add(REQUEST_BLOCK_H);

            // Add Data Block
            var listBlock = new List<FENETProtocolBlock_Frame>();
            listBlock.AddRange(Enumerable.Range(0, numberBlockSentContinuous).Select(i => new FENETProtocolBlock_Frame
            {
                DeciveNameAddress = numberBlock,
                LengthDeviceAndAddress = FENETProtocolLib.GetLength(numberBlock.DeviceName.Length + numberBlock.Address.Length)
            }));
            var byteList = listBlock.SelectMany(frame =>
            {
                var result = new List<byte>();
                result.AddRange(frame.LengthDeviceAndAddress); // Length
                result.AddRange(frame.DeciveNameAddress.DeviceName); // Device Name
                result.AddRange(frame.DeciveNameAddress.Address); // Address
                result.AddRange(frame.DeciveNameAddress.NumberOfData); // Number Of Data
                return result;
            }).ToList();
            txBuf.AddRange(byteList);

            this.Send(txBuf.ToArray());

            // Get response:
            var rxPacket = this.ReceivePacketMultiBlock(txBuf, 20 + 2 + 2 + 2 + 2 + 2 + 2 + BitConverter.ToUInt16(numberBlock.NumberOfData, 0)); // Header (20 Byte) + Command (2 Byte) + Data Type (2 Byte) + Reserved Area (2 Byte) + Error Status (2 Byte) + Variable_Length (2 Byte) + Data_Count (2 Byte) + Data (n Byte)
            if (rxPacket == null) { return null; }
            return rxPacket.List_Data; // Return Data When Read Data By MultiBlock Continuous
        }
        private byte[] FcWriteMultiBlock(byte deviceCodes, List<FENETProtocolBlock> numberBlock)
        {
            var txBuf = this.Header_Company_ID();                             //From ..........[0] To ..........[9]  Add Packet Header_Company ID(10 byte)    

            txBuf.AddRange(this.Header_PLC_Info());                           //From ..........[10] To ..........[11]  Add Packet PLC Information (2 byte)

            txBuf.AddRange(this.Header_CPU_Info());                           //..........[12]  Add Packet CPU Information (1 byte)

            txBuf.AddRange(this.Header_Source_Of_Frame());                    //..........[13]  Add Packet Source Of Frame/ Frame Direction (1 byte)

            txBuf.AddRange(this.Header_Invoke_ID());                          //From ..........[14] To ..........[15]  Add Invoke ID (2 byte) 

            int Length = 0;                                                   //From ..........[16] To ..........[17]  Add Length (2 byte)
            Length = numberBlock.Sum(item => 2 + item.DeviceName.Length + item.Address.Length + 2 + item.DataWrite.Length);

            int REQUEST_DATA_LENGTH = 2 + 2 + 2 + 2 + Length;
            byte REQUEST_DATA_LENGTH_L = (Byte)(REQUEST_DATA_LENGTH & 0xFF);
            byte REQUEST_DATA_LENGTH_H = (Byte)(REQUEST_DATA_LENGTH >> 8);
            txBuf.Add(REQUEST_DATA_LENGTH_L);
            txBuf.Add(REQUEST_DATA_LENGTH_H);

            txBuf.Add(this.Header_FEnet_Position());                          //..........[18]  Add  FEnet Position (1 byte)

            var checkSum = FENETProtocolLib.ByteCheckSum(txBuf.ToArray(), 0, 18);       //..........[19]  Add  Add Check Sum (1 byte)
            txBuf.Add(checkSum);

            txBuf.AddRange(this.CMD_WRITE);                                    //From ..........[20] To ..........[21]  Add Command (2 byte)

            txBuf.AddRange(FENETProtocolDeviceName.getDeviceAddrType(deviceCodes));                               //From ..........[22] To ..........[23]  Add Data Type (2 byte)  

            txBuf.AddRange(this.Header_Reserved());                           //From ..........[24] To ..........[25]  Add Reserved (2 byte)  

            byte REQUEST_BLOCK_L = (Byte)(numberBlock.Count & 0xFF);  //From ..........[26] To ..........[27]  Add Number Of Block (2 byte)  
            byte REQUEST_BLOCK_H = (Byte)(numberBlock.Count >> 8);
            txBuf.Add(REQUEST_BLOCK_L);
            txBuf.Add(REQUEST_BLOCK_H);

            var listBlock = new List<FENETProtocolBlock_Frame>();
            listBlock.AddRange(Enumerable.Range(0, numberBlock.Count).Select(i => new FENETProtocolBlock_Frame
            {
                DeciveNameAddress = numberBlock[i],
                LengthDeviceAndAddress = FENETProtocolLib.GetLength(numberBlock[i].DeviceName.Length + numberBlock[i].Address.Length)
            }));
            var lengthAndDeviceName = listBlock.SelectMany(frame =>
            {
                var ret = new List<byte>();
                ret.AddRange(frame.LengthDeviceAndAddress); // Length Of Device Name
                ret.AddRange(frame.DeciveNameAddress.DeviceName); // Device Name
                ret.AddRange(frame.DeciveNameAddress.Address); // Address
                return ret;
            }).ToList();
            txBuf.AddRange(lengthAndDeviceName);

            var lengthAndDataWrite = listBlock.SelectMany(frame =>
            {
                var ret = new List<byte>();
                ret.AddRange(FENETProtocolLib.GetLength(frame.DeciveNameAddress.DataWrite.Length)); // Length Of Data
                ret.AddRange(frame.DeciveNameAddress.DataWrite); // Data
                return ret;
            }).ToList();
            txBuf.AddRange(lengthAndDataWrite);

            this.Send(txBuf.ToArray());

            // Get response:
            var rxPacket = this.ReceivePacketMultiBlock(txBuf, 20 + 2 + 2 + 2 + 2 + 2); // Header (20 Byte) + Command (2 Byte) + Data Type (2 Byte) + Reserved Area (2 Byte) + Error Status (2 Byte) + Variable_Length (2 Byte) + Data_Count (2 Byte) + Data (1 Byte)
            if (rxPacket == null) { return null; }
            return rxPacket.Error_Status; // Return Error Status When Write MultiBlock
        }
        private byte[] FcWriteMultiBlock_Continuous(byte deviceCodes, FENETProtocolBlock numberBlock)
        {
            var txBuf = this.Header_Company_ID();                             //From ..........[0] To ..........[9]  Add Packet Header_Company ID(10 byte)    

            txBuf.AddRange(this.Header_PLC_Info());                           //From ..........[10] To ..........[11]  Add Packet PLC Information (2 byte)

            txBuf.AddRange(this.Header_CPU_Info());                           //..........[12]  Add Packet CPU Information (1 byte)

            txBuf.AddRange(this.Header_Source_Of_Frame());                    //..........[13]  Add Packet Source Of Frame/ Frame Direction (1 byte)

            txBuf.AddRange(this.Header_Invoke_ID());                          //From ..........[14] To ..........[15]  Add Invoke ID (2 byte) 

            //From ..........[16] To ..........[17]  Add Length (2 byte)
            var LengthOneBlock = 2 + numberBlock.DeviceName.Length + numberBlock.Address.Length + numberBlock.NumberOfData.Length + numberBlock.DataWrite.Length; // 2 byte (Variable Length) + n byte (Device Name) + n byte (Address) + n byte (Number Of Data) 

            int REQUEST_DATA_LENGTH = 2 + 2 + 2 + 2 + LengthOneBlock;
            byte REQUEST_DATA_LENGTH_L = (Byte)(REQUEST_DATA_LENGTH & 0xFF);
            byte REQUEST_DATA_LENGTH_H = (Byte)(REQUEST_DATA_LENGTH >> 8);
            txBuf.Add(REQUEST_DATA_LENGTH_L);
            txBuf.Add(REQUEST_DATA_LENGTH_H);

            txBuf.Add(this.Header_FEnet_Position());                          //..........[18]  Add  FEnet Position (1 byte)

            var checkSum = FENETProtocolLib.ByteCheckSum(txBuf.ToArray(), 0, 18);       //..........[19]  Add  Add Check Sum (1 byte)
            txBuf.Add(checkSum);

            txBuf.AddRange(this.CMD_WRITE);                                    //From ..........[20] To ..........[21]  Add Command (2 byte)

            txBuf.AddRange(FENETProtocolDeviceName.getDeviceAddrType(deviceCodes));                               //From ..........[22] To ..........[23]  Add Data Type (2 byte)  

            txBuf.AddRange(this.Header_Reserved());                           //From ..........[24] To ..........[25]  Add Reserved (2 byte)  

            var numberBlockSentContinuous = 1; // When Read Continuous Mode Number Block Allway 1 Block
            byte REQUEST_BLOCK_L = (Byte)(numberBlockSentContinuous & 0xFF);  //From ..........[26] To ..........[27]  Add Number Of Block (2 byte)  
            byte REQUEST_BLOCK_H = (Byte)(numberBlockSentContinuous >> 8);
            txBuf.Add(REQUEST_BLOCK_L);
            txBuf.Add(REQUEST_BLOCK_H);

            // Add Data Block
            var listBlock = new List<FENETProtocolBlock_Frame>();
            listBlock.AddRange(Enumerable.Range(0, numberBlockSentContinuous).Select(i => new FENETProtocolBlock_Frame
            {
                DeciveNameAddress = numberBlock,
                LengthDeviceAndAddress = FENETProtocolLib.GetLength(numberBlock.DeviceName.Length + numberBlock.Address.Length)
            }));
            var byteList = listBlock.SelectMany(frame =>
            {
                var result = new List<byte>();
                result.AddRange(frame.LengthDeviceAndAddress); // Length Variable
                result.AddRange(frame.DeciveNameAddress.DeviceName); // Device Name
                result.AddRange(frame.DeciveNameAddress.Address); // Address

                result.AddRange(frame.DeciveNameAddress.NumberOfData); // Number Of Data
                result.AddRange(frame.DeciveNameAddress.DataWrite); // Data Write
                return result;
            }).ToList();
            txBuf.AddRange(byteList);

            this.Send(txBuf.ToArray());

            // Get response:
            var rxPacket = this.ReceivePacketMultiBlock(txBuf, 20 + 2 + 2 + 2 + 2 + 2); // Header (20 Byte) + Command (2 Byte) + Data Type (2 Byte) + Reserved Area (2 Byte) + Error Status (2 Byte) + Variable_Length (2 Byte) + Data_Count (2 Byte) + Data (1 Byte)
            if (rxPacket == null) { return null; }
            return rxPacket.Error_Status; // Return Error Status When Write MultiBlock Continuous
        }
        private FENETProtocolPacket ReceivePacketMultiBlock(List<byte> PacketHeader, Int32 expectedSize)
        {
            #region Reciver Data
            var rxBuf = new byte[expectedSize];
            if (!this.TcpClient.Connected)
            {
                this.ConnectionChanged(this.TcpClient.RemoteEndPoint, false);
                return null;
            }
            try
            {
                int tout = FENETProtocolConstants.TCP_FENET_RX_TIMEOUT;
                while (tout > 0 && this.IsRunning)
                {
                    if (this.TcpClient.Receive(rxBuf, 0, rxBuf.Length, SocketFlags.None) == expectedSize) { break; }
                    Thread.Sleep(10);
                    tout -= 1;
                }
                if (tout == 0 || !this.IsRunning) { return null; }
            }
            catch (Exception ex)
            {
                this.Logger.Create(String.Format("Receive Packet Error: " + ex.Message));
                return null;
            }
            #endregion

            #region Write To Log File When Reciver Data From PLC
            var hexString = string.Concat(rxBuf.Select(b => b.ToString("X2")));
            this.CreateLog(String.Format("RX({0}) :{1}", rxBuf.Length, hexString));
            this.WriteLog(false, rxBuf, hexString);
            #endregion

            #region Data Packet Reciver From Client
            var ret = new FENETProtocolPacket();
            #endregion

            #region Decode Company ID
            if (rxBuf[0] != PacketHeader[0])
            {
                this.CreateLog(" -> Invalid Byte No.[0] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[1] != PacketHeader[1])
            {
                this.CreateLog(" -> Invalid Byte No.[1] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[2] != PacketHeader[2])
            {
                this.CreateLog(" -> Invalid Byte No.[2] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[3] != PacketHeader[3])
            {
                this.CreateLog(" -> Invalid Byte No.[3] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[4] != PacketHeader[4])
            {
                this.CreateLog(" -> Invalid Byte No.[4] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[5] != PacketHeader[5])
            {
                this.CreateLog(" -> Invalid Byte No.[5] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[6] != PacketHeader[6])
            {
                this.CreateLog(" -> Invalid Byte No.[6] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[7] != PacketHeader[7])
            {
                this.CreateLog(" -> Invalid Byte No.[7] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[8] != PacketHeader[8])
            {
                this.CreateLog(" -> Invalid Byte No.[8] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[9] != PacketHeader[9])
            {
                this.CreateLog(" -> Invalid Byte No.[9] Company ID When Reciver Feedback From PLC !");
                return null;
            }
            ret.LSIS_ID = new byte[10];
            Array.Copy(rxBuf, 0, ret.LSIS_ID, 0, ret.LSIS_ID.Length);
            #endregion

            #region Decode PLC Info
            ret.PLC_Information = new byte[2];
            Array.Copy(rxBuf, 10, ret.PLC_Information, 0, ret.PLC_Information.Length);
            #endregion

            #region Decode CPU Info
            if (rxBuf[12] != PacketHeader[12])
            {
                this.CreateLog(" -> Invalid Byte CPU Information When Reciver Feedback From PLC !");
                return null;
            }
            ret.CPU_Information = rxBuf[12];
            #endregion

            #region Frame Direction
            if (rxBuf[13] != SOURCE_OF_FRAME_FEEDBACK)
            {
                this.CreateLog(" -> Invalid Byte Source Of Frame When Reciver Feedback From PLC !");
                return null;
            }
            ret.Frame_Direction = rxBuf[13];
            #endregion

            #region Frame Order No.
            if (rxBuf[14] != PacketHeader[14])
            {
                this.CreateLog(" -> Invalid Byte No.[0] Frame Order No When Reciver Feedback From PLC !");
                return null;
            }
            else if (rxBuf[15] != PacketHeader[15])
            {
                this.CreateLog(" -> Invalid Byte No.[1] Frame Order No When Reciver Feedback From PLC !");
                return null;
            }
            ret.Frame_Order_No = new byte[2];
            Array.Copy(rxBuf, 14, ret.Frame_Order_No, 0, ret.Frame_Order_No.Length);
            #endregion

            #region Length
            ret.Length = new byte[2];
            Array.Copy(rxBuf, 16, ret.Length, 0, ret.Length.Length);
            #endregion

            #region Position Information
            ret.Position_Information = rxBuf[18];
            #endregion

            #region Check Sum
            var CheckSumFeedback = FENETProtocolLib.ByteCheckSum(rxBuf.ToArray(), 0, 18);
            if (CheckSumFeedback != rxBuf[19])
            {
                this.CreateLog(" -> Invalid Byte CheckSum When Reciver Feedback From PLC !");
                return null;
            }
            ret.Check_Sum = rxBuf[19];
            #endregion

            #region Command
            var listCmd = new byte[] { PacketHeader[20], PacketHeader[21] };
            if (listCmd.SequenceEqual(CMD_READ))
            {
                var CMDFeedback = new byte[2];
                Array.Copy(rxBuf, 20, CMDFeedback, 0, CMDFeedback.Length);
                if (CMDFeedback.SequenceEqual(CMD_FEEDBACK_READ))
                {
                    ret.Command = new byte[2];
                    Array.Copy(CMD_FEEDBACK_READ, 0, ret.Command, 0, ret.Command.Length);
                }
                else
                {
                    this.CreateLog(" -> Invalid Byte Command Read Feedback When Reciver Feedback From PLC !");
                    return null;
                }
            }
            else if (listCmd.SequenceEqual(CMD_WRITE))
            {
                var CMDFeedback = new byte[2];
                Array.Copy(rxBuf, 20, CMDFeedback, 0, CMDFeedback.Length);
                if (CMDFeedback.SequenceEqual(CMD_FEEDBACK_WRITE))
                {
                    ret.Command = new byte[2];
                    Array.Copy(CMD_FEEDBACK_WRITE, 0, ret.Command, 0, ret.Command.Length);
                }
                else
                {
                    this.CreateLog(" -> Invalid Byte Command Write Feedback When Reciver Feedback From PLC !");
                    return null;
                }
            }
            else
            {
                this.CreateLog(" -> Invalid Byte Command When Reciver Feedback From PLC !");
                return null;
            }
            #endregion

            #region Data Type
            if (!(rxBuf[22] == PacketHeader[22]))
            {
                this.CreateLog(" -> Invalid Byte No.[0] Data Type When Reciver Feedback From PLC !");
                return null;
            }
            else if (!(rxBuf[23] == PacketHeader[23]))
            {
                this.CreateLog(" -> Invalid Byte No.[1] Data Type When Reciver Feedback From PLC !");
                return null;
            }
            ret.Data_Type = new byte[2];
            Array.Copy(rxBuf, 22, ret.Data_Type, 0, ret.Data_Type.Length);
            #endregion

            #region  Reserved Area
            ret.Reserved_Area = new byte[2];
            Array.Copy(rxBuf, 24, ret.Reserved_Area, 0, ret.Reserved_Area.Length);
            #endregion

            #region Error Code
            if (!(rxBuf[26] == 0x00))
            {
                this.CreateLog(" -> Invalid Byte No.[0] Error Code When Reciver Feedback From PLC !");
                return null;
            }
            else if (!(rxBuf[27] == 0x00))
            {
                this.CreateLog(" -> Invalid Byte No.[1] Error Code When Reciver Feedback From PLC !");
                return null;
            }
            ret.Error_Status = new byte[2];
            Array.Copy(rxBuf, 26, ret.Error_Status, 0, ret.Error_Status.Length);
            #endregion

            #region Check Is Read/Write Data
            if (CMD_FEEDBACK_WRITE.SequenceEqual(ret.Command))
            {
                return ret;
            }
            #endregion

            #region Variable Length Block
            var listLengthBlockSent = new byte[] { PacketHeader[26], PacketHeader[27] };
            var listLengthBlockReciver = new byte[] { rxBuf[28], rxBuf[29] };
            if (listLengthBlockSent.SequenceEqual(listLengthBlockReciver))
            {
                ret.Variable_Length = new byte[2];
                Array.Copy(rxBuf, 28, ret.Variable_Length, 0, ret.Variable_Length.Length);
            }
            else
            {
                this.CreateLog(" -> Invalid Byte Length Block When Reciver Feedback From PLC !");
                return null;
            }
            #endregion

            #region List Data Count
            int index = 30;
            var numberBlock = BitConverter.ToUInt16(ret.Variable_Length, 0);
            ret.List_Data_Count = Enumerable.Range(0, numberBlock)
                .Select(i =>
                {
                    var dataCount = new byte[2];
                    Array.Copy(rxBuf, index, dataCount, 0, dataCount.Length);
                    var lengthFeedback = BitConverter.ToUInt16(dataCount, 0);
                    index += dataCount.Length + lengthFeedback;
                    return dataCount;
                })
                .ToList();
            #endregion

            #region List Data
            index = 32;
            ret.List_Data = ret.List_Data_Count
                .Select(dataCount =>
                {
                    var lengthFeedback = BitConverter.ToUInt16(dataCount, 0);
                    var data = new byte[lengthFeedback];
                    Array.Copy(rxBuf, index, data, 0, data.Length);
                    index += data.Length + 2;
                    return data;
                })
                .ToList();
            #endregion

            #region Return Data
            return ret;
            #endregion
        }
        #endregion

        #region Read/Write Multi Bits
        public bool ReadBit(byte deviceName, String bitAddress)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, PLC Disconnect .", deviceName, bitAddress));
                        //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, PLC Disconnect .", deviceName, bitAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                        return ret;
                    }
                    if (!FENETProtocolDeviceName.getBitType(deviceName))
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Cannot Read Data Type # Bit Type In Mode ReadBit.", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_CANNOT_TYPE_DIFFRENT_BIT_TYPE_IN_BIT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Cannot Read Data Type # Bit Type In Mode ReadBit.", deviceName, bitAddress));
                        return ret;
                    }
                    if (this.checkHexaDecimaInput(bitAddress))
                    {
                        if (this.checkStringAddr(bitAddress))
                        {
                            var listDevice = new List<FENETProtocolBlock>();
                            listDevice.AddRange(Enumerable.Range(0, 1).Select(i => new FENETProtocolBlock
                            {
                                DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName),
                                Address = this.getAddrBit(bitAddress, i)
                            }));
                            var bitsDataReciver = this.FcReadMultiBlock(deviceName, listDevice);
                            if (bitsDataReciver != null)
                            {
                                if (bitsDataReciver.Count == 1)
                                {
                                    ret = bitsDataReciver[0].SequenceEqual(BYTE_TRUE) ? true : false;
                                }
                                else
                                {
                                    this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 1));
                                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                    Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 1));
                                }
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                                //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                         //   DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                            Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadBit Error: " + ex.Message);
                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_BIT_ERROR));
                    Logger.Create("ReadBit Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool[] ReadMultiBits(byte deviceName, String bitAddress, UInt16 cntBits)
        {
            lock (PlcLocker)
            {
                bool[] ret = Enumerable.Repeat(false, cntBits).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, PLC Disconnect.", deviceName, bitAddress, cntBits));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, PLC Disconnect.", deviceName, bitAddress, cntBits));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Decive Name Input Not Correct.", deviceName, bitAddress, cntBits));
                      //  DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Decive Name Input Not Correct.", deviceName, bitAddress, cntBits));
                        return ret;
                    }
                    if (!FENETProtocolDeviceName.getBitType(deviceName))
                    {
                        this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Cannot Read Data Type # Bit Type In Mode ReadMultiBits.", deviceName, bitAddress, cntBits));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_CANNOT_TYPE_DIFFRENT_BIT_TYPE_IN_BIT));
                        Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddress = {1} : Counts = {2} Failed, Cannot Read Data Type # Bit Type In Mode ReadMultiBits.", deviceName, bitAddress, cntBits));
                        return ret;
                    }
                    if (this.checkHexaDecimaInput(bitAddress))
                    {
                        if (this.checkStringAddr(bitAddress))
                        {
                            var listDevice = new List<FENETProtocolBlock>();
                            listDevice.AddRange(Enumerable.Range(0, cntBits).Select(i => new FENETProtocolBlock
                            {
                                DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName),
                                Address = this.getAddrBit(bitAddress, i)
                            }));
                            var bitsDataReciver = this.FcReadMultiBlock(deviceName, listDevice);
                            if (bitsDataReciver != null)
                            {
                                if (bitsDataReciver.Count == cntBits)
                                {
                                    ret = bitsDataReciver.Select(b => b.SequenceEqual(BYTE_TRUE)).ToArray();
                                }
                                else
                                {
                                    this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2}, Data Response.Length != {2}.", deviceName, bitAddress, cntBits));
                                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                    Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Data Response.Length != {2}.", deviceName, bitAddress, cntBits));
                                }
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Data Response == Null.", deviceName, bitAddress, cntBits));
                                //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Data Response == Null.", deviceName, bitAddress, cntBits));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Bits Address Not Correct.", deviceName, bitAddress, cntBits));
                            //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Bits Address Not Correct.", deviceName, bitAddress, cntBits));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Bits Address Not Correct.", deviceName, bitAddress, cntBits));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiBits DeviceName = {0} : BitAddressStart = {1} : Counts = {2} Failed, Bits Address Not Correct.", deviceName, bitAddress, cntBits));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiBits Error: " + ex.Message);
                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_BITS_ERROR));
                    Logger.Create("ReadMultiBits Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteBit(byte deviceName, String bitAddress, bool dataBitWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteBit DeviceName = {0} : BitAddress = {1} Failed, PLC Disconnect .", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteBit DeviceName = {0} : BitAddress = {1} Failed, PLC Disconnect .", deviceName, bitAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                        return ret;
                    }
                    if (!FENETProtocolDeviceName.getBitType(deviceName))
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Cannot Read Data Type # Bit Type In Mode WriteBit.", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_CANNOT_TYPE_DIFFRENT_BIT_TYPE_IN_BIT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Cannot Read Data Type # Bit Type In Mode WriteBit.", deviceName, bitAddress));
                        return ret;
                    }
                    if (this.checkHexaDecimaInput(bitAddress))
                    {
                        if (this.checkStringAddr(bitAddress))
                        {
                            var listDevice = new List<FENETProtocolBlock>();
                            listDevice.AddRange(Enumerable.Range(0, 1).Select(i => new FENETProtocolBlock
                            {
                                DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName),
                                Address = this.getAddrBit(bitAddress, i),
                                DataWrite = dataBitWrite ? BYTE_WRITE_TRUE : BYTE_WRITE_FALSE
                            }));
                            var bitsDataReciver = this.FcWriteMultiBlock(deviceName, listDevice);
                            if (bitsDataReciver != null)
                            {
                                if (bitsDataReciver.Length == 2)
                                {
                                    ret = bitsDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                                }
                                else
                                {
                                    this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 2));
                                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                    Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 2));
                                }
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                                //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                            //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                            Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                        //DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadBit DeviceName = {0} : BitAddress = {1} Failed, Bit Address Not Correct.", deviceName, bitAddress));
                    }
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadBit Error: " + ex.Message);
                   // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_BIT_ERROR));
                    Logger.Create("ReadBit Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiBits(byte deviceName, String bitAddress, List<bool> dataWriteMultiBits)
        {
            lock (PlcLocker)
            {
                var ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, PLC Disconnect.", deviceName, bitAddress));
                       // DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddress = {1} Failed, PLC Disconnect.", deviceName, bitAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, bitAddress));
                        return ret;
                    }
                    if (!FENETProtocolDeviceName.getBitType(deviceName))
                    {
                        this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Cannot Write Data Type # Bit Type In Mode WriteMultiBits.", deviceName, bitAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_CANNOT_TYPE_DIFFRENT_BIT_TYPE_IN_BIT));
                        Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Cannot Write Data Type # Bit Type In Mode WriteMultiBits.", deviceName, bitAddress));
                        return ret;
                    }
                    if (this.checkHexaDecimaInput(bitAddress))
                    {
                        if (this.checkStringAddr(bitAddress))
                        {
                            var listDevice = new List<FENETProtocolBlock>();
                            listDevice.AddRange(Enumerable.Range(0, dataWriteMultiBits.Count).Select(i => new FENETProtocolBlock
                            {
                                DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName),
                                Address = this.getAddrBit(bitAddress, i),
                                DataWrite = dataWriteMultiBits[i] ? BYTE_WRITE_TRUE : BYTE_WRITE_FALSE
                            }));
                            var bitsDataReciver = this.FcWriteMultiBlock(deviceName, listDevice);
                            if (bitsDataReciver != null)
                            {
                                if (bitsDataReciver.Length == 2)
                                {
                                    ret = bitsDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                                }
                                else
                                {
                                    this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 2));
                                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                    Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, bitAddress, 2));
                                }
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Data Response == Null.", deviceName, bitAddress));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Bits Address Not Correct.", deviceName, bitAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Bits Address Not Correct.", deviceName, bitAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Bits Address Not Correct.", deviceName, bitAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiBits DeviceName = {0} : BitAddressStart = {1} Failed, Bist Address Not Correct.", deviceName, bitAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiBits Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_BITS_ERROR));
                    Logger.Create("WriteMultiBits Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi Bytes
        public byte ReadByte(byte deviceName, String byteAddress)
        {
            lock (PlcLocker)
            {
                byte ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, PLC Disconnect.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, PLC Disconnect.", deviceName, byteAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteAddress);
                        listDevice.NumberOfData = new byte[] { 0x01, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = byteDataReciver[0][0];
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadByte DeviceName = {0} : ByteAddress = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadByte Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_BYTE_ERROR));
                    Logger.Create("ReadByte Error: " + ex.Message);
                }
                return ret;
            }
        }
        public byte[] ReadMultiBytes(byte deviceName, String byteAddress, UInt16 cntBytes)
        {
            lock (PlcLocker)
            {
                byte[] ret = Enumerable.Repeat((byte)0x00, cntBytes).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, byteAddress, cntBytes));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, byteAddress, cntBytes));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, byteAddress, cntBytes));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, byteAddress, cntBytes));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteAddress, out StartAddressRead) && (cntBytes > 0))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntBytes);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntBytes)
                            {
                                ret = byteDataReciver.SelectMany(arr => arr).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Data Response.Length != {2}.", deviceName, byteAddress, cntBytes));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Data Response.Length != {2}.", deviceName, byteAddress, cntBytes));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, byteAddress, cntBytes));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, byteAddress, cntBytes));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Byte Address Not Correct.", deviceName, byteAddress, cntBytes));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiBytes DeviceName = {0} : ByteAddressStart = {1} : Count = {2} Failed, Byte Address Not Correct.", deviceName, byteAddress, cntBytes));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiBytes Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_BYTES_ERROR));
                    Logger.Create("ReadMultiBytes Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteByte(byte deviceName, String byteAddress, byte dataByteWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, PLC Disconnect .", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, PLC Disconnect .", deviceName, byteAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        return ret;
                    }

                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteAddress);
                        listDevice.NumberOfData = new byte[] { 0x01, 00 };
                        listDevice.DataWrite = new byte[] { dataByteWrite };

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteByte DeviceName = {0} : ByteAddress = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteByte Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_BYTE_ERROR));
                    Logger.Create("WriteByte Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiBytes(byte deviceName, String byteAddress, List<byte> dataWriteMultiBytes)
        {
            lock (PlcLocker)
            {
                var ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, PLC Disconnect.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, PLC Disconnect.", deviceName, byteAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, byteAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteMultiBytes.ToArray().Count());
                        listDevice.DataWrite = dataWriteMultiBytes.ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, byteAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Data Response == Null.", deviceName, byteAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiBytes DeviceName = {0} : ByteAddressStart = {1} Failed, Byte Address Not Correct.", deviceName, byteAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiBytes Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_BYTES_ERROR));
                    Logger.Create("WriteMultiBytes Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi Word UInt16
        public UInt16 ReadWord_UInt16(byte deviceName, String wordAddress)
        {
            lock (PlcLocker)
            {
                UInt16 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = new byte[] { 0x02, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToUInt16(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadWord_UInt16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_WORD_UINT16_ERROR));
                    Logger.Create("ReadWord_UInt16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteWord_UInt16(byte deviceName, String wordAddress, UInt16 dataWordWrite)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = new byte[] { 0x02, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.UInt16ToByte(dataWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteWord_UInt16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteWord_UInt16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_WORD_UINT16_ERROR));
                    Logger.Create("WriteWord_UInt16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public UInt16[] ReadMultiWords_UInt16(byte deviceName, String wordAddress, UInt16 cntWords)
        {
            lock (PlcLocker)
            {
                UInt16[] ret = Enumerable.Repeat((UInt16)00, cntWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, wordAddress, cntWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, wordAddress, cntWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead) && cntWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntWords * 2);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1 && byteDataReciver[0].Length == cntWords * 2)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 2);
                                ret = Enumerable.Range(0, cntWords).Select(i => FENETProtocolLib.BytesToUInt16(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed: Data Response.Length != {2}.", deviceName, wordAddress, cntWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed: Data Response.Length != {2}.", deviceName, wordAddress, cntWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, cntWords));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, cntWords));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, wordAddress, cntWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiWords_UInt16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_WORDS_UINT16_ERROR));
                    Logger.Create("ReadMultiWords_UInt16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiWords_UInt16(byte deviceName, String wordAddress, List<UInt16> dataWriteMultiWords_Uint16)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, PLC Disconnect.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, PLC Disconnect.", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteMultiWords_Uint16.ToArray().Count() * 2);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteMultiWords_Uint16.Count).Select(i => FENETProtocolLib.UInt16ToByte(dataWriteMultiWords_Uint16[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, dataWriteMultiWords_Uint16.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, dataWriteMultiWords_Uint16.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiWords_UInt16 DeviceName = {0} : WordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiWords_UInt16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_WORDS_UINT16_ERROR));
                    Logger.Create("WriteMultiWords_UInt16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi Word Int16
        public Int16 ReadWord_Int16(byte deviceName, String wordAddress)
        {
            lock (PlcLocker)
            {
                Int16 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = new byte[] { 0x02, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToInt16(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadWord_Int16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_WORD_INT16_ERROR));
                    Logger.Create("ReadWord_Int16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteWord_Int16(byte deviceName, String wordAddress, Int16 dataWordWrite)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, PLC Disconnect .", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = new byte[] { 0x02, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.Int16ToByte(dataWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Data Response == Null.", deviceName, wordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteWord_Int16 DeviceName = {0} : WordAddress = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteWord_Int16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_WORD_INT16_ERROR));
                    Logger.Create("WriteWord_Int16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public Int16[] ReadMultiWords_Int16(byte deviceName, String wordAddress, UInt16 cntWords)
        {
            lock (PlcLocker)
            {
                Int16[] ret = Enumerable.Repeat((Int16)00, cntWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, wordAddress, cntWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, wordAddress, cntWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead) && cntWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntWords * 2);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1 && byteDataReciver[0].Length == cntWords * 2)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 2);
                                ret = Enumerable.Range(0, cntWords).Select(i => FENETProtocolLib.BytesToInt16(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, cntWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, cntWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, cntWords));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, cntWords));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, wordAddress, cntWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, wordAddress, cntWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiWords_Int16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_WORDS_INT16_ERROR));
                    Logger.Create("ReadMultiWords_Int16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiWords_Int16(byte deviceName, String wordAddress, List<Int16> dataWriteMultiWord_Int16)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, PLC Disconnect.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, PLC Disconnect.", deviceName, wordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, wordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(wordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(wordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteMultiWord_Int16.ToArray().Count() * 2);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteMultiWord_Int16.Count).Select(i => FENETProtocolLib.Int16ToByte(dataWriteMultiWord_Int16[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, wordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, dataWriteMultiWord_Int16.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, wordAddress, dataWriteMultiWord_Int16.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiWords_Int16 DeviceName = {0} : WordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, wordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiWords_Int16 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_WORDS_INT16_ERROR));
                    Logger.Create("WriteMultiWords_Int16 Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi Double Word UInt32
        public UInt32 ReadDoubleWord_UInt32(byte deviceName, String doubleWordAddress)
        {
            lock (PlcLocker)
            {
                UInt32 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x04, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToUInt32(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadDoubleWord_UInt32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_DOUBLE_WORD_UINT32_ERROR));
                    Logger.Create("ReadDoubleWord_UInt32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteDoubleWord_UInt32(byte deviceName, String doubleWordAddress, UInt32 dataDoubleWordWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x04, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.UInt32ToByte(dataDoubleWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteDoubleWord_UInt32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteDoubleWord_UInt32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_DOUBLE_WORD_UINT32_ERROR));
                    Logger.Create("WriteDoubleWord_UInt32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public UInt32[] ReadMultiDoubleWords_UInt32(byte deviceName, String doubleWordAddress, UInt16 cntDoubleWords)
        {
            lock (PlcLocker)
            {
                UInt32[] ret = Enumerable.Repeat((UInt32)00, cntDoubleWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, doubleWordAddress, cntDoubleWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead) && cntDoubleWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntDoubleWords * 4);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntDoubleWords * 4 && byteDataReciver.Count == 1)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 4);
                                ret = Enumerable.Range(0, cntDoubleWords).Select(i => FENETProtocolLib.BytesToUInt32(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, cntDoubleWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, cntDoubleWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, cntDoubleWords));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, cntDoubleWords));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiDoubleWords_UInt32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_DOUBLE_WORDS_UINT32_ERROR));
                    Logger.Create("ReadMultiDoubleWords_UInt32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiDoubleWords_UInt32(byte deviceName, String doubleWordAddress, List<UInt32> dataWriteMultiWord_UInt32)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteMultiWord_UInt32.ToArray().Count() * 4);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteMultiWord_UInt32.Count).Select(i => FENETProtocolLib.UInt32ToByte(dataWriteMultiWord_UInt32[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, dataWriteMultiWord_UInt32.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, dataWriteMultiWord_UInt32.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_UInt32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiDoubleWords_UInt32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_DOUBLE_WORDS_UINT32_ERROR));
                    Logger.Create("WriteMultiDoubleWords_UInt32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi Double Word Int32
        public Int32 ReadDoubleWord_Int32(byte deviceName, String doubleWordAddress)
        {
            lock (PlcLocker)
            {
                Int32 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x04, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToInt32(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadDoubleWord_Int32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_DOUBLE_WORD_INT32_ERROR));
                    Logger.Create("ReadDoubleWord_Int32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteDoubleWord_Int32(byte deviceName, String doubleWordAddress, Int32 dataDoubleWordWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x04, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.Int32ToByte(dataDoubleWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, doubleWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteDoubleWord_Int32 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteDoubleWord_Int32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_DOUBLE_WORD_INT32_ERROR));
                    Logger.Create("WriteDoubleWord_Int32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public Int32[] ReadMultiDoubleWords_Int32(byte deviceName, String doubleWordAddress, UInt16 cntDoubleWords)
        {
            lock (PlcLocker)
            {
                Int32[] ret = Enumerable.Repeat((Int32)00, cntDoubleWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, doubleWordAddress, cntDoubleWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead) && cntDoubleWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntDoubleWords * 4);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntDoubleWords * 4 && byteDataReciver.Count == 1)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 4);
                                ret = Enumerable.Range(0, cntDoubleWords).Select(i => FENETProtocolLib.BytesToInt32(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, cntDoubleWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, cntDoubleWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, cntDoubleWords));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, cntDoubleWords));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, DoubleWord Address Not Correct.", deviceName, doubleWordAddress, cntDoubleWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiDoubleWords_Int32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_DOUBLE_WORDS_INT32_ERROR));
                    Logger.Create("ReadMultiDoubleWords_Int32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiDoubleWords_Int32(byte deviceName, String doubleWordAddress, List<Int32> dataWriteMultiWords_Int32)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, doubleWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(doubleWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(doubleWordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(doubleWordAddress.ToArray().Count() * 4);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteMultiWords_Int32.Count).Select(i => FENETProtocolLib.Int32ToByte(dataWriteMultiWords_Int32[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, doubleWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, dataWriteMultiWords_Int32.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, doubleWordAddress, dataWriteMultiWords_Int32.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, doubleWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiDoubleWords_Int32 DeviceName = {0} : DoubleWordAddressStart = {1} Failed, Word Address Not Correct.", deviceName, doubleWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiDoubleWords_Int32 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_DOUBLE_WORDS_INT32_ERROR));
                    Logger.Create("WriteMultiDoubleWords_Int32 Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi LongWord_UInt64
        public UInt64 ReadLongWord_UInt64(byte deviceName, String longWordAddress)
        {
            lock (PlcLocker)
            {
                UInt64 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x08, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToUInt64(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadLongWord_UInt64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_LONG_WORD_UINT64_ERROR));
                    Logger.Create("ReadLongWord_UInt64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteLongWord_UInt64(byte deviceName, String longWordAddress, UInt64 dataLongWordWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, dataLongWordWrite));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, dataLongWordWrite));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x08, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.UInt64ToByte(dataLongWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, dataLongWordWrite, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteLongWord_UInt64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, dataLongWordWrite, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteLongWord_UInt64 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, dataLongWordWrite));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteLongWord_UInt64 DeviceName = {0} : DoubleWordAddress = {1} Failed, Data Response == Null.", deviceName, dataLongWordWrite));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteLongWord_UInt64 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteLongWord_UInt64 DeviceName = {0} : DoubleWordAddress = {1} Failed, DoubleWord Address Not Correct.", deviceName, dataLongWordWrite));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteLongWord_UInt64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_LONG_WORD_UINT64_ERROR));
                    Logger.Create("WriteLongWord_UInt64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public UInt64[] ReadMultiLongWords_UInt64(byte deviceName, String longWordAddress, UInt16 cntLongWords)
        {
            lock (PlcLocker)
            {
                UInt64[] ret = Enumerable.Repeat((UInt64)00, cntLongWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, longWordAddress, cntLongWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress, cntLongWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead) && cntLongWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntLongWords * 8);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntLongWords * 8 && byteDataReciver.Count == 1)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 8);
                                ret = Enumerable.Range(0, cntLongWords).Select(i => FENETProtocolLib.BytesToUInt64(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed: Data Response.Length != {2}.", deviceName, longWordAddress, cntLongWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed: Data Response.Length != {2}.", deviceName, longWordAddress, cntLongWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, cntLongWords));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, cntLongWords));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Word Address Not Correct.", deviceName, longWordAddress, cntLongWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiLongWords_UInt64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_LONG_WORD_UINT64_ERROR));
                    Logger.Create("ReadMultiLongWords_UInt64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiLongWords_UInt64(byte deviceName, String longWordAddress, List<UInt64> dataWriteLongWords_UInt64)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteLongWords_UInt64.ToArray().Count() * 8);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteLongWords_UInt64.Count).Select(i => FENETProtocolLib.UInt64ToByte(dataWriteLongWords_UInt64[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, dataWriteLongWords_UInt64.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, dataWriteLongWords_UInt64.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiLongWords_UInt64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiLongWords_UInt64_Continuous Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_LONG_WORD_UINT64_ERROR));
                    Logger.Create("WriteMultiLongWords_UInt64_Continuous Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write Multi LongWord_Int64
        public Int64 ReadLongWord_Int64(byte deviceName, String longWordAddress)
        {
            lock (PlcLocker)
            {
                Int64 ret = 0;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x08, 0x00 };

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Count == 1)
                            {
                                ret = FENETProtocolLib.BytesToInt64(byteDataReciver[0]);
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 1));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 1));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: Data Response == Null.", deviceName, longWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: Data Response == Null.", deviceName, longWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: LongWord Address Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: LongWord Address Not Correct.", deviceName, longWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadLongWord_Int64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_LONG_WORD_INT64_ERROR));
                    Logger.Create("ReadLongWord_Int64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteLongWord_Int64(byte deviceName, String longWordAddress, Int64 dataLongWordWrite)
        {
            lock (PlcLocker)
            {
                Boolean ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, PLC Disconnect.", deviceName, dataLongWordWrite));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: Decive Name Input Not Correct.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed: Decive Name Input Not Correct.", deviceName, dataLongWordWrite));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = new byte[] { 0x08, 0x00 };
                        listDevice.DataWrite = FENETProtocolLib.Int64ToByte(dataLongWordWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, dataLongWordWrite, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddress = {1} Failed, Data Response.Length != {2}.", deviceName, dataLongWordWrite, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, dataLongWordWrite));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteLongWord_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, dataLongWordWrite));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteLongWord_Int64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_LONG_WORD_INT64_ERROR));
                    Logger.Create("WriteLongWord_Int64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public Int64[] ReadMultiLongWords_Int64(byte deviceName, String longWordAddress, UInt16 cntLongWords)
        {
            lock (PlcLocker)
            {
                Int64[] ret = Enumerable.Repeat((Int64)00, cntLongWords).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, longWordAddress, cntLongWords));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress, cntLongWords));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead) && cntLongWords > 0)
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntLongWords * 8);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntLongWords * 8 && byteDataReciver.Count == 1)
                            {
                                var listResult = FENETProtocolLib.BytesToList(byteDataReciver[0], 8);
                                ret = Enumerable.Range(0, cntLongWords).Select(i => FENETProtocolLib.BytesToInt64(listResult[i])).ToArray();
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, cntLongWords));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, cntLongWords));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response == Null.", deviceName, longWordAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, LongWord Address Not Correct.", deviceName, longWordAddress, cntLongWords));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, LongWord Address Not Correct.", deviceName, longWordAddress, cntLongWords));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadMultiLongWords_Int64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_MULTI_LONG_WORD_INT64_ERROR));
                    Logger.Create("ReadMultiLongWords_Int64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        public bool WriteMultiLongWords_Int64(byte deviceName, String longWordAddress, List<Int64> dataWriteLongWords_Int64)
        {
            lock (PlcLocker)
            {
                bool ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, PLC Disconnect.", deviceName, longWordAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, longWordAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(longWordAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr_Continuous(longWordAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(dataWriteLongWords_Int64.ToArray().Count() * 8);
                        listDevice.DataWrite = Enumerable.Range(0, dataWriteLongWords_Int64.Count).Select(i => FENETProtocolLib.Int64ToByte(dataWriteLongWords_Int64[i])).SelectMany(bytes => bytes).ToArray();

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, longWordAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, dataWriteLongWords_Int64.Count));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, longWordAddress, dataWriteLongWords_Int64.Count));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteMultiLongWords_Int64 DeviceName = {0} : LongWordAddressStart = {1} Failed, LongWord Address Not Correct.", deviceName, longWordAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteMultiLongWords_Int64 Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_MULTI_LONG_WORD_UINT64_ERROR));
                    Logger.Create("WriteMultiLongWords_Int64 Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion

        #region Read/Write String
        public String ReadString(byte deviceName, String byteStringAddress, UInt16 cntCharactor)
        {
            lock (PlcLocker)
            {
                var retStr = "";
                byte[] ret = Enumerable.Repeat((byte)0x00, cntCharactor).ToArray();
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, byteStringAddress, cntCharactor));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, PLC Disconnect.", deviceName, byteStringAddress, cntCharactor));
                        return retStr;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, byteStringAddress, cntCharactor));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Decive Name Input Not Correct.", deviceName, byteStringAddress, cntCharactor));
                        return retStr;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteStringAddress, out StartAddressRead) && (cntCharactor > 0))
                    {
                        // Data Input
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteStringAddress);
                        listDevice.NumberOfData = BitConverter.GetBytes(cntCharactor);

                        var byteDataReciver = this.FcReadMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver[0].Length == cntCharactor)
                            {
                                ret = byteDataReciver.SelectMany(arr => arr).ToArray();
                                retStr = Encoding.ASCII.GetString(ret);
                                if (retStr.EndsWith("\r\n") || retStr.EndsWith("\r") || retStr.EndsWith("\n") || retStr.EndsWith("\0"))
                                {
                                    retStr = retStr.TrimEnd(new char[] { '\r', '\n', '\0' });
                                }
                            }
                            else
                            {
                                this.CreateLog(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Data Response.Length != {2}.", deviceName, byteStringAddress, cntCharactor));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Data Response.Length != {2}.", deviceName, byteStringAddress, cntCharactor));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, byteStringAddress, cntCharactor));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Data Response == Null.", deviceName, byteStringAddress, cntCharactor));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Byte String Address Not Correct.", deviceName, byteStringAddress, cntCharactor));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("ReadString DeviceName = {0} : ByteStringAddressStart = {1} : Count = {2} Failed, Byte String Address Not Correct.", deviceName, byteStringAddress, cntCharactor));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("ReadString Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_READ_STRING_ERROR));
                    Logger.Create("ReadString Error: " + ex.Message);
                }
                return retStr;
            }
        }
        public bool WriteString(byte deviceName, String byteStringAddress, String StrWrite)
        {
            lock (PlcLocker)
            {
                var ret = false;
                try
                {
                    if (!this.TcpClient.Connected)
                    {
                        this.CreateLog(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, PLC Disconnect.", deviceName, byteStringAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_PLC_IS_NOT_CONNECT));
                        Logger.Create(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, PLC Disconnect.", deviceName, byteStringAddress));
                        return ret;
                    }
                    if (FENETProtocolDeviceName.getListByteAddr(deviceName) == null)
                    {
                        this.CreateLog(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, byteStringAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DEVICE_NAME_NOT_CORRECT));
                        Logger.Create(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Decive Name Input Not Correct.", deviceName, byteStringAddress));
                        return ret;
                    }
                    UInt32 StartAddressRead;
                    if (UInt32.TryParse(byteStringAddress, out StartAddressRead))
                    {
                        // Data Write 
                        var listDevice = new FENETProtocolBlock();
                        listDevice.DeviceName = FENETProtocolDeviceName.getListByteAddr(deviceName);
                        listDevice.Address = this.getAddr(byteStringAddress);
                        listDevice.NumberOfData = FENETProtocolLib.GetLength(ASCIIEncoding.ASCII.GetBytes(StrWrite).Count());
                        listDevice.DataWrite = ASCIIEncoding.ASCII.GetBytes(StrWrite);

                        var byteDataReciver = this.FcWriteMultiBlock_Continuous(deviceName, listDevice);
                        if (byteDataReciver != null)
                        {
                            if (byteDataReciver.Length == 2)
                            {
                                ret = byteDataReciver.SequenceEqual(BYTE_NO_ERROR) ? true : false;
                            }
                            else
                            {
                                this.CreateLog(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, byteStringAddress, 2));
                                DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                                Logger.Create(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Data Response.Length != {2}.", deviceName, byteStringAddress, 2));
                            }
                        }
                        else
                        {
                            this.CreateLog(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Data Response == Null.", deviceName, byteStringAddress));
                            DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_DATA_RESPONSE_NOT_CORRECT));
                            Logger.Create(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Data Response == Null.", deviceName, byteStringAddress));
                        }
                    }
                    else
                    {
                        this.CreateLog(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Byte String Address Not Correct.", deviceName, byteStringAddress));
                        DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_ADDRESS_NOT_CORRECT));
                        Logger.Create(String.Format("WriteString DeviceName = {0} : ByteStringAddressStart = {1} Failed, Byte String Address Not Correct.", deviceName, byteStringAddress));
                    }
                }
                catch (Exception ex)
                {
                    this.CreateLog("WriteString Error: " + ex.Message);
                    DbWrite.createEvent(new EventLog(EventLog.EV_FENETPROTOCOL_WRITE_STRING_ERROR));
                    Logger.Create("WriteString Error: " + ex.Message);
                }
                return ret;
            }
        }
        #endregion
        private List<byte> Header_Company_ID() // 10 byte
        {
            var ret = new List<Byte>();
            ret.AddRange(this.Settings.Is_LSIS_XGT ? this.BYTE_LSIS : this.BYTE_LGIS);
            return ret;
        }
        private List<byte> Header_PLC_Info() // 2 byte
        {
            var ret = new List<byte>();
            ret.Add(PLC_INFOR);
            ret.Add(PLC_INFOR);

            return ret;
        }
        private List<byte> Header_CPU_Info() // 1 byte
        {
            var ret = new List<byte>();
            ret.Add(FENETProtocolDeviceName.getByteCPU(this.Settings.CPU_Infor));

            return ret;
        }
        private List<byte> Header_Source_Of_Frame() // 1 byte
        {
            var ret = new List<byte>();
            ret.Add(SOURCE_OF_FRAME);

            return ret;
        }
        private List<byte> Header_Invoke_ID() // 2 byte
        {
            var ret = new List<byte>();
            ret.Add(INVOKE_ID);
            ret.Add(INVOKE_ID);

            return ret;
        }
        private byte Header_FEnet_Position() // 1 byte
        {
            byte ret = 0;

            var slotNo = (byte)this.Settings.SLOT_No; // Slot No
            ret |= slotNo;
            var baseNo = (byte)(this.Settings.BASE_No << 4); // Base No
            ret |= baseNo;
            return ret;
        }
        private List<byte> Header_Reserved() // 2 byte
        {
            var ret = new List<byte>();
            ret.Add(RESERVED);
            ret.Add(RESERVED);

            return ret;
        }

        private void Send(byte[] txBuf)
        {
            this.TcpClient.Send(txBuf); // Sent Data To PLC

            var hexString = string.Concat(txBuf.Select(b => b.ToString("X2")));
            this.CreateLog(String.Format("TX({0}) :{1}", txBuf.Length, hexString));

            this.WriteLog(true, txBuf, hexString);
        }

        private async void CreateLog(String msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                if (!string.Equals(msg, this.lastLog))
                {
                    this.lastLog = msg;
                    if (msg.EndsWith("\r\n") || msg.EndsWith("\r") || msg.EndsWith("\n"))
                    {
                        msg = msg.TrimEnd(new char[] { '\r', '\n' });
                    }
                    Task tskCreateLog = Task.Run(() =>
                    {
                        if (this.LogCallback != null)
                        {
                            this.LogCallback(String.Format("FENETProtocol {0}: {1}", this.Settings.PLCName, msg));
                        }
                    });
                    await Task.WhenAny(new List<Task> { tskCreateLog });
                }
            }
        }

        private async void WriteLog(bool sentData, byte[] txBuf, String log)
        {
            Task tskWriteLog = Task.Run(() =>
            {
                if (this.Settings.Is_Enable_Log)
                {
                    var logger = new FENETProtocolLogger();
                    if (sentData)
                    {
                        logger.CreateTxLog(String.Format("TX({0}) :{1}", txBuf.Length, log));
                    }
                    else
                    {
                        logger.CreateRxLog(String.Format("RX({0}) :{1}", txBuf.Length, log));
                    }
                }
            });
            await Task.WhenAny(new List<Task> { tskWriteLog });
        }
    }
}
