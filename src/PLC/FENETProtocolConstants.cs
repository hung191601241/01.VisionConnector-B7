using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionInspection
{
    class FENETProtocolConstants
    {
        // Timeout Connect
        public const int TCP_FENET_RX_TIMEOUT = 500;

        // Company ID (LSIS-XGT\n\n)
        public const byte L = 0x4C;
        public const byte S = 0x53;
        public const byte I = 0x49;
        public const byte _ = 0x2D;
        public const byte X = 0x58;
        public const byte G = 0x47;
        public const byte T = 0x54;
        public const byte _N = 0x00;

        // Company ID (LGIS-GLOFA)
        public const byte O = 0x4F;
        public const byte F = 0x46;
        public const byte A = 0x41;

        // PLC Info
        public const byte PLC_INFOR = 0x00;

        // Source Of Frame
        public const byte SOURCE_OF_FRAME = 0x33;
        public const byte SOURCE_OF_FRAME_FEEDBACK = 0x11;

        // Invoke ID
        public const byte INVOKE_ID = 0x00;

        // Reserved 
        public const byte RESERVED = 0x00;
    }
}
