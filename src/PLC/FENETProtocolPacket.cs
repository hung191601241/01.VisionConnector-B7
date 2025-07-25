using System;
using System.Collections.Generic;

namespace VisionInspection
{
    class FENETProtocolPacket
    {
        public byte[] LSIS_ID { get; set; } // 10 Byte
        public byte[] PLC_Information { get; set; } // 2 Byte
        public byte CPU_Information { get; set; } // 1 Byte
        public byte Frame_Direction { get; set; } // 1 Byte
        public byte[] Frame_Order_No { get; set; } // 2 Byte
        public byte[] Length { get; set; } // 1 Byte
        public byte Position_Information { get; set; } // 1 Byte
        public byte Check_Sum { get; set; } // 1 Byte
        public byte[] Command { get; set; } // 2 Byte
        public byte[] Data_Type { get; set; } // 2 Byte
        public byte[] Reserved_Area { get; set; } // 2 Byte
        public byte[] Error_Status { get; set; } // 2 Byte
        public byte[] Variable_Length { get; set; } // 2 Byte
        public List<byte[]> List_Data_Count { get; set; } // List Data Count
        public List<byte[]> List_Data { get; set; } // List Data
    }
}
