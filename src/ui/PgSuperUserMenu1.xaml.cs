﻿using System;
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
    /// Interaction logic for PgSuperUserMenu1.xaml
    /// </summary>
    public partial class PgSuperUserMenu1 : Page
    {

        MyLogger logger = new MyLogger("PagePgSuperUserMenu1");



        #region khai bao bien so sanh
        float MinDevive01 = 0;
        float MinDevive02 = 0;
        float MinDevive03 = 0;
        float MinDevive04 = 0;
        float MinDevive05 = 0;
        float MinDevive06 = 0;
        float MinDevive07 = 0;
        float MinDevive08 = 0;
        float MinDevive09 = 0;
        float MinDevive10 = 0;
        float MinDevive11 = 0;
        float MinDevive12 = 0;
        float MinDevive13 = 0;
        float MinDevive14 = 0;
        float MinDevive15 = 0;
        float MinDevive16 = 0;
        float MinDevive17 = 0;
        float MinDevive18 = 0;
        float MinDevive19 = 0;
        float MinDevive20 = 0;
        float MinDevive21 = 0;
        float MinDevive22 = 0;
        float MinDevive23 = 0;
        float MinDevive24 = 0;
        float MinDevive25 = 0;
        float MinDevive26 = 0;
        float MinDevive27 = 0;
        float MinDevive28 = 0;
        float MinDevive29 = 0;
        float MinDevive30 = 0;
        float MinDevive31 = 0;
        float MinDevive32 = 0;
        float MinDevive33 = 0;
        float MinDevive34 = 0;
        float MinDevive35 = 0;
        float MinDevive36 = 0;

        float MaxDevice01 = 100;
        float MaxDevice02 = 100;
        float MaxDevice03 = 100;
        float MaxDevice04 = 100;
        float MaxDevice05 = 100;
        float MaxDevice06 = 100;
        float MaxDevice07 = 100;
        float MaxDevice08 = 100;
        float MaxDevice09 = 100;
        float MaxDevice10 = 100;
        float MaxDevice11 = 100;
        float MaxDevice12 = 100;
        float MaxDevice13 = 100;
        float MaxDevice14 = 100;
        float MaxDevice15 = 100;
        float MaxDevice16 = 100;
        float MaxDevice17 = 100;
        float MaxDevice18 = 100;
        float MaxDevice19 = 100;
        float MaxDevice20 = 100;
        float MaxDevice21 = 100;
        float MaxDevice22 = 100;
        float MaxDevice23 = 100;
        float MaxDevice24 = 100;
        float MaxDevice25 = 999;
        float MaxDevice26 = 100;
        float MaxDevice27 = 100;
        float MaxDevice28 = 100;
        float MaxDevice29 = 100;
        float MaxDevice30 = 100;
        float MaxDevice31 = 100;
        float MaxDevice32 = 100;
        float MaxDevice33 = 100;
        float MaxDevice34 = 100;
        float MaxDevice35 = 100;
        float MaxDevice36 = 100;
        #endregion


        public PgSuperUserMenu1()
        {
            InitializeComponent();
            this.Loaded += PgSuperUserMenu1_Loaded;
            this.Unloaded += PgSuperUserMenu1_Unloaded;
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.btSetting3.Click += BtSetting3_Click;
            this.btSetting4.Click += BtSetting4_Click;
          
            this.BtnSave.Click += BtnSave_Click;


            #region Event TouchDown
            //this.TbxDevice01.PreviewMouseDown += TbxDevice01_PreviewMouseDown;
            this.tbxDevice1.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice2.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice3.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice4.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice5.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice6.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice7.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice8.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice9.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice10.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice11.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice12.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice13.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice14.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice15.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice16.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice17.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice18.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice19.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice20.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice21.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice22.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice23.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice24.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice25.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice26.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice27.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice28.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice29.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice30.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice31.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice32.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice33.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice34.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice35.TouchDown += TbxDevice_TouchDown;
            this.tbxDevice36.TouchDown += TbxDevice_TouchDown;

            this.tbxDevice1.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice2.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice3.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice4.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice5.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice6.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice7.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice8.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice9.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice10.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice11.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice12.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice13.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice14.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice15.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice16.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice17.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice18.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice19.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice20.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice21.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice22.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice23.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice24.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice25.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice26.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice27.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice28.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice29.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice30.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice31.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice32.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice33.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice34.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice35.PreviewMouseDown += TbxDevice_PreviewMouseDown;
            this.tbxDevice36.PreviewMouseDown += TbxDevice_PreviewMouseDown;


            #endregion

        }


        private void TbxDevice_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    TextBox_TextChanged(textbox, new RoutedEventArgs());
            //}
        }


        private void PgSuperUserMenu1_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        private async void PgSuperUserMenu1_Loaded(object sender, RoutedEventArgs e)
        {

            this.Uploadtextbox();
            await Task.Delay(1);
            this.UpdateUI();

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm comfirmYesNo = new WndConfirm();
                if (!comfirmYesNo.DoComfirmYesNo("You Want Save Setting?")) return;

                short Time1 = Convert.ToInt16(Convert.ToDouble(tbxDevice1.Text) * 10);
                short Time2 = Convert.ToInt16(Convert.ToDouble(tbxDevice2.Text) * 10);
                short Time3 = Convert.ToInt16(Convert.ToDouble(tbxDevice3.Text) * 10);
                short Time4 = Convert.ToInt16(Convert.ToDouble(tbxDevice4.Text) * 10);
                short Time5 = Convert.ToInt16(Convert.ToDouble(tbxDevice5.Text) * 10);

                short Time6 = Convert.ToInt16(Convert.ToDouble(tbxDevice6.Text) * 10);
                short Time7 = Convert.ToInt16(Convert.ToDouble(tbxDevice7.Text) * 10);
                short Time8 = Convert.ToInt16(Convert.ToDouble(tbxDevice8.Text) * 10);
                short Time9 = Convert.ToInt16(Convert.ToDouble(tbxDevice9.Text) * 10);
                short Time10 = Convert.ToInt16(Convert.ToDouble(tbxDevice10.Text) * 10);

                short Time11 = Convert.ToInt16(Convert.ToDouble(tbxDevice11.Text) * 10);
                short Time12 = Convert.ToInt16(Convert.ToDouble(tbxDevice12.Text) * 10);
                short Time13 = Convert.ToInt16(Convert.ToDouble(tbxDevice13.Text) * 10);
                short Time14 = Convert.ToInt16(Convert.ToDouble(tbxDevice14.Text) * 10);
                short Time15 = Convert.ToInt16(Convert.ToDouble(tbxDevice15.Text) * 10);

                short Time16 = Convert.ToInt16(Convert.ToDouble(tbxDevice16.Text));
                short Time17 = Convert.ToInt16(Convert.ToDouble(tbxDevice17.Text) * 10);
                short Time18 = Convert.ToInt16(Convert.ToDouble(tbxDevice18.Text) * 10);
                short Time19 = Convert.ToInt16(Convert.ToDouble(tbxDevice19.Text) * 10);
                short Time20 = Convert.ToInt16(Convert.ToDouble(tbxDevice20.Text));

                short Time21 = Convert.ToInt16(Convert.ToDouble(tbxDevice21.Text) * 10);
                short Time22 = Convert.ToInt16(Convert.ToDouble(tbxDevice22.Text) * 10);
                short Time23 = Convert.ToInt16(Convert.ToDouble(tbxDevice23.Text));
                short Time24 = Convert.ToInt16(Convert.ToDouble(tbxDevice24.Text) * 10);
                short Time25 = Convert.ToInt16(Convert.ToDouble(tbxDevice25.Text) * 10);



                if (UiManager.PLC.IsConnected)
                {
                    

                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DOOR_SEN_ANTI_JAMMING, Time1);
                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AREA_SEN_ANTI_JAMMING, Time2);
                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DOOR_SENSOR_ERR_TIMEOUT, Time3);
                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AREA_SENSOR_ERR_TIMEOUT, Time4);
                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_INPUT_DETECT_JIG_DELAY_TIME, Time5);
                    UiManager.PLC.WriteWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_INPUT_DETECT_CHECK_JIG_DELAY_TIME, Time6);

                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow("Messenger : Save Data Successfully ");

                }
                else
                {
                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow("Messenger : Save Data Fail ");
                }
                this.UpdateUI();
            }
            catch (Exception ex)
            {
                logger.Create($"BtnSave_Click: {ex.Message}");
            }

        }


        private async void UpdateUI()
        {
            await Task.Run(() =>
            {
                if (UiManager.PLC.IsConnected == true)
                {
                    var ReadDevice01 = GetTimeDoorSenAnti();
                    var ReadDevice02 = GetTimeAreaSenAnti();
                    var ReadDevice03 = GetTimeDoorSensorError();
                    var ReadDevice04 = GetTimeAreaSensorError();
                    var ReadDevice05 = GetTimeInputDetectJig();

                    var ReadDevice06 = GetTimeInputDetectCheckJig();
                   

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.tbxDevice1.Text = "Spare";
                        this.tbxDevice2.Text = "Spare";
                        this.tbxDevice3.Text = "Spare";
                        this.tbxDevice4.Text = "Spare";
                        this.tbxDevice5.Text = "Spare";
                        this.tbxDevice6.Text = "Spare";
                        this.tbxDevice7.Text = "Spare";
                        this.tbxDevice8.Text = "Spare";
                        this.tbxDevice9.Text = "Spare";
                        this.tbxDevice10.Text = "Spare";
                        this.tbxDevice11.Text = "Spare";
                        this.tbxDevice12.Text = "Spare";
                        this.tbxDevice13.Text = "Spare";
                        this.tbxDevice14.Text = "Spare";
                        this.tbxDevice15.Text = "Spare";
                        this.tbxDevice16.Text = "Spare";
                        this.tbxDevice17.Text = "Spare";
                        this.tbxDevice18.Text = "Spare";
                        this.tbxDevice19.Text = "Spare";
                        this.tbxDevice20.Text = "Spare";
                        this.tbxDevice21.Text = "Spare";
                        this.tbxDevice22.Text = "Spare";
                        this.tbxDevice23.Text = "Spare";
                        this.tbxDevice24.Text = "Spare";
                        this.tbxDevice25.Text = "Spare";
                        this.tbxDevice26.Text = "Spare";
                        this.tbxDevice27.Text = "Spare";
                        this.tbxDevice28.Text = "Spare";
                        this.tbxDevice29.Text = "Spare";
                        this.tbxDevice30.Text = "Spare";
                        this.tbxDevice31.Text = "Spare";
                        this.tbxDevice32.Text = "Spare";
                        this.tbxDevice33.Text = "Spare";
                        this.tbxDevice34.Text = "Spare";
                        this.tbxDevice35.Text = "Spare";
                        this.tbxDevice36.Text = "Spare";

                        this.tbxDevice1.Text = (ReadDevice01).ToString("F1");
                        this.tbxDevice2.Text = (ReadDevice02).ToString("F1");
                        this.tbxDevice3.Text = (ReadDevice03).ToString("F1");
                        this.tbxDevice4.Text = (ReadDevice04).ToString("F1");
                        this.tbxDevice5.Text = (ReadDevice05).ToString("F1");

                        this.tbxDevice6.Text = (ReadDevice06).ToString("F1");
                       



                    }));
                }

            });
            if (UiManager.PLC.IsConnected != true)
            {
                for (int i = 1; i <= 36; i++)
                {
                    string textBoxName = "tbxDevice" + i;
                    TextBox textBox = this.FindName(textBoxName) as TextBox;
                    if (textBox != null)
                    {
                        textBox.Text = "Error";
                    }
                }
                
            }


        }

        public double GetTimeDoorSenAnti()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DOOR_SEN_ANTI_JAMMING) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_DOOR_SEN_ANTI_JAMMING: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetTimeAreaSenAnti()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AREA_SEN_ANTI_JAMMING) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_DOOR_SEN_ANTI_JAMMING: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetTimeDoorSensorError()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_DOOR_SENSOR_ERR_TIMEOUT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_DOOR_SENSOR_ERR_TIMEOUT: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetTimeAreaSensorError()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AREA_SENSOR_ERR_TIMEOUT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_AREA_SENSOR_ERR_TIMEOUT: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetTimeInputDetectJig()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_INPUT_DETECT_JIG_DELAY_TIME) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_INPUT_DETECT_JIG_DELAY_TIME: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetTimeInputDetectCheckJig()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_INPUT_DETECT_CHECK_JIG_DELAY_TIME) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("D_INPUT_DETECT_CHECK_JIG_DELAY_TIME: " + ex.Message));
                return ret;
            }
            return ret;
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
        #region Event TouchDown
        private void TbxDevice_TouchDown(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    TextBox_TextChanged(textbox, new RoutedEventArgs());
            //}

        }
        #endregion
        #region sosanh
        private void Uploadtextbox()
        {
            this.LbMin01.Content = this.MinDevive01.ToString();
            this.LbMin02.Content = this.MinDevive02.ToString();
            this.LbMin03.Content = this.MinDevive03.ToString();
            this.LbMin04.Content = this.MinDevive04.ToString();
            this.LbMin05.Content = this.MinDevive05.ToString();
            this.LbMin06.Content = this.MinDevive06.ToString();
            this.LbMin07.Content = this.MinDevive07.ToString();
            this.LbMin08.Content = this.MinDevive08.ToString();
            this.LbMin09.Content = this.MinDevive09.ToString();
            this.LbMin10.Content = this.MinDevive10.ToString();
            this.LbMin11.Content = this.MinDevive11.ToString();
            this.LbMin12.Content = this.MinDevive12.ToString();
            this.LbMin13.Content = this.MinDevive13.ToString();
            this.LbMin14.Content = this.MinDevive14.ToString();
            this.LbMin15.Content = this.MinDevive15.ToString();
            this.LbMin16.Content = this.MinDevive16.ToString();
            this.LbMin17.Content = this.MinDevive17.ToString();
            this.LbMin18.Content = this.MinDevive18.ToString();
            this.LbMin19.Content = this.MinDevive19.ToString();
            this.LbMin20.Content = this.MinDevive20.ToString();
            this.LbMin21.Content = this.MinDevive21.ToString();
            this.LbMin22.Content = this.MinDevive22.ToString();
            this.LbMin23.Content = this.MinDevive23.ToString();
            this.LbMin24.Content = this.MinDevive24.ToString();
            this.LbMin25.Content = this.MinDevive25.ToString();
            this.LbMin26.Content = this.MinDevive26.ToString();
            this.LbMin27.Content = this.MinDevive27.ToString();
            this.LbMin28.Content = this.MinDevive28.ToString();
            this.LbMin29.Content = this.MinDevive29.ToString();
            this.LbMin30.Content = this.MinDevive30.ToString();
            this.LbMin31.Content = this.MinDevive31.ToString();
            this.LbMin32.Content = this.MinDevive32.ToString();
            this.LbMin33.Content = this.MinDevive33.ToString();
            this.LbMin34.Content = this.MinDevive34.ToString();
            this.LbMin35.Content = this.MinDevive35.ToString();
            this.LbMin36.Content = this.MinDevive36.ToString();

            this.LbMax01.Content = this.MaxDevice01.ToString();
            this.LbMax02.Content = this.MaxDevice02.ToString();
            this.LbMax03.Content = this.MaxDevice03.ToString();
            this.LbMax04.Content = this.MaxDevice04.ToString();
            this.LbMax05.Content = this.MaxDevice05.ToString();
            this.LbMax06.Content = this.MaxDevice06.ToString();
            this.LbMax07.Content = this.MaxDevice07.ToString();
            this.LbMax08.Content = this.MaxDevice08.ToString();
            this.LbMax09.Content = this.MaxDevice09.ToString();
            this.LbMax10.Content = this.MaxDevice10.ToString();
            this.LbMax11.Content = this.MaxDevice11.ToString();
            this.LbMax12.Content = this.MaxDevice12.ToString();
            this.LbMax13.Content = this.MaxDevice13.ToString();
            this.LbMax14.Content = this.MaxDevice14.ToString();
            this.LbMax15.Content = this.MaxDevice15.ToString();
            this.LbMax16.Content = this.MaxDevice16.ToString();
            this.LbMax17.Content = this.MaxDevice17.ToString();
            this.LbMax18.Content = this.MaxDevice18.ToString();
            this.LbMax19.Content = this.MaxDevice19.ToString();
            this.LbMax20.Content = this.MaxDevice20.ToString();
            this.LbMax21.Content = this.MaxDevice21.ToString();
            this.LbMax22.Content = this.MaxDevice22.ToString();
            this.LbMax23.Content = this.MaxDevice23.ToString();
            this.LbMax24.Content = this.MaxDevice24.ToString();
            this.LbMax25.Content = this.MaxDevice25.ToString();
            this.LbMax26.Content = this.MaxDevice26.ToString();
            this.LbMax27.Content = this.MaxDevice27.ToString();
            this.LbMax28.Content = this.MaxDevice28.ToString();
            this.LbMax29.Content = this.MaxDevice29.ToString();
            this.LbMax30.Content = this.MaxDevice30.ToString();
            this.LbMax31.Content = this.MaxDevice31.ToString();
            this.LbMax32.Content = this.MaxDevice32.ToString();
            this.LbMax33.Content = this.MaxDevice33.ToString();
            this.LbMax34.Content = this.MaxDevice34.ToString();
            this.LbMax35.Content = this.MaxDevice35.ToString();
            this.LbMax36.Content = this.MaxDevice36.ToString();

        }
        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrWhiteSpace(textBox.Text) && float.TryParse(textBox.Text, out float number))
            {
                // Lấy điều kiện cụ thể cho TextBox hiện tại (ví dụ: dựa trên tên của TextBox)
                float minCondition = GetMinCondition(textBox.Name);
                float maxCondition = GetMaxCondition(textBox.Name);

                if (number >= minCondition && number <= maxCondition)
                {

                    textBox.Background = Brushes.Black; // Trở lại màu nền mặc định
                }
                if (number < minCondition)
                {
                    textBox.Background = Brushes.Red;
                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow($"Vui lòng nhập một số lớn hơn {minCondition} và nhỏ hơn {maxCondition} cho {textBox.Name}.");

                    textBox.Text = "";
                    textBox.Text = minCondition.ToString();
                    textBox.Background = Brushes.Black;
                }
                if (number > maxCondition)
                {
                    textBox.Background = Brushes.Red;
                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow($"Vui lòng nhập một số  lớn hơn {minCondition} và nhỏ hơn {maxCondition} cho {textBox.Name}.");

                    textBox.Text = "";
                    textBox.Text += maxCondition.ToString();
                    textBox.Background = Brushes.Black;
                }
            }
            else
            {

                textBox.Background = Brushes.Red;
                textBox.Text = "0";
                textBox.Background = Brushes.Black;
            }
            if (textBox.Text.Contains('.'))
            {
                string[] parts = textBox.Text.Split('.');
                if (parts.Length == 2 && parts[1].Length > 1)
                {
                    textBox.Text = $"{parts[0]}.{parts[1][0]}"; // Chỉ lấy một số sau dấu phẩy
                }
            }



        }
        private float GetMinCondition(string textBoxName)
        {
            switch (textBoxName)
            {
                case "tbxDevice1":
                    return MinDevive01;
                case "tbxDevice2":
                    return MinDevive02;
                case "tbxDevice3":
                    return MinDevive03;
                case "tbxDevice4":
                    return MinDevive04;
                case "tbxDevice5":
                    return MinDevive05;
                case "tbxDevice6":
                    return MinDevive06;
                case "tbxDevice7":
                    return MinDevive07;
                case "tbxDevice8":
                    return MinDevive08;
                case "tbxDevice9":
                    return MinDevive09;
                case "tbxDevice10":
                    return MinDevive10;
                case "tbxDevice11":
                    return MinDevive11;
                case "tbxDevice12":
                    return MinDevive12;
                case "tbxDevice13":
                    return MinDevive13;
                case "tbxDevice14":
                    return MinDevive14;
                case "tbxDevice15":
                    return MinDevive15;
                case "tbxDevice16":
                    return MinDevive16;
                case "tbxDevice17":
                    return MinDevive17;
                case "tbxDevice18":
                    return MinDevive18;
                case "tbxDevice19":
                    return MinDevive19;
                case "tbxDevice20":
                    return MinDevive20;
                case "tbxDevice21":
                    return MinDevive21;
                case "tbxDevice22":
                    return MinDevive22;
                case "tbxDevice23":
                    return MinDevive23;
                case "tbxDevice24":
                    return MinDevive24;
                case "tbxDevice25":
                    return MinDevive25;
                case "tbxDevice26":
                    return MinDevive26;
                case "tbxDevice27":
                    return MinDevive27;
                case "tbxDevice28":
                    return MinDevive28;
                case "tbxDevice29":
                    return MinDevive30;
                case "tbxDevice31":
                    return MinDevive31;
                case "tbxDevice32":
                    return MinDevive32;
                case "tbxDevice33":
                    return MinDevive34;
                case "tbxDevice35":
                    return MinDevive35;
                case "tbxDevice36":
                    return MinDevive36;

                default:
                    return 0;
            }
        }


        private float GetMaxCondition(string textBoxName)
        {

            switch (textBoxName)
            {
                case "tbxDevice1":
                    return MaxDevice01;
                case "tbxDevice2":
                    return MaxDevice02;
                case "tbxDevice3":
                    return MaxDevice03;
                case "tbxDevice4":
                    return MaxDevice04;
                case "tbxDevice5":
                    return MaxDevice05;
                case "tbxDevice6":
                    return MaxDevice06;
                case "tbxDevice7":
                    return MaxDevice07;
                case "tbxDevice8":
                    return MaxDevice08;
                case "tbxDevice9":
                    return MaxDevice09;
                case "tbxDevice10":
                    return MaxDevice10;
                case "tbxDevice11":
                    return MaxDevice12;
                case "tbxDevice13":
                    return MaxDevice13;
                case "tbxDevice14":
                    return MaxDevice14;
                case "tbxDevice15":
                    return MaxDevice15;
                case "tbxDevice16":
                    return MaxDevice16;
                case "tbxDevice17":
                    return MaxDevice17;
                case "tbxDevice18":
                    return MaxDevice18;
                case "tbxDevice19":
                    return MaxDevice19;
                case "tbxDevice20":
                    return MaxDevice20;
                case "tbxDevice21":
                    return MaxDevice21;
                case "tbxDevice22":
                    return MaxDevice22;
                case "tbxDevice23":
                    return MaxDevice23;
                case "tbxDevice24":
                    return MaxDevice24;
                case "tbxDevice25":
                    return MaxDevice25;
                case "tbxDevice26":
                    return MaxDevice26;
                case "tbxDevice27":
                    return MaxDevice27;
                case "tbxDevice28":
                    return MaxDevice28;
                case "tbxDevice29":
                    return MaxDevice29;
                case "tbxDevice30":
                    return MaxDevice30;
                case "tbxDevice31":
                    return MaxDevice31;
                case "tbxDevice32":
                    return MaxDevice32;
                case "tbxDevice33":
                    return MaxDevice33;
                case "tbxDevice34":
                    return MaxDevice34;
                case "tbxDevice35":
                    return MaxDevice35;
                case "tbxDevice36":
                    return MaxDevice36;
                default:
                    return 50;
            }
        }
        #endregion
    }
}
