using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisionInspection;

namespace AutoLaserCuttingInput
{
    /// <summary>
    /// Interaction logic for PgSystemMenu01.xaml
    /// </summary>
    public partial class PgSystemMenu01 : Page
    {
        private bool IsRunning;

        private Color Colo_ON_GR;
        private Color Colo_ON_RE;
        private Color Colo_OFF;

        private List<bool> L_ListUpdateBitLamp_10500 = new List<bool>();
        MyLogger logger = new MyLogger("PG_System_Menu");
        public PgSystemMenu01()
        {
            InitializeComponent();
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.Loaded += PgSystemMenu01_Loaded;
            this.Unloaded += PgSystemMenu01_Unloaded;

            this.btDryRunModel.Click += BtDryRunModel_Click;
            this.btStepRunOn.Click += BtStepRunOn_Click;
            this.btDisableAlarm.Click += BtDisableAlarm_Click;
            this.btDisableBuzzer.Click += BtDisableBuzzer_Click;
            this.btDisableDoorSensor.Click += BtDisableDoorSensor_Click;
            this.btDisabaleSafetySensor.Click += BtDisabaleSafetySensor_Click;

            this.btBypassDetectCheckJig.Click += BtBypassDetectCheckJig_Click;
            this.btBypassVacuumCheck.Click += BtBypassVacuumCheck_Click;
            this.btDisableVacuum.Click += BtDisableVacuum_Click;

            this.btDisableCH1.Click += BtDisableCH1_Click;
            this.btDisableCH2.Click += BtDisableCH2_Click;
            this.btManualRun.Click += BtManualRun_Click;





        }

