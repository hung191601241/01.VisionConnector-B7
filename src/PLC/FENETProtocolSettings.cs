using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionInspection
{
    public class FENETProtocolSettings
    {
        //"192.168.0.38"  2004
        public String PLCIp { get; set; }  // Ip Address
        public Int32 PLCPort { get; set; }  // Port Address
        public bool Is_LSIS_XGT { get; set; } // Using True: LSIS-XGT/n/n
                                              // Using False: LGIS-GLOFA
        public String CPU_Infor { get; set; } // XGK/ XGI/ XGR

        public int SLOT_No { get; set; } // Bit 0~3 : FEnet I/F module’s Slot No.
        public int BASE_No { get; set; } // Bit 4~7 : FEnet I/F module’s Base No
        public String PLCName { get; set; }
        public bool Is_Enable_Log { get; set; } // Enable Write Log
        public FENETProtocolSettings() // Construction
        {
            this.PLCIp = "127.0.0.1";
            this.PLCPort = 2004;
            this.Is_LSIS_XGT = true;
            this.CPU_Infor = "XGK";
            this.SLOT_No = 0;
            this.BASE_No = 0;
            this.PLCName = "";
            this.Is_Enable_Log = true;
        }

        public FENETProtocolSettings Clone() // Clone McProtocolSettings
        {
            return new FENETProtocolSettings()
            {
                PLCIp = String.Copy(this.PLCIp),
                PLCPort = this.PLCPort,
                Is_LSIS_XGT = this.Is_LSIS_XGT,
                CPU_Infor = string.Copy(this.CPU_Infor),
                SLOT_No = this.SLOT_No,
                BASE_No = this.BASE_No,
                PLCName = string.Copy(this.PLCName),
                Is_Enable_Log = this.Is_Enable_Log
            };
        }
    }
}
