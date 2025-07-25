using System;
using System.Diagnostics;
using System.IO;

namespace VisionInspection
{
    class FENETProtocolLogger
    {
        private static Object Lock = new Object();

        public void CreateTxLog(String tx)
        {
            if (tx.EndsWith("\r\n") || tx.EndsWith("\r") || tx.EndsWith("\n"))
            {
                tx = tx.TrimEnd(new char[] { '\r', '\n' });
            }
            var log = String.Format("{0} [SEND TO PLC] : {1}", DateTime.Now.ToString("HH:mm:ss.fff"), tx);
            this.Create(log);
        }

        public void CreateRxLog(String rx)
        {
            if (rx.EndsWith("\r\n") || rx.EndsWith("\r") || rx.EndsWith("\n"))
            {
                rx = rx.TrimEnd(new char[] { '\r', '\n' });
            }
            var log = String.Format("{0} [READ FROM PLC] : {1}", DateTime.Now.ToString("HH:mm:ss.fff"), rx);
            this.Create(log);
        }

        private void Create(String log)
        {
            lock (Lock)
            {
                try
                {
                    // Check file existing:                    
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data Equipments", "FENETProtocol");
                    folder = Path.Combine(folder, DateTime.Today.ToString("yyyy-MM"));
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    var fileName = String.Format("{0}.log", DateTime.Today.ToString("yyyy-MM-dd"));
                    var filePath = Path.Combine(folder, fileName);

                    // Create log:
                    using (var strWriter = new StreamWriter(filePath, true))
                    {
                        strWriter.WriteLine(log);
                        strWriter.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write("FENETProtocol Logger Create Error: " + ex.Message);
                }
            }
        }
    }
}
