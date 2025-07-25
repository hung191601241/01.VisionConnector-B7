using System;
using System.Collections.Generic;

namespace VisionInspection
{
    internal class FENETProtocolDeviceName
    {
        public static String[] ListBitOfPLC = new String[7] { "BIT_P", "BIT_M", "BIT_L", "BIT_K", "BIT_C", "BIT_T", "BIT_F" };
        public static UInt16 MaxReadBit = 16;
        public static Byte[] DataWriteBitOfPLC = new byte[2] { 0, 1 };

        // Data Type: Bit
        public const Byte BIT_PX = 0;
        public const Byte BIT_MX = 1;
        public const Byte BIT_LX = 2;
        public const Byte BIT_KX = 3;
        public const Byte BIT_CX = 4;
        public const Byte BIT_TX = 5;
        public const Byte BIT_FX = 6;

        // Data Type Continuous: Byte
        public const Byte BYTE_PB = 7;
        public const Byte BYTE_MB = 8;
        public const Byte BYTE_LB = 9;
        public const Byte BYTE_KB = 10;
        public const Byte BYTE_CB = 11;
        public const Byte BYTE_TB = 12;
        public const Byte BYTE_FB = 13;
        public const Byte BYTE_DB = 14;

        // CPU Type
        public const String XGK = "40";
        public const String XGI = "41";
        public const String XGR = "42";

        private static Dictionary<String, Byte> stringValue = new Dictionary<String, Byte>()
        {
            {"BIT_P", 0 },
            {"BIT_M", 1 },
            {"BIT_L", 2 },
            {"BIT_K", 3 },
            {"BIT_C", 4 },
            {"BIT_T", 5 },
            {"BIT_F", 6 },

            {"BYTE_P", 7 },
            {"BYTE_M", 8 },
            {"BYTE_L", 9 },
            {"BYTE_K", 10 },
            {"BYTE_C", 11 },
            {"BYTE_T", 12 },
            {"BYTE_F", 13 },
            {"BYTE_D", 14 }
        };
        public static Byte getDeviceValueReturn(string addr)
        {
            byte ret = 00;
            if (!stringValue.TryGetValue(addr, out ret))
            {
                return 00;
            }
            return ret;
        }
        private static Dictionary<Byte, byte[]> deviceNameType = new Dictionary<Byte, byte[]>()
        {
            {BIT_PX, new byte[] { 0x00, 0x00 } },
            {BIT_MX, new byte[] { 0x00, 0x00 } },
            {BIT_LX, new byte[] { 0x00, 0x00 } },
            {BIT_KX, new byte[] { 0x00, 0x00 } },
            {BIT_CX, new byte[] { 0x00, 0x00 } },
            {BIT_TX, new byte[] { 0x00, 0x00 } },
            {BIT_FX, new byte[] { 0x00, 0x00 } },

            {BYTE_PB, new byte[] { 0x14, 0x00 } },
            {BYTE_MB, new byte[] { 0x14, 0x00 } },
            {BYTE_LB, new byte[] { 0x14, 0x00 } },
            {BYTE_KB, new byte[] { 0x14, 0x00 } },
            {BYTE_CB, new byte[] { 0x14, 0x00 } },
            {BYTE_TB, new byte[] { 0x14, 0x00 } },
            {BYTE_FB, new byte[] { 0x14, 0x00 } },
            {BYTE_DB, new byte[] { 0x14, 0x00 } }
        };
        public static byte[] getDeviceAddrType(byte addr)
        {
            var ret = new byte[2] { 0x00, 0x00 };
            if (!deviceNameType.TryGetValue(addr, out ret))
            {
                return null;
            }
            return ret;
        }

        private static Dictionary<Byte, byte[]> deviceNameMap = new Dictionary<Byte, byte[]>()
        {
            {BIT_PX, new byte[] {0x25,0x50,0x58} },
            {BIT_MX, new byte[] {0x25,0x4D,0x58} },
            {BIT_LX, new byte[] {0x25,0x4C,0x58} },
            {BIT_KX, new byte[] {0x25,0x4B,0x58} },
            {BIT_CX, new byte[] {0x25,0x43,0x58} },
            {BIT_TX, new byte[] {0x25,0x54,0x58} },
            {BIT_FX, new byte[] {0x25,0x46,0x58} },

            {BYTE_PB, new byte[] {0x25,0x50,0x42} },
            {BYTE_MB, new byte[] {0x25,0x4D,0x42} },
            {BYTE_LB, new byte[] {0x25,0x4C,0x42} },
            {BYTE_KB, new byte[] {0x25,0x4B,0x42} },
            {BYTE_CB, new byte[] {0x25,0x43,0x42} },
            {BYTE_TB, new byte[] {0x25,0x54,0x42} },
            {BYTE_FB, new byte[] {0x25,0x46,0x42} },
            {BYTE_DB, new byte[] {0x25,0x44,0x42} }
        };
        public static byte[] getListByteAddr(byte addr)
        {
            var ret = new byte[3] { 0x00, 0x00, 0x00 };
            if (!deviceNameMap.TryGetValue(addr, out ret))
            {
                return null;
            }
            return ret;
        }
        private static Dictionary<Byte, Boolean> bitType = new Dictionary<Byte, Boolean>()
        {
            {BIT_PX, true},
            {BIT_MX, true},
            {BIT_LX, true},
            {BIT_KX, true},
            {BIT_CX, true},
            {BIT_TX, true},
            {BIT_FX, true},

            {BYTE_PB, false},
            {BYTE_MB, false},
            {BYTE_LB, false},
            {BYTE_KB, false},
            {BYTE_CB, false},
            {BYTE_TB, false},
            {BYTE_FB, false},
            {BYTE_DB, false}
        };
        public static bool getBitType(byte deviceName)
        {
            bool ret;
            if (!bitType.TryGetValue(deviceName, out ret))
            {
                ret = false;
            }
            return ret;
        }
        private static Dictionary<Byte, Boolean> byteType = new Dictionary<Byte, Boolean>()
        {
            {BIT_PX, false},
            {BIT_MX, false},
            {BIT_LX, false},
            {BIT_KX, false},
            {BIT_CX, false},
            {BIT_TX, false},
            {BIT_FX, false},

            {BYTE_PB, true},
            {BYTE_MB, true},
            {BYTE_LB, true},
            {BYTE_KB, true},
            {BYTE_CB, true},
            {BYTE_TB, true},
            {BYTE_FB, true},
            {BYTE_DB, true}
        };
        public static bool getByteType(byte deviceName)
        {
            bool ret;
            if (!byteType.TryGetValue(deviceName, out ret))
            {
                ret = false;
            }
            return ret;
        }
        private static Dictionary<String, byte> ByteCPUInfor = new Dictionary<String, byte>()
        {
            {XGK, 0xA0},
            {XGI, 0xA4},
            {XGR, 0xA8}
        };

        public static byte getByteCPU(String CPU_Infor)
        {
            byte ret;
            if (!ByteCPUInfor.TryGetValue(CPU_Infor, out ret))
            {
                ret = 0xA0;
            }
            return ret;
        }
    }
}
