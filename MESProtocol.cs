using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisionInspection;

namespace AutoLaserCuttingInput
{
    class MESProtocolPacket
    {
        public string EquipmentId { get; set; }
        public string Status { get; set; }
        public string LotID { get; set; }
        public List<string> DataQrCode { get; set; }
        public string CheckSum { get; set; }
    }
    class MESProtocol
    {
        // Logger
        private static MyLogger logger = new MyLogger("MESComm");

        private const String E001 = "E001"; // MES Ask Ready OK ?
        private const String E002 = "E002"; // Feedback MES READYOK/ READYNG 

        private const String E091 = "E091"; // Data Parameter Client Send
        private const String E092 = "E092"; // Data Parameter Client Reciver

        private byte[] equiqmentId = ASCIIEncoding.ASCII.GetBytes("XBND002  "); // Get Byte[] Of Equipment
        private byte[] lotNo = ASCIIEncoding.ASCII.GetBytes("P91101   ");// Get Byte[] Of LotNo

       

        private bool IsRunning = false; // Is Running

        // TCP server:
        private TcpListener TCPListener;

        private Thread TCPManagerThread;
        private TcpClient TCPClientMES;

        private Boolean CancelFlag = false;

        private LogCallback LogCallback; // Write Log
        private event DlOneParam ConnectionChanged; // Conenction Change

        // Event MES Reciver Data
        public delegate void RxPacketHandler(MESProtocolPacket packet);
        public event RxPacketHandler PacketReceived;  // 

        private String lastLog = "";

