using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionInspection
{
    class FENETProtocolBlock
    {
        public byte[] DeviceName { get; set; }
        public byte[] Address { get; set; }
        public byte[] DataWrite { get; set; }
        public byte[] NumberOfData { get; set; }
    }

    class FENETProtocolBlock_Frame // Data 1 Block
    {
        public byte[] LengthDeviceAndAddress { get; set; }
        public FENETProtocolBlock DeciveNameAddress { get; set; }
    }
}
