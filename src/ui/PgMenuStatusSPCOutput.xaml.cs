using AutoLaserCuttingInput;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VisionInspection
{
    /// <summary>
    /// Interaction logic for PgMenuStatusSPCOutput.xaml
    /// </summary>
    public partial class PgMenuStatusSPCOutput : Page
    {
        private static MyLogger logger = new MyLogger("PgMenuStatusSPCOutput");

        private Brush BT_ACTIVE_BACKGROUND = Brushes.SkyBlue;

        private int selectedYear = 2020;
        private int selectedMonth = 1;

        public PgMenuStatusSPCOutput()
        {
            InitializeComponent();

            this.Loaded += this.PgMenuStatusSPCOutput_Loaded;
            this.btLog.Click += this.BtLog_Click;
            this.btSpcOutput.Click += this.BtSpcOutput_Click;
            this.btSpcSearch.Click += this.BtSpcSearch_Click;

            this.rdViewMonthInYear.Click += this.RdX_Click;
            this.rdViewWeekInYear.Click += this.RdX_Click;
            this.rdViewWeekInMonth.Click += this.RdX_Click;
            this.rdViewDayInMonth.Click += this.RdX_Click;

            this.btReadAgain.Click += this.BtReadAgain_Click;
            this.bt1Month.Click += this.BtXMonth_Click;
            this.bt2Month.Click += this.BtXMonth_Click;
            this.bt3Month.Click += this.BtXMonth_Click;
            this.bt4Month.Click += this.BtXMonth_Click;
            this.bt5Month.Click += this.BtXMonth_Click;
            this.bt6Month.Click += this.BtXMonth_Click;
            this.bt7Month.Click += this.BtXMonth_Click;
            this.bt8Month.Click += this.BtXMonth_Click;
            this.bt9Month.Click += this.BtXMonth_Click;
            this.bt10Month.Click += this.BtXMonth_Click;
            this.bt11Month.Click += this.BtXMonth_Click;
            this.bt12Month.Click += this.BtXMonth_Click;
            this.btPreMonth.Click += this.BtPreMonth_Click;
            this.btNextMonth.Click += this.BtNextMonth_Click;
            this.btPreYear.Click += this.BtPreYear_Click;
            this.btNextYear.Click += this.BtNextYear_Click;
            this.btSaveToExcel.Click += this.BtSaveToExcel_Click;

            this.btLotSearch.Click += this.BtLotSearch_Click;
        }

        private void updateUI()
        {
            this.lblYear.Content = selectedYear.ToString();
            this.tvSaveToExcel.Text = String.Format("Save to Excel file\r\n({0}year)", selectedYear);
            var monthEnabled = (bool)rdViewWeekInMonth.IsChecked || (bool)rdViewDayInMonth.IsChecked;
            for (int i = 1; i <= 12; i++)
            {
                var bt = this.ugridMonths.FindName(String.Format("bt{0}Month", i)) as Button;
                if (bt != null)
                {
                    bt.IsEnabled = monthEnabled;
                    if ((selectedMonth == i) && monthEnabled)
                    {
                        bt.Background = BT_ACTIVE_BACKGROUND;
                    }
                    else
                    {
                        bt.ClearValue(Button.BackgroundProperty);
                    }
                }
            }
            this.btPreMonth.IsEnabled = monthEnabled;
            this.btNextMonth.IsEnabled = monthEnabled;

            var str = String.Format("{0}year", selectedYear);
            if (monthEnabled)
            {
                str += String.Format(" {0}month", selectedMonth);
            }
            this.dgridResults.Columns[0].Header = str;
        }

        private void execute()
        {
           
        }

        private void BtLotSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)rdViewMonthInYear.IsChecked)
                {

                }
                else if ((bool)rdViewDayInMonth.IsChecked)
                {

                }
                else if ((bool)rdViewWeekInYear.IsChecked)
                {

                }
                else if ((bool)rdViewWeekInMonth.IsChecked)
                {

                }
            }
            catch (Exception ex)
            {
                logger.Create("BtLotSearch_Click error:" + ex.Message);
            }
        }

        private void RdX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                updateUI();

                execute();
            }
            catch (Exception ex)
            {
                logger.Create("RdX_Click error:" + ex.Message);
            }
        }

        private void BtReadAgain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                execute();
            }
            catch (Exception ex)
            {
                logger.Create("BtReadAgain_Click error:" + ex.Message);
            }
        }

        private void BtXMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bt = sender as Button;
                if (bt != null)
                {
                    var btName = bt.Content.ToString();
                    if (btName.Contains("10"))
                    {
                        this.selectedMonth = 10;
                    }
                    else if (btName.Contains("11"))
                    {
                        this.selectedMonth = 11;
                    }
                    else if (btName.Contains("12"))
                    {
                        this.selectedMonth = 12;
                    }
                    else
                    {
                        this.selectedMonth = int.Parse(bt.Content.ToString().Substring(0, 1));
                    }

                    execute();
                }
                updateUI();
            }
            catch (Exception ex)
            {
                logger.Create("RdX_Click error:" + ex.Message);
            }
        }

        private void BtPreMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMonth > 1)
                {
                    selectedMonth--;
                }
                updateUI();

                execute();
            }
            catch (Exception ex)
            {
                logger.Create("BtPreMonth_Click error:" + ex.Message);
            }
        }

        private void BtNextMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMonth < 12)
                {
                    selectedMonth++;
                }
                updateUI();

                execute();
            }
            catch (Exception ex)
            {
                logger.Create("BtNextMonth_Click error:" + ex.Message);
            }
        }

        private void BtPreYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedYear--;
                updateUI();

                execute();
            }
            catch (Exception ex)
            {
                logger.Create("BtPreYear_Click error:" + ex.Message);
            }
        }

        private void BtNextYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedYear++;
                updateUI();

                execute();
            }
            catch (Exception ex)
            {
                logger.Create("BtNextYear_Click error:" + ex.Message);
            }
        }

        private void BtSaveToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new Thread(new ThreadStart(() => {
                    try
                    {
                        //SpcManager.SaveToExcel(selectedYear);
                    }
                    catch (Exception ex1)
                    {
                        logger.Create("SpcManager.SaveToExcel error:" + ex1.Message);
                    }
                })).Start();

                new WndConfirm().DoComfirmYesNo("Saved it as an Excel file on my desktop", Window.GetWindow(this));

            }
            catch (Exception ex)
            {
                logger.Create("BtSaveToExcel_Click error:" + ex.Message);
            }
        }

        private void PgMenuStatusSPCOutput_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.selectedYear = 2020;
                this.selectedMonth = 1;
                updateUI();
            }
            catch (Exception ex)
            {
                logger.Create("PgMenuStatusSPCOutput_Loaded error:" + ex.Message);
            }
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