        // Check Isconnected Of MES
        public Boolean IsConnected
        {
            get
            {
                if (this.TCPClientMES != null) // Check Socket Is Null
                {
                    try
                    {
                        if (this.TCPClientMES.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (this.TCPClientMES.Client.Receive(buff, SocketFlags.Peek) == 0)
                                return false;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        // Construction
        public MESProtocol(String ipAddr, int port)
        {
            this.TCPListener = new TcpListener(IPAddress.Parse(ipAddr), port);
        }
        #region Setup Equipment ID And Lot ID
        public void Setup(String eqId, string lotID)
        {
            try
            {
                this.SetupEquipment(eqId);
                this.SetupLotID(lotID);
            }
            catch (Exception ex)
            {
                logger.Create("Setup Equipment And LotID Error: " + ex.Message);
            }
        }
        private void SetupEquipment(String eqId)
        {
            if (!String.IsNullOrEmpty(eqId))
            {
                var arr = eqId.ToCharArray();
                int sz = arr.Length;
                if (sz > 9) { sz = 9; }
                this.equiqmentId = new byte[9];
                for (int i = 0; i < sz; i++)
                {
                    this.equiqmentId[i] = (byte)arr[i];
                }
                for (int i = sz; i < 9; i++)
                {
                    this.equiqmentId[i] = 0;
                }
            }
        }
        private byte[] ReturnEquipment(String eqId)
        {
            var ret = new byte[9];
            if (!String.IsNullOrEmpty(eqId))
            {
                var arr = eqId.ToCharArray();
                int sz = arr.Length;
                if (sz > 9) { sz = 9; }
                for (int i = 0; i < sz; i++)
                {
                    ret[i] = (byte)arr[i];
                }
                for (int i = sz; i < 9; i++)
                {
                    ret[i] = 0;
                }
            }
            return ret;
        }
        //  CAI ĐẶT VAFO
        private void SetupLotID(string lotID)
        {
            if (!String.IsNullOrEmpty(lotID))
            {
                var arr = lotID.ToCharArray();
                int sz = arr.Length;
                if (sz > 9) { sz = 9; }
                this.lotNo = new byte[9];
                for (int i = 0; i < sz; i++)
                {
                    this.lotNo[i] = (byte)arr[i];
                }
                for (int i = sz; i < 9; i++)
                {
                    this.lotNo[i] = 0;
                }
            }
        } // CHECK LOTID NHẬN VỀ
        private byte[] ReturnLot(String lotID)
        {
            var ret = new byte[9];
            if (!String.IsNullOrEmpty(lotID))
            {
                var arr = lotID.ToCharArray();
                int sz = arr.Length;
                if (sz > 9) { sz = 9; }
                for (int i = 0; i < sz; i++)
                {
                    ret[i] = (byte)arr[i];
                }
                for (int i = sz; i < 9; i++)
                {
                    ret[i] = 0;
                }
            }
            return ret;
        }
        #endregion

        #region Check MES Ready E001

        // TRUÓC KHI GUI PHAI CHECK MES READY
        public Boolean CheckMCSReady(int timeout) // Check MES Ready
        {
            this.isReady = false;

            this.Send_READY_REQ();

            for (int i = 0; i < 10; i++)
            {
                if (this.isReady)
                {
                    break;
                }
                Thread.Sleep(timeout / 10);
            }
            if (!this.isReady)
            {
                DbWrite.createEvent(new EventLog(EventLog.EV_MES_READY_TIMEOUT));
            }
            return this.isReady;
        }
        private void Send_READY_REQ()
        {
            try
            {
                if (!this.IsConnected)
                {
                    logger.Create(String.Format(" -> TCP Connection Not Ready -> Discard Sending READY_REQ!"));
                    return;
                }
                var packet = new List<byte>();
                packet.AddRange(this.equiqmentId);
                packet.AddRange(ASCIIEncoding.ASCII.GetBytes(E001));
                var txBuf = packet.ToArray();
                logger.Create(String.Format("MES.SEND: " + ASCIIEncoding.ASCII.GetString(txBuf)));
                this.TCPClientMES.Client.Send(txBuf);

                // Write Log
                var arr = new byte[txBuf.Length];
                Array.Copy(txBuf, 0, arr, 0, arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == 0x00) { arr[i] = 0x20; }
                }
                this.CreateLog(ASCIIEncoding.ASCII.GetString(arr));

            }
            catch (Exception ex)
            {
                logger.Create("Send_READY_REQ Error: " + ex.Message);
            }
        }
        private void Return_READY_REQ()
        {
            try
            {
                if (!this.IsConnected)
                {
                    logger.Create(String.Format(" -> TCP Connection Not Ready -> Discard Sending Return READY_REQ!"));
                    return;
                }
                var packet = new List<byte>();
                packet.AddRange(this.equiqmentId);
                packet.AddRange(ASCIIEncoding.ASCII.GetBytes(E002));
                var txBuf = packet.ToArray();
                logger.Create(String.Format("MES.SEND: " + ASCIIEncoding.ASCII.GetString(txBuf)));
                this.TCPClientMES.Client.Send(txBuf);

                // Write Log
                var arr = new byte[txBuf.Length];
                Array.Copy(txBuf, 0, arr, 0, arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == 0x00) { arr[i] = 0x20; }
                }
                this.CreateLog(ASCIIEncoding.ASCII.GetString(arr));
            }
            catch (Exception ex)
            {
                logger.Create("Send Return READY_REQ Error: " + ex.Message);
            }
        }
        #endregion

        #region Check MES Result E091
        public List<String> CheckQRCodes(DataCheckPKG[] data, int timeout)
        {
            this.CancelFlag = false;

            this.qrResults = new List<String>();
            this.qrReplied = false;
            this.Send_QRCODE_REQ(data);

            // Wait for result:
            const int CHECK_TIME = 500; // 0.5s
            while ((!this.CancelFlag) && (timeout > 0))
            {
                if (this.qrReplied)
                {
                    break;
                }
                timeout -= CHECK_TIME;
                Thread.Sleep(CHECK_TIME);
            }

            if (this.qrReplied)
            {
                return this.qrResults;
            }
            DbWrite.createEvent(new EventLog(EventLog.EV_MES_CHECK_TIMEOUT));
            return null;
        }
        private void Send_QRCODE_REQ(DataCheckPKG[] dataSent)
        {
            try
            {
                if (!this.IsConnected)
                {
                    logger.Create(" -> TCP MES Connection Not Ready -> Discard Sending QRCODE_REQ!");
                    return;
                }
                // Create Payload:
                var payload = new List<byte>(0);
                for (int i = 0; i < dataSent.Length; i++)
                {
                    if (string.Equals(dataSent[i].QrCodePKG, "ERROR"))
                    {
                        var arrqrcodeEmpty = ASCIIEncoding.ASCII.GetBytes("0");
                        payload.AddRange(arrqrcodeEmpty);
                    }
                    else
                    {
                        var arrqrcode = ASCIIEncoding.ASCII.GetBytes(dataSent[i].QrCodePKG);
                        payload.AddRange(arrqrcode);
                    }
                    payload.Add((byte)'^');
                    var result = dataSent[i].ResultVision ? "OK" : "NG";
                    var arrResult = ASCIIEncoding.ASCII.GetBytes(result);
                    payload.AddRange(arrResult);
                    payload.Add((byte)';');
                }

                // Create Packet:
                var packet = new List<byte>();
                packet.AddRange(this.equiqmentId);
                packet.AddRange(ASCIIEncoding.ASCII.GetBytes(E091));
                packet.AddRange(this.lotNo);
                packet.Add((Byte)';');
                packet.AddRange(payload);
                packet.AddRange(this.lotNo);
                var txBuf = packet.ToArray();
                logger.Create("MES.SEND:" + ASCIIEncoding.ASCII.GetString(txBuf));
                this.TCPClientMES.Client.Send(txBuf);

                // Write Log
                var arr = new byte[txBuf.Length];
                Array.Copy(txBuf, 0, arr, 0, arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == 0x00) { arr[i] = 0x20; }
                }
                this.CreateLog(ASCIIEncoding.ASCII.GetString(arr));
            }
            catch (Exception ex)
            {
                logger.Create("Send_QRCODE_REQ error:" + ex.Message);
            }
        }
        #endregion

        // Start Communication
        public Boolean Start()
        {
            try
            {
                logger.Create("Start TCP Server Connection MES...");
                this.PacketReceived += this.MESComm_PacketReceived;
                this.TCPListener.Start();

                this.IsRunning = true;
                this.TCPManagerThread = new Thread(new ThreadStart(this.tcpManager));
                this.TCPManagerThread.IsBackground = true;
                this.TCPManagerThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                logger.Create("Start Error:" + ex.Message);
            }
            return false;
        }
        public void Stop()
        {
            try
            {
                this.TCPListener.Stop();
                Thread.Sleep(100);

                if (this.TCPManagerThread != null)
                {
                    this.IsRunning = false;
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                logger.Create("Stop Error: " + ex.Message);
            }
        }
        public void Cancel()
        {
            this.CancelFlag = true;
        }

        private volatile Boolean isReady = false;
        private volatile List<String> qrResults;
        private volatile Boolean qrReplied = false;

        private void MESComm_PacketReceived(MESProtocolPacket packet)
        {
            if (packet.Status.Equals(E002)) { this.isReady = true; }
            else if (packet.Status.Equals(E001))
            {
                if (!String.Equals(ASCIIEncoding.ASCII.GetString(this.equiqmentId), packet.EquipmentId))
                {
                    UiManager.appSettings.connection.EquipmentName = packet.EquipmentId;
                    UiManager.SaveAppSettings();

                    this.SetupEquipment(packet.EquipmentId);
                }
                this.Return_READY_REQ();
            }
            else if (packet.Status.Equals(E092))
            {
                if (this.qrResults != null)
                {
                    foreach (var x in packet.DataQrCode)
                    {
                        this.qrResults.Add(x);
                    }
                    this.qrReplied = true;
                }
            }
        }

        private void tcpManager()
        {
            while (this.IsRunning)
            {
                try
                {
                    this.TCPClientMES = this.TCPListener.AcceptTcpClient();
                    if (this.ConnectionChanged != null)
                    {
                        this.ConnectionChanged(this.TCPClientMES.Client.RemoteEndPoint, true);
                    }
                    NetworkStream stream = this.TCPClientMES.GetStream();
                    while (stream != null && this.IsRunning && this.IsConnected)
                    {
                        if (stream.DataAvailable)
                        {
                            var rxBuf = new byte[this.TCPClientMES.Available];
                            var rxLen = 0;
                            rxLen = stream.Read(rxBuf, 0, rxBuf.Length);
                            if (rxLen > 0)
                            {
                                // Write Log
                                var arr = new byte[rxLen];
                                Array.Copy(rxBuf, 0, arr, 0, rxLen);
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    if (arr[i] == 0x00) { arr[i] = 0x20; }
                                }
                                this.CreateLog(ASCIIEncoding.ASCII.GetString(arr));

                                logger.Create(String.Format("MES.RECIVER: {0}", ASCIIEncoding.ASCII.GetString(rxBuf, 0, rxLen)));
                                var rxData = new byte[rxLen];
                                Array.Copy(rxBuf, 0, rxData, 0, rxLen);
                                var packet = this.GetPacket(rxData, rxLen);
                                if (packet != null)
                                {
                                    if (this.PacketReceived != null)
                                    {
                                        this.PacketReceived(packet);
                                    }
                                }
                            }
                        }
                    }
                    if (!this.IsConnected)
                    {
                        logger.Create("MES Disconnect !!!");
                        this.CreateLog("MES Disconnect !!!");
                        if (this.ConnectionChanged != null)
                        {
                            this.ConnectionChanged(this.TCPClientMES.Client.RemoteEndPoint, false);
                        }
                        stream.Close();
                        stream.Dispose();

                        this.TCPClientMES.Close();
                        this.TCPClientMES.Dispose();

                        continue;
                    }
                    if (!this.IsRunning)
                    {
                        logger.Create(String.Format("-> User STOP MES Connect !!!"));
                        this.CreateLog("-> User STOP MES Connect !!!");
                        if (this.ConnectionChanged != null)
                        {
                            this.ConnectionChanged(this.TCPClientMES.Client.RemoteEndPoint, false);
                        }
                        stream.Close();
                        stream.Dispose();

                        this.TCPClientMES.Close();
                        this.TCPClientMES.Dispose();

                        continue;
                    }
                    if (stream == null)
                    {
                        logger.Create(String.Format("-> MES Close Stream Socket !!!"));
                        this.CreateLog(String.Format("-> MES Close Stream Socket !!!"));

                        if (this.ConnectionChanged != null)
                        {
                            this.ConnectionChanged(this.TCPClientMES.Client.RemoteEndPoint, false);
                        }

                        stream.Close();
                        stream.Dispose();

                        this.TCPClientMES.Close();
                        this.TCPClientMES.Dispose();
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.Create("Tcp Manager MES Error: " + ex.Message);
                }
            }
        }
        private MESProtocolPacket GetPacket(byte[] buf, int rxLen)
        {
            // Reciver data by Socket ==> MES_PACKET data
            var ret = new MESProtocolPacket();
            byte[] arr;
            int idx = 0;

            #region Equipment_Id [9 byte]
            if (idx + 9 > rxLen) // Equipment ID (9byte)
            {
                logger.Create(String.Format("-> Equipment ID Too Short! " + "Via The Format: Equipment ID == 9 Byte"));
                return null;
            }
            arr = new byte[9];
            Array.Copy(buf, idx, arr, 0, 9);

            var equipment = ASCIIEncoding.ASCII.GetString(arr).Trim();
            var equipmentbyte = this.ReturnEquipment(equipment);
            var equipmentFeedback = ASCIIEncoding.ASCII.GetString(equipmentbyte);

            idx += 9;
            ret.EquipmentId = equipmentFeedback;
            #endregion

            #region Status [4 byte]          
            if (idx + 4 > rxLen) // Status (4byte)
            {
                logger.Create(String.Format("Status->  Too Short! " + "Via The Format: Status == 4 Byte"));
                return null;
            }
            arr = new byte[4];
            Array.Copy(buf, idx, arr, 0, 4);

            var cmdFeedback = ASCIIEncoding.ASCII.GetString(arr);
            if (!(string.Equals(E002, cmdFeedback) || string.Equals(E092, cmdFeedback) || string.Equals(E001, cmdFeedback)))
            {
                logger.Create(String.Format("-> Status MES Feedback : {0} Not Matching With {1}/{2}/{3}", cmdFeedback, E001, E002, E092));
                return null;
            }

            idx += 4;
            ret.Status = ASCIIEncoding.ASCII.GetString(arr);
            if (string.Equals(E001, ret.Status) || string.Equals(E002, ret.Status))
            {
                return ret;
            }
            #endregion

            #region Check status Of Feedback
            if (ret.Status.Equals(E002))
            {
                return ret;
            }
            #endregion

            #region Lot ID [9 byte]
            if (idx + 9 > rxLen) // CMD (14byte)
            {
                logger.Create(String.Format("-> Lot ID-> Too Short! " + "Via The Format: Lot ID == 9 Byte"));
                return null;
            }
            arr = new byte[9];
            Array.Copy(buf, idx, arr, 0, 9);
            var lotID = ASCIIEncoding.ASCII.GetString(arr).Trim();
            var lotIDbyte = this.ReturnLot(lotID);
            var lotIDFeedback = ASCIIEncoding.ASCII.GetString(lotIDbyte);

            var lotIDSetup = ASCIIEncoding.ASCII.GetString(this.lotNo);
            if (!String.Equals(lotIDFeedback, lotIDSetup))
            {
                logger.Create(String.Format("-> Lot ID MES Feedback :{0}/ Not Matching With Lot ID MES Setup :{1}/", lotID, lotIDFeedback));
                return null;
            }
            idx += 9;
            ret.LotID = lotIDFeedback;
            #endregion

            #region Skip ; [1 byte]
            // Not Caculator ';'
            idx += 1;
            #endregion

            #region Result Check QrCode
            ret.DataQrCode = new List<string>(0);
            if (ret.Status.Equals(E092))
            {
                arr = new byte[buf.Length - idx];
                Array.Copy(buf, idx, arr, 0, arr.Length);
                idx += arr.Length;
                var str = ASCIIEncoding.ASCII.GetString(arr);
                var sortData = str.Split(';');
                if (sortData.Length == ((UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount)*2)+1)
                {
                    int i = 0;
                    for (i = 0; i < (UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount) *2; i++)
                    {
                        ret.DataQrCode.Add(sortData[i].Trim());
                    }
                    var checkSum = sortData[i].Trim();
                    var checkSumbyte = this.ReturnLot(checkSum);
                    var checkSumFeedback = ASCIIEncoding.ASCII.GetString(checkSumbyte);
                    if (!String.Equals(checkSumFeedback, lotIDSetup))
                    {
                        logger.Create(String.Format(" -> Checksum MES Feedback : {0} Not Matching With Lot ID MES Setup : {1}", checkSumFeedback, lotIDSetup));
                        return null;
                    }
                    ret.CheckSum = checkSumFeedback;
                }
                else
                {
                    logger.Create(String.Format(" -> MES Feedback: {0} Not Enough Element/ Setup : {1}.", sortData.Length, (UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount) * 2));
                    return null;
                }
            }
            else
            {
                logger.Create(String.Format(" -> Status MES Feedback : {0} Not Matching With Status MES Protocol Format: {1}.", ret.Status, E092));
                return null;
            }
            #endregion

            return ret;
        }
        private async void CreateLog(String msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                if (!string.Equals(msg, this.lastLog))
                {
                    this.lastLog = msg;
                    Task tskCreateLog = Task.Run(() =>
                    {
                        if (this.LogCallback != null)
                        {
                            this.LogCallback(String.Format("MESProtocol {0}: {1}", UiManager.appSettings.MesSettings1.MESName, msg));
                        }
                    });
                    await Task.WhenAny(new List<Task> { tskCreateLog });
                }
            }
        }
    }
}
