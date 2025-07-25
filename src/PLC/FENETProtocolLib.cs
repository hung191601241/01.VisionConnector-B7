using System;
using System.Collections.Generic;
using System.Linq;

namespace VisionInspection
{
    class FENETProtocolLib
    {
        public static byte ByteCheckSum(Byte[] Buff, int iStart, int iEnd)
        {
            int i = 0;
            int CheckSum = 0;
            for (i = iStart; i <= iEnd; i++)
            {
                CheckSum += Buff[i];
            }
            return (byte)(CheckSum % 256);
        }
        public static List<byte[]> BytesToList(byte[] byteDataReciver, UInt16 indx)
        {
            return Enumerable.Range(0, byteDataReciver.Length / indx)
                                            .Select(i => byteDataReciver.Skip(i * indx).Take(indx).ToArray())
                                            .ToList();
        }
        public static Int16 BytesToInt16(byte[] bytes) 
        {
            Int16 result = BitConverter.ToInt16(bytes, 0);
            return result;
        }
        public static UInt16 BytesToUInt16(byte[] bytes)
        {
            UInt16 result = BitConverter.ToUInt16(bytes, 0);
            return result;
        }
        public static Int32 BytesToInt32(byte[] bytes)
        {
            Int32 result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
        public static UInt32 BytesToUInt32(byte[] bytes)
        {
            UInt32 result = BitConverter.ToUInt32(bytes, 0);
            return result;
        }
        public static Int64 BytesToInt64(byte[] bytes)
        {
            Int64 result = BitConverter.ToInt64(bytes, 0);
            return result;
        }
        public static UInt64 BytesToUInt64(byte[] bytes)
        {
            UInt64 result = BitConverter.ToUInt64(bytes, 0);
            return result;
        }

        public static byte[] Int16ToByte (Int16 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }
        public static byte[] UInt16ToByte(UInt16 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }

        public static byte[] Int32ToByte(Int32 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }
        public static byte[] UInt32ToByte(UInt32 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }

        public static byte[] Int64ToByte(Int64 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }
        public static byte[] UInt64ToByte(UInt64 data)
        {
            byte[] byteArray = BitConverter.GetBytes(data);
            return byteArray;
        }

        public static byte[] GetLength (int length)
        {
            return new byte[] { (Byte)(length & 0xFF), (Byte)(length >> 8) };
        }
    }
}
