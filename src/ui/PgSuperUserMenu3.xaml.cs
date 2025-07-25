
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for PgSuperUserMenu3.xaml
    /// </summary>
    public partial class PgSuperUserMenu3 : Page
    {

        private List<int> D_ListUpdateDevicePLC_2000 = new List<int>();
        private MyLogger logger = new MyLogger("PG_SUPER_USE_SERVO_SETUP");
        public PgSuperUserMenu3()
        {
            InitializeComponent();
            this.Loaded += PgSuperUserMenu3_Loaded;
            this.Unloaded += PgSuperUserMenu3_Unloaded;
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.btSetting3.Click += BtSetting3_Click;
            this.btSetting4.Click += BtSetting4_Click;
           
            this.BtnSave.Click += BtnSave_Click;



            this.TbxDevice1.TouchDown += TBNotCheck_TouchDown;
            this.TbxDevice1.PreviewMouseDown += TBNotCheck_PreviewMouseDown;

            this.TbxDevice2.TouchDown += TBNotCheck_TouchDown;
            this.TbxDevice2.PreviewMouseDown += TBNotCheck_PreviewMouseDown;

            this.TbxDevice3.TouchDown += TB_TouchDown;
            this.TbxDevice3.PreviewMouseDown += TB_PreviewMouseDown;

            this.TbxDevice4.TouchDown += TB_TouchDown;
            this.TbxDevice4.PreviewMouseDown += TB_PreviewMouseDown;

            this.TbxDevice5.TouchDown += TB_TouchDown;
            this.TbxDevice5.PreviewMouseDown += TB_PreviewMouseDown;

            this.TbxDevice19.TouchDown += TBNotCheck_TouchDown;
            this.TbxDevice19.PreviewMouseDown += TBNotCheck_PreviewMouseDown;

            this.TbxDevice20.TouchDown += TBNotCheck_TouchDown;
            this.TbxDevice20.PreviewMouseDown += TBNotCheck_PreviewMouseDown;

            this.TbxDevice21.TouchDown += TB_TouchDown;
            this.TbxDevice21.PreviewMouseDown += TB_PreviewMouseDown;

            this.TbxDevice22.TouchDown += TB_TouchDown;
            this.TbxDevice22.PreviewMouseDown += TB_PreviewMouseDown;

            this.TbxDevice23.TouchDown += TB_TouchDown;
            this.TbxDevice23.PreviewMouseDown += TB_PreviewMouseDown;

        }



        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm comfirmYesNo = new WndConfirm();
            if (!comfirmYesNo.DoComfirmYesNo("You Want Save Setting?")) return;


            int Ax1_ACC_TIME = Convert.ToInt32(Math.Round(Convert.ToDouble(TbxDevice1.Text)));
            int Ax1_DEC_TIME = Convert.ToInt32(Math.Round(Convert.ToDouble(TbxDevice2.Text)));

            int Ax2_ACC_TIME = Convert.ToInt32(Math.Round(Convert.ToDouble(TbxDevice19.Text)));
            int Ax2_DEC_TIME = Convert.ToInt32(Math.Round(Convert.ToDouble(TbxDevice20.Text)));

            int Ax1_SPEED_LIMIT_ALL = (int)(Convert.ToDouble(TbxDevice3.Text) * 1);
            int Ax1_SPEED_ORG = (int)(Convert.ToDouble(TbxDevice4.Text) * 1);
            int Ax1_SPEED_JOG = (int)(Convert.ToDouble(TbxDevice5.Text) * 1);

            int Ax2_SPEED_LIMIT_ALL = (int)(Convert.ToDouble(TbxDevice21.Text) * 1);
            int Ax2_SPEED_ORG = (int)(Convert.ToDouble(TbxDevice22.Text) * 1);
            int Ax2_SPEED_JOG = (int)(Convert.ToDouble(TbxDevice23.Text) * 1);



            if (UiManager.PLC.IsConnected)
            {
                var ACC_AX1 = (UInt32)(Ax1_ACC_TIME);
                var DEC_AX1 = (UInt32)(Ax1_DEC_TIME);
                var SPEED_LIMIT_AX1 = (UInt32)(Ax1_SPEED_LIMIT_ALL);
                var SPEED_ORG_AX1 = (UInt32)(Ax1_SPEED_ORG);
                var SPEED_JOG_AX1 = (UInt32)(Ax1_SPEED_JOG);

                var ACC_AX2 = (UInt32)(Ax2_ACC_TIME);
                var DEC_AX2 = (UInt32)(Ax2_DEC_TIME);
                var SPEED_LIMIT_AX2 = (UInt32)(Ax2_SPEED_LIMIT_ALL);
                var SPEED_ORG_AX2 = (UInt32)(Ax2_SPEED_ORG);
                var SPEED_JOG_AX2 = (UInt32)(Ax2_SPEED_JOG);

                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ACC_TIME_AX1, ACC_AX1);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DEC_TIME_AX1, DEC_AX1);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_SPEED_LIMIT_ALL_AX1, SPEED_LIMIT_AX1);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ORG_SPEED_AX1, SPEED_ORG_AX1);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_JOG_SPEED_AX1, SPEED_JOG_AX1);

                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ACC_TIME_AX2, ACC_AX2);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DEC_TIME_AX2, DEC_AX2);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_SPEED_LIMIT_ALL_AX2, SPEED_LIMIT_AX2);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ORG_SPEED_AX2, SPEED_ORG_AX2);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_JOG_SPEED_AX2, SPEED_JOG_AX2);

                WndMessenger ShowMessenger = new WndMessenger();
                ShowMessenger.MessengerShow("Messenger : Save Data Successfully ");


            }
            else
            {
                WndMessenger ShowMessenger = new WndMessenger();
                ShowMessenger.MessengerShow("Messenger : Save Data Was NOT Successful ");
            }
            this.UpdateOneShotPLC();
        }

        private void PgSuperUserMenu3_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private async void PgSuperUserMenu3_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);
            this.UpdateOneShotPLC();
        }

        private void TB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    TextBox_TextChanged(textbox, new RoutedEventArgs());
            //}
        }

        private void TB_TouchDown(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    TextBox_TextChanged(textbox, new RoutedEventArgs());
            //}
        }

        private void TBNotCheck_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;

            //}
        }

        private void TBNotCheck_TouchDown(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;

            //}
        }

        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrWhiteSpace(textBox.Text) && float.TryParse(textBox.Text, out float number))
            {
                if (!textBox.Text.Contains('.'))
                {
                    // Thêm "0.000" sau dấu phẩy cho số nguyên
                    textBox.Text = $"{textBox.Text}.00";
                }
                else
                {
                    string[] parts = textBox.Text.Split('.');
                    if (parts.Length == 2)
                    {
                        if (parts[1].Length > 2)
                        {
                            // Chỉ lấy ba số sau dấu phẩy
                            textBox.Text = $"{parts[0]}.{parts[1].Substring(0, 2)}";
                        }
                        else if (parts[1].Length < 3)
                        {
                            // Nếu ít hơn ba số sau dấu phẩy, thêm số 0 cho đủ
                            textBox.Text = $"{parts[0]}.{parts[1].PadRight(2, '0')}";
                        }
                    }
                }
            }
            else
            {

            }


        }
        
        private void BtSetting4_Click(object sender, RoutedEventArgs e)
        {


            UiManager.SwitchPage(PAGE_ID.PAGE_SUPER_USER_MENU_SETTING_SERVO);

        }

        private void BtSetting3_Click(object sender, RoutedEventArgs e)
        {

            UiManager.SwitchPage(PAGE_ID.PAGE_SUPER_USER_MENU_SETTING_ALARM);
        }

        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {

            UiManager.SwitchPage(PAGE_ID.PAGE_SUPER_USER_MENU_DELAY_MACHINE);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {

            UiManager.SwitchPage(PAGE_ID.PAGE_SUPER_USER_MENU);
        }

        private void UpdateOneShotPLC()
        {
            if (UiManager.PLC.IsConnected)
            {
                var AccTimeAx1 = GetAccTimeAx1();
                var DecTimeAx1 = GetDecTimeAx1();
                var GetSpeedLimitAx1 = GetSpeedLimitAllAx1();
                var GetOrgAx1 = GetOrgSpeedAx1();
                var GetJogAx1 = GetJogSpeedAx1();

                var AccTimeAx2 = GetAccTimeAx2();
                var DecTimeAx2 = GetDecTimeAx2();
                var GetSpeedLimitAx2 = GetSpeedLimitAllAx2();
                var GetOrgAx2 = GetOrgSpeedAx2();
                var GetJogAx2 = GetJogSpeedAx2();

                Dispatcher.Invoke(() =>
                {
                    this.TbxDevice1.Text = AccTimeAx1.ToString();
                    this.TbxDevice2.Text = DecTimeAx1.ToString();
                    this.TbxDevice3.Text = GetSpeedLimitAx1.ToString();
                    this.TbxDevice4.Text = GetOrgAx1.ToString();
                    this.TbxDevice5.Text = GetJogAx1.ToString();

                    this.TbxDevice19.Text = AccTimeAx2.ToString();
                    this.TbxDevice20.Text = DecTimeAx2.ToString();
                    this.TbxDevice21.Text = GetSpeedLimitAx2.ToString();
                    this.TbxDevice22.Text = GetOrgAx2.ToString();
                    this.TbxDevice23.Text = GetJogAx2.ToString();
                });
            }
           
        }


        public double GetAccTimeAx1()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ACC_TIME_AX1) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ACC_TIME_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetDecTimeAx1()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DEC_TIME_AX1) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_DEC_TIME_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetSpeedLimitAllAx1()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_SPEED_LIMIT_ALL_AX1) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_SPEED_LIMIT_ALL_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetOrgSpeedAx1()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ORG_SPEED_AX1) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ORG_SPEED_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetJogSpeedAx1()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_JOG_SPEED_AX1) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ORG_SPEED_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }


        public double GetAccTimeAx2()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ACC_TIME_AX2) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ACC_TIME_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetDecTimeAx2()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DEC_TIME_AX2) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_DEC_TIME_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetSpeedLimitAllAx2()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_SPEED_LIMIT_ALL_AX2) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_SPEED_LIMIT_ALL_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetOrgSpeedAx2()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ORG_SPEED_AX2) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ORG_SPEED_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetJogSpeedAx2()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_JOG_SPEED_AX2) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_ORG_SPEED_AX1: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public string FormatNumber(long number)
        {
            bool isNegative = number < 0;
            double dividedNumber = number / 100.0;
            string numberStr = Math.Abs(dividedNumber).ToString("F2"); // Format with 2 decimal places

            string[] parts = numberStr.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts.Length > 1 ? parts[1] : "00";

            string formattedIntegerPart = "";
            while (integerPart.Length > 3)
            {
                formattedIntegerPart = "," + integerPart.Substring(integerPart.Length - 3) + formattedIntegerPart;
                integerPart = integerPart.Substring(0, integerPart.Length - 3);
            }
            formattedIntegerPart = integerPart + formattedIntegerPart;

            string formatted = formattedIntegerPart + "." + decimalPart;

            if (isNegative)
            {
                formatted = "-" + formatted;
            }
            return formatted;
        }
    }
}
