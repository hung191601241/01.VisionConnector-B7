using System;
using System.Diagnostics;
using System.IO;

namespace VisionInspection
{
    class MyLogger
    {
        private static Object objLock = new Object();

        private String prefix = "";

        public MyLogger(String prefix) {
            this.prefix = prefix;
        }

        public void Create(String content) {
            // Get FilePath:
            var fileName = String.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "DebugLogs", "Unit");
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            var filePath = System.IO.Path.Combine(folder, fileName);

            lock (objLock) {
                try {
                    var log = String.Format("\r\n{0}-{1}: {2}", DateTime.Now.ToString("HH:mm:ss.ff"), this.prefix, content);

                    System.Diagnostics.Debug.Write(log);

                    using (var strWriter = new StreamWriter(filePath, true)) {
                        strWriter.Write(log);
                        strWriter.Flush();
                    }
                } catch (Exception ex) {
                    Debug.Write("\r\nMyLoger.Create error:" + ex.Message);
                }
            }
        }
        public void CreateMES(string PCB, string resultvison, string MES)
        {

            var fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string lotNumber = UiManager.appSettings.lotData.lotId;
            string config = UiManager.appSettings.lotData.deviceId;


            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogData", currentDate, lotNumber, config);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }


            var filePath = Path.Combine(folder, fileName);

            lock (objLock)
            {
                try
                {
                    // Nếu file chưa tồn tại, tạo và ghi tiêu đề
                    if (!File.Exists(filePath))
                    {
                        string header = "Date, PCB, RESULT VISION, RESULT MES";
                        File.AppendAllText(filePath, header + Environment.NewLine);
                    }

                    // Chuẩn bị nội dung log

                    var log = String.Format("\r\n{0},{1},{2},{3}", DateTime.Now.ToString("HH:mm:ss.ff"), PCB, resultvison, MES);

                    // Ghi log ra cửa sổ Debug
                    System.Diagnostics.Debug.Write(log);

                    // Ghi nội dung vào file
                    using (var strWriter = new StreamWriter(filePath, true))
                    {
                        strWriter.Write(log);
                        strWriter.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write($"\r\nMyLogger.Create error: {ex.Message}");
                }
            }
        }
    }
}
