using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VisionInspection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PgMain : Page
    {
        public Boolean uiLogEnable { get; set; } = true;
        private String lastLog = "";
        private int gLogIndex;
        private bool autoScrollMode = true;
        public ObservableCollection<logEntry> LogEntries { get; set; } = new ObservableCollection<logEntry>();
        private void addLog(String log)
        {
            try
            {
                if (log != null && !log.Equals(lastLog))
                {
                    lastLog = log;
                    logger.Create("addLog:" + log);

                    // UI log:
                    if (true)
                    {
                        logEntry x = new logEntry()
                        {
                            logIndex = gLogIndex++,
                            logTime = DateTime.Now.ToString("HH:mm:ss.ff"),
                            logMessage = log,
                        };
                        this.Dispatcher.Invoke(() =>
                        {
                            LogEntries.Add(x);

                            // Nếu số lượng log vượt quá 1000
                            if (LogEntries.Count > 300)
                            {
                                // Giữ lại 50 dòng gần nhất
                                var recentLogs = LogEntries.Skip(LogEntries.Count - 100).ToList();
                                LogEntries.Clear();
                                foreach (var item in recentLogs)
                                    LogEntries.Add(item);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Create("addLog error:" + ex.Message);
            }
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                if (e.Source.GetType().Equals(typeof(ScrollViewer)))
                {
                    ScrollViewer sv = (ScrollViewer)e.Source;

                    if (sv != null)
                    {
                        // User scroll event : set or unset autoscroll mode
                        if (e.ExtentHeightChange == 0)
                        {   // Content unchanged : user scroll event
                            if (sv.VerticalOffset == sv.ScrollableHeight)
                            {   // Scroll bar is in bottom -> Set autoscroll mode
                                autoScrollMode = true;
                            }
                            else
                            {   // Scroll bar isn't in bottom -> Unset autoscroll mode
                                autoScrollMode = false;
                            }
                        }

                        // Content scroll event : autoscroll eventually
                        if (autoScrollMode && e.ExtentHeightChange != 0)
                        {   // Content changed and autoscroll mode set -> Autoscroll
                            sv.ScrollToVerticalOffset(sv.ExtentHeight);
                        }
                    }
                }
            }
            catch
            {
            }
        }



    }
    public static class ActionClearAlarm
    {
        public static Action ClearErrorAction { get; set; }
    }
    public class logEntry : PropertyChangedBase
    {
        public int logIndex { get; set; }
        public String logTime { get; set; }
        public string logMessage { get; set; }
    }
    public class collapsibleLogEntry : logEntry
    {
        public List<logEntry> Contents { get; set; }
    }
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
            catch (Exception ex)
            {
                Debug.Write("OnPropertyChanged error:" + ex.Message);
            }
        }
    }
}