        private void BtManualRun_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_MANUAL_RUN, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_MANUAL_RUN, false);
            }
        }

        private void BtDisableCH2_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_CH2, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_CH2, false);
            }
        }

        private void BtDisableCH1_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_CH1, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_CH1, false);
            }
        }

        private void BtDisableVacuum_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_VACUUMM, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_VACUUMM, false);
            }
        }

        private void BtBypassVacuumCheck_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_BYPASS_VACUUMM_CHECK, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_BYPASS_VACUUMM_CHECK, false);
            }
        }

        private void BtBypassDetectCheckJig_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_BYPASS_DETECT_CHECK_JIG, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_BYPASS_DETECT_CHECK_JIG, false);
            }
        }

        private void BtDisabaleSafetySensor_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_SAFETY_SENSORS, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_SAFETY_SENSORS, false);
            }
        }

        private void BtDisableDoorSensor_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_DOOR_SENSORS, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_DOOR_SENSORS, false);
            }
        }

        private void BtDisableBuzzer_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_BUZZER, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_BUZZER, false);
            }
        }

        private void BtDisableAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_ALARM, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DISABLE_ALARM, false);
            }
        }

        private void BtStepRunOn_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_STEP_RUN_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_STEP_RUN_ON, false);
            }
        }

        private void BtDryRunModel_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DRYRUM_MODE, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_DRYRUM_MODE, false);
            }
        }

        private void PgSystemMenu01_Unloaded(object sender, RoutedEventArgs e)
        {
            this.IsRunning = false;
        }

        private void PgSystemMenu01_Loaded(object sender, RoutedEventArgs e)
        {
            Thread TheadReadPLC = new Thread(() => ReadPLC());
            TheadReadPLC.IsBackground = true;
            TheadReadPLC.Start();

            this.IsRunning = true;

            string hexColorOn1 = "#66FF66"; // Mã màu ON (XANH )
            string hexColorOn2 = "#FF0033"; // Mã màu OFF (ĐỎ)
            string hexColorOff = "#EEEEEE"; // Mã màu OFF (TRẮNG)
            Colo_ON_GR = (Color)ColorConverter.ConvertFromString(hexColorOn1);
            Colo_ON_RE = (Color)ColorConverter.ConvertFromString(hexColorOn2);
            Colo_OFF = (Color)ColorConverter.ConvertFromString(hexColorOff);
        }
        private void ReadPLC()
        {
            while (IsRunning)
            {
                if (UiManager.PLC.IsConnected)
                {
                    var btDryRun = GetLampDryrun();
                    var btStepRun = GetLampStepRun();
                    var btDisableAlarm = GetLampDisableAlarm();
                    var btDisableBuzzer = GetLampDisableBuzzer();
                    var btDisableDoorSensor = GetLampDoorSensor();
                    var btDisableSafety = GetLampDoorSafery();
                    var btByPassCheckJig = GetLampDetectJig();
                    var btByPassVacuumCheck = GetLampVacuumCheck();
                    var btDisableVacuum = GetLampVacuum();
                    var btCH1 = GetCH1();
                    var btCH2 = GetCH2();
                    var btManualRun = GetManualRun();

                    Dispatcher.Invoke(() =>
                    {
                        this.btDryRunModel.Background = new SolidColorBrush(btDryRun ? Colo_ON_GR : Colo_OFF);
                        this.btStepRunOn.Background = new SolidColorBrush(btStepRun ? Colo_ON_GR : Colo_OFF);
                        this.btDisableAlarm.Background = new SolidColorBrush(btDisableAlarm ? Colo_ON_GR : Colo_OFF);
                        this.btDisableBuzzer.Background = new SolidColorBrush(btDisableBuzzer ? Colo_ON_GR : Colo_OFF);
                        this.btDisableDoorSensor.Background = new SolidColorBrush(btDisableDoorSensor ? Colo_ON_GR : Colo_OFF);
                        this.btDisabaleSafetySensor.Background = new SolidColorBrush(btDisableSafety ? Colo_ON_GR : Colo_OFF);

                        this.btBypassDetectCheckJig.Background = new SolidColorBrush(btByPassCheckJig ? Colo_ON_GR : Colo_OFF);
                        this.btBypassVacuumCheck.Background = new SolidColorBrush(btByPassVacuumCheck ? Colo_ON_GR : Colo_OFF);
                        this.btDisableVacuum.Background = new SolidColorBrush(btDisableVacuum ? Colo_ON_GR : Colo_OFF);

                        this.btDisableCH1.Background = new SolidColorBrush(btCH1 ? Colo_ON_GR : Colo_OFF);
                        this.btDisableCH2.Background = new SolidColorBrush(btCH2 ? Colo_ON_GR : Colo_OFF);
                        this.btManualRun.Background = new SolidColorBrush(btManualRun ? Colo_ON_GR : Colo_OFF);

                        this.tbxDryRunModel.Text = btDryRun ? "ON" : "OFF";
                        this.tbxStepRunOn.Text = btStepRun ? "ON" : "OFF";
                        this.tbxDisableAlarm.Text = btDisableAlarm ? "ON" : "OFF";
                        this.tbxDisableBuzzer.Text = btDisableBuzzer ? "ON" : "OFF";
                        this.tbxDisableDoorSensor.Text = btDisableDoorSensor ? "ON" : "OFF";
                        this.tbxDisabaleSafetySensor.Text = btDisableSafety ? "ON" : "OFF";

                        this.tbxBypassDetectCheckJig.Text = btByPassCheckJig ? "ON" : "OFF";
                        this.tbxBypassVacuumCheck.Text = btByPassVacuumCheck ? "ON" : "OFF";
                        this.tbxBypassVacuumCheck.Text = btDisableVacuum ? "ON" : "OFF";

                        this.tbxDisableCH1.Text = btCH1 ? "ON" : "OFF";
                        this.tbxDisableCH2.Text = btCH2 ? "ON" : "OFF";
                        this.tbxManualRun.Text = btManualRun ? "ON" : "OFF";

                    });
                 }
                Thread.Sleep(1);
            }
        }
        public bool GetManualRun()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_MANUAL_RUN);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DRYRUM_MODE: " + ex.Message));
                return false;
            }
        }
        public bool GetCH2()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_CH2);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DRYRUM_MODE: " + ex.Message));
                return false;
            }
        }
        public bool GetCH1()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_CH1);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DRYRUM_MODE: " + ex.Message));
                return false;
            }
        }

        public bool GetLampDryrun()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DRYRUM_MODE);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DRYRUM_MODE: " + ex.Message));
                return false;
            }
        }
        public bool GetLampStepRun()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_STEP_RUN_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_STEP_RUN_ON: " + ex.Message));
                return false;
            }
        }
        public bool GetLampDisableAlarm()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_ALARM);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DISABLE_ALARM: " + ex.Message));
                return false;
            }
        }
        public bool GetLampDisableBuzzer()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_BUZZER);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DISABLE_BUZZER: " + ex.Message));
                return false;
            }
        }
        public bool GetLampDoorSensor()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_DOOR_SENSORS);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DISABLE_DOOR_SENSORS: " + ex.Message));
                return false;
            }
        }
        public bool GetLampDoorSafery()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_SAFETY_SENSORS);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DISABLE_SAFETY_SENSORS: " + ex.Message));
                return false;
            }
        }
        public bool GetLampDetectJig()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_BYPASS_DETECT_CHECK_JIG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_BYPASS_DETECT_CHECK_JIG: " + ex.Message));
                return false;
            }
        }
        public bool GetLampVacuumCheck()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_BYPASS_VACUUMM_CHECK);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_BYPASS_VACUUMM_CHECK: " + ex.Message));
                return false;
            }
        }
        public bool GetLampVacuum()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_DISABLE_VACUUMM);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("K_LAMP_DISABLE_VACUUMM: " + ex.Message));
                return false;
            }
        }
        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
          
            UiManager.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
         
            UiManager.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU);
        }
    }
}
