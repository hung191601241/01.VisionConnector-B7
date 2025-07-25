using System;
using System.Windows;
using System.Windows.Controls;

namespace VisionInspection
{
    /// <summary>
    /// Interaction logic for PgStatusLog.xaml
    /// </summary>
    public partial class PgPlcStatusLog : Page
    {
        private static MyLogger logger = new MyLogger("PgMenuStatusLog");

        private const int ALARM_PAGE_SIZE = 100;
        private int alarmCurrentPage = 0;
        private int alarmTotalPage = 0;

        private const int LOG_PAGE_SIZE = 100;
        private int logCurrentPage = 0;
        private int logTotalPage = 0;

        public PgPlcStatusLog()
        {
            InitializeComponent();

            this.Loaded += this.PgStatusLog_Loaded;
            this.btLog.Click += this.BtLog_Click;
            this.btSpcOutput.Click += this.BtSpcOutput_Click;
            this.btSpcSearch.Click += this.BtSpcSearch_Click;

            // Alarm tab:
            this.btAlarmFirst.Click += this.BtAlarmFirst_Click;
            this.btAlarmPrePage.Click += this.BtAlarmPrePage_Click;
            this.btAlarmPrevious.Click += this.BtAlarmPrevious_Click;
            this.btAlarmCurrent.Click += this.BtAlarmCurrent_Click;
            this.btAlarmNext.Click += this.BtAlarmNext_Click;
            this.btAlarmNextPage.Click += this.BtAlarmNextPage_Click;
            this.btAlarmLast.Click += this.BtAlarmLast_Click;

            // Event tab:
            this.btLogPrevious.Click += this.BtLogPrevious_Click;
            this.btLogNext.Click += this.BtLogNext_Click;
            this.btLogToday.Click += this.BtLogToday_Click;
            this.btLogPrePage.Click += this.BtLogPrePage_Click;
            this.btLogNextPage.Click += this.BtLogNextPage_Click;
            this.dtLogDate.SelectedDateChanged += this.DtDate_SelectedDateChanged;
        }

        private void loadLogs()
        {
            var dt = this.dtLogDate.SelectedDate.Value;
            var logCnt = DbRead.CountUserLogs(dt);
            logTotalPage = (logCnt + LOG_PAGE_SIZE - 1) / LOG_PAGE_SIZE;
            var userLogs = DbRead.GetUserLogs(dt, logCurrentPage, LOG_PAGE_SIZE);
            dgridLogs.ItemsSource = userLogs;

            this.btLogCurrent.Content = String.Format("{0}/{1}", logCurrentPage + 1, logTotalPage);

            dgridLogs.Focus();
            dgridLogs.SelectedIndex = 0;
        }

        private void DtDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                logCurrentPage = 0;
                loadLogs();
            }
            catch (Exception ex)
            {
                logger.Create("DtDate_SelectedDateChanged error:" + ex.Message);
            }
        }

        private void BtLogPrevious_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dt = this.dtLogDate.SelectedDate.Value;
                this.dtLogDate.SelectedDate = dt.AddDays(-1); // .Subtract(new TimeSpan(1, 0, 0, 0));
            }
            catch (Exception ex)
            {
                logger.Create("BtLogPrevious_Click error:" + ex.Message);
            }
        }

        private void BtLogNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dt = this.dtLogDate.SelectedDate.Value;
                this.dtLogDate.SelectedDate = dt.AddDays(1);
            }
            catch (Exception ex)
            {
                logger.Create("BtLogNext_Click error:" + ex.Message);
            }
        }

        private void BtLogToday_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dt = this.dtLogDate.SelectedDate.Value;
                this.dtLogDate.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                logger.Create("BtLogToday_Click error:" + ex.Message);
            }
        }

        private void BtLogPrePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (logCurrentPage > 0)
                {
                    logCurrentPage--;
                }
                loadLogs();
            }
            catch (Exception ex)
            {
                logger.Create("BtLogPrePage_Click error:" + ex.Message);
            }
        }

        private void BtLogNextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (logCurrentPage < logTotalPage - 1)
                {
                    logCurrentPage++;
                }
                loadLogs();
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmNextPage_Click error:" + ex.Message);
            }
        }

        private int getTotalPageCount()
        {
            var evCnt = DbRead.CountEvents();
            return (evCnt + ALARM_PAGE_SIZE - 1) / ALARM_PAGE_SIZE;
        }

        private void loadEvents()
        {
            this.btAlarmCurrent.Content = String.Format("{0}/{1}", alarmCurrentPage + 1, alarmTotalPage);

            var events = DbRead.GetEvents(alarmCurrentPage, ALARM_PAGE_SIZE);
            dgridAlarms.ItemsSource = events;

            dgridAlarms.Focus();
            dgridAlarms.SelectedIndex = 0;
        }

        private void BtAlarmFirst_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                alarmTotalPage = getTotalPageCount();
                alarmCurrentPage = 0;
                loadEvents();
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmFirst_Click error:" + ex.Message);
            }
        }

        private void BtAlarmPrePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                alarmTotalPage = getTotalPageCount();
                if (alarmCurrentPage > 0)
                {
                    alarmCurrentPage--;
                }
                loadEvents();
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmPrePage_Click error:" + ex.Message);
            }
        }

        private void BtAlarmPrevious_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgridAlarms.Focus();
                int nextIndex = dgridAlarms.SelectedIndex;
                if (nextIndex > 0)
                {
                    dgridAlarms.SelectedIndex = nextIndex - 1;
                }
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmPrevious_Click error:" + ex.Message);
            }
        }

        private void BtAlarmCurrent_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtAlarmNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgridAlarms.Focus();
                int nextIndex = dgridAlarms.SelectedIndex + 1;
                if (nextIndex < dgridAlarms.Items.Count)
                {
                    dgridAlarms.SelectedIndex = nextIndex;
                }
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmNext_Click error:" + ex.Message);
            }
        }

        private void BtAlarmNextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                alarmTotalPage = getTotalPageCount();
                if (alarmCurrentPage < alarmTotalPage - 1)
                {
                    alarmCurrentPage++;
                }
                loadEvents();
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmNextPage_Click error:" + ex.Message);
            }
        }

        private void BtAlarmLast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                alarmTotalPage = getTotalPageCount();
                if (alarmTotalPage > 0)
                {
                    alarmCurrentPage = alarmTotalPage - 1;
                }
                loadEvents();
            }
            catch (Exception ex)
            {
                logger.Create("BtAlarmLast_Click error:" + ex.Message);
            }
        }

        private void PgStatusLog_Loaded(object sender, RoutedEventArgs e)
        {
            //try {
            alarmTotalPage = getTotalPageCount();
            alarmCurrentPage = 0;
            loadEvents();

            logCurrentPage = 0;
            this.dtLogDate.SelectedDate = DateTime.Today;
            loadLogs();
            //} catch (Exception ex) {
            //    logger.Create("PgStatusLog_Loaded error:" + ex.Message);
            //}
        }

        private void BtLog_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_STATUS_LOG);
        }

        private void BtSpcOutput_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_STATUS_SPC_OUTPUT);
        }

        private void BtSpcSearch_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_STATUS_SPC_SEARCH);
        }
    }
}