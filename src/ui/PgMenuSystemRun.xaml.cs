using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Input;
using AutoLaserCuttingInput;

namespace VisionInspection
{
    /// <summary>
    /// Interaction logic for PgPlcSystemRun.xaml
    /// </summary>
    public partial class PgPlcSystemRun : Page
    {
        private System.Timers.Timer cycleTimer;
        //private System.Timers.Timer updateTimer;

        private static MyLogger logger = new MyLogger("PgPlcManual");
        private const int LOG_DISPLAY_DELAY = 100;
        private MCProtocol plcComm = new MCProtocol();
        private int[] PlcDeviceDoubleWord;
        List<bool> PlcDeviceMultiBitM;
        private bool isAlarming;

        public PgPlcSystemRun()
        {
            InitializeComponent();
            this.cycleTimer = new System.Timers.Timer(100);
            this.cycleTimer.AutoReset = true;
            this.cycleTimer.Elapsed += CycleTimer_Elapsed;
            this.Loaded += PgPlcSystemRun_Loaded;
            this.Unloaded += PgPlcSystemRun_Unloaded;
            this.btnVisionOn_ch1.Click += BtnVisionOn_ch1_Click;
            this.btnVisionOff_ch1.Click += BtnVisionOff_ch1_Click;
            this.btnVisionOn_ch2.Click += BtnVisionOn_ch2_Click;
            this.btnVisionOff_ch2.Click += BtnVisionOff_ch2_Click;
            this.btnLampMCOn_ch1.Click += BtnLampMCOn_ch1_Click;
            this.btnLampMCOff_ch1.Click += BtnLampMCOff_ch1_Click;
            this.btnLampMCOn_ch2.Click += BtnLampMCOn_ch2_Click;
            this.btnLampMCOff_ch2.Click += BtnLampMCOff_ch2_Click;
            this.btnLightCurtainOn_ch1.Click += BtnLightCurtainOn_ch1_Click;
            this.btnLightCurtainOff_ch1.Click += BtnLightCurtainOff_ch1_Click;
            this.btnLightCurtainOn_ch2.Click += BtnLightCurtainOn_ch2_Click;
            this.btnLightCurtainOff_ch2.Click += BtnLightCurtainOff_ch2_Click;
            this.btnDoorOn_ch1.Click += BtnDoorOn_ch1_Click;
            this.btnDoorOff_ch1.Click += BtnDoorOff_ch1_Click;
            this.btnDoorOn_ch2.Click += BtnDoorOn_ch2_Click;
            this.btnDoorOff_ch2.Click += BtnDoorOff_ch2_Click;
            this.btnProductOn_ch1.Click += BtnProductOn_ch1_Click;
            this.btnProductOff_ch1.Click += BtnProductOff_ch1_Click;
            this.btnProductOn_ch2.Click += BtnProductOn_ch2_Click;
            this.btnProductOff_ch2.Click += BtnProductOff_ch2_Click;
        }

        private void BtnProductOff_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnProductOn_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnProductOff_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnProductOn_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDoorOff_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDoorOn_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDoorOff_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDoorOn_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLightCurtainOff_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLightCurtainOn_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLightCurtainOff_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLightCurtainOn_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLampMCOff_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLampMCOn_ch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLampMCOff_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLampMCOn_ch1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVisionOff_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!plcComm.IsConnected())
            {
               
                return;
            }
            plcComm.SetBit(DataType.Devicecode.M, DataType.Devicecode.M, 1157);
        }

        private void BtnVisionOn_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!plcComm.IsConnected())
            {
         
                return;
            }
            plcComm.RstBit(DataType.Devicecode.M, DataType.Devicecode.M, 1157);
        }

        private void BtnVisionOff_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!plcComm.IsConnected())
            {

                return;
            }
            plcComm.SetBit(DataType.Devicecode.M, DataType.Devicecode.M, 1156);
        }

        private void BtnVisionOn_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!plcComm.IsConnected())
            {
                return;
            }
            plcComm.RstBit(DataType.Devicecode.M, DataType.Devicecode.M, 1156);
        }

        private void PgPlcSystemRun_Unloaded(object sender, RoutedEventArgs e)
        {
            if (plcComm != null)
                plcComm.Disconnect();
        }

        private void PgPlcSystemRun_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void CycleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!plcComm.IsConnected())
                return;
            try
            {

                PlcDeviceDoubleWord = plcComm.ReadDoubleWord(DataType.Devicecode.D, 7000, 300);
                PlcDeviceMultiBitM = plcComm.ReadMultiBits(DataType.Devicecode.M, 0, 460);
                UpdateUIData();
                return;
            }
            catch (Exception ex)
            {
                logger.Create("PLC Com Err" + ex.ToString());
                return;
            }
        }
        private void UpdateUIData()
        {
            if (!plcComm.IsConnected())
                return;
            var converter = new BrushConverter();
            //Motor Control
            this.Dispatcher.Invoke(() =>
            {
                if (TabControl.SelectedIndex == 0)
                {
                    if (!PlcDeviceMultiBitM[1156])
                    {
                        recVisionOn_ch1.Fill = Brushes.Yellow;
                        recVisionOff_ch1.Fill = Brushes.DarkRed;
                    }
                    else if (PlcDeviceMultiBitM[1156])
                    {
                        recVisionOn_ch1.Fill = Brushes.DarkRed;
                        recVisionOff_ch1.Fill = Brushes.Yellow;
                    }
                }
                else if (TabControl.SelectedIndex == 1)
                {
                    if (!PlcDeviceMultiBitM[1157])
                    {
                        recVisionOn_ch2.Fill = Brushes.Yellow;
                        recVisionOff_ch2.Fill = Brushes.DarkRed;
                    }
                    else if (PlcDeviceMultiBitM[1157])
                    {
                        recVisionOn_ch2.Fill = Brushes.DarkRed;
                        recVisionOff_ch2.Fill = Brushes.Yellow;
                    }
                }
                if (!PlcDeviceMultiBitM[1156])
                {
                    btnVisionOn_ch1.Background = Brushes.Cyan;
                    btnVisionOff_ch1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                else if (PlcDeviceMultiBitM[1156])
                {
                    btnVisionOff_ch1.Background = Brushes.Cyan;
                    btnVisionOn_ch1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                if (!PlcDeviceMultiBitM[1157])
                {
                    btnVisionOn_ch2.Background = Brushes.Cyan;
                    btnVisionOff_ch2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                else if (PlcDeviceMultiBitM[1157])
                {
                    btnVisionOff_ch2.Background = Brushes.Cyan;
                    btnVisionOn_ch2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
            });
        }
        private void displayAlarm(Int32 code)
        {
            try
            {
                isAlarming = true;
                var alarm = new AlarmInfo(code, AlarmInfo.getSolution(code));
                this.Dispatcher.Invoke(() =>
                {
                    var wnd = new WndAlert(code, AlarmInfo.getSolution(code), AlarmInfo.getMessage(code));
                    wnd.ShowDialog();
                    isAlarming = false;
                });
                while (isAlarming)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                logger.Create("displayAlarm error:" + ex.Message);
            }
        }
    }
}