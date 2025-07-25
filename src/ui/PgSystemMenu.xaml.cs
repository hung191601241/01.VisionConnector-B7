
using ITM_Semiconductor;
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
    /// Interaction logic for PgSystemMenu.xaml
    /// </summary>
    public partial class PgSystemMenu : Page
    {
        MyLogger logger = new MyLogger("PG_SYSTEM_MES");
        RunSettings runSettings;
        RunSettings oldSettings;
        public PgSystemMenu()
        {
            InitializeComponent();
            this.Loaded += PgSystemMenu_Loaded;
            this.Unloaded += PgSystemMenu_Unloaded;
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;


            this.btMesOnline.Click += BtMesOnline_Click;
            this.btMesOffline.Click += BtMesOffline_Click;

            this.btMixLottON.Click += BtMixLottON_Click;
            this.btMixLottOFF.Click += BtMixLottOFF_Click;

            this.btDoubleCodeON.Click += BtDoubleCodeON_Click;
            this.btDoubleCodeOFF.Click += BtDoubleCodeOFF_Click;

            this.btByPassVisionON.Click += BtByPassVisionON_Click;
            this.btByPassVisionOFF.Click += BtByPassVisionOFF_Click;

            this.TbxNumberScannerTrigger.PreviewMouseDown += TbxNumberScannerTrigger_PreviewMouseDown;
            this.TbxNumberScannerTrigger.TouchDown += TbxNumberScannerTrigger_TouchDown;
            this.BtnUndo.Click += BtnUndo_Click;
        }

        private void BtByPassVisionOFF_Click(object sender, RoutedEventArgs e)
        {
            runSettings.ByPassVision = false;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtByPassVisionON_Click(object sender, RoutedEventArgs e)
        {
            runSettings.ByPassVision = true;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtDoubleCodeOFF_Click(object sender, RoutedEventArgs e)
        {
            runSettings.CheckDoubleCode = false;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtDoubleCodeON_Click(object sender, RoutedEventArgs e)
        {
            runSettings.CheckDoubleCode = true;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtMixLottOFF_Click(object sender, RoutedEventArgs e)
        {
            runSettings.CheckMixLot = false;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtMixLottON_Click(object sender, RoutedEventArgs e)
        {
            runSettings.CheckMixLot = true;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void TbxNumberScannerTrigger_TouchDown(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    //runSettings.scannerNumberTrigger = Convert.ToInt32(TbxNumberScannerTrigger.Text);
            //    UiManager.SaveAppSettings();
            //}
        }

        private void TbxNumberScannerTrigger_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    //runSettings.scannerNumberTrigger = Convert.ToInt32(TbxNumberScannerTrigger.Text);
            //    UiManager.SaveAppSettings();
            //}
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Messenger :Are you sure to UNDO all changes? ?")) return;

            runSettings = this.oldSettings.Clone();

            UiManager.SaveAppSettings();
            MessageBox.Show("UNDO done!");
            this.UpdateUI();
        }

        private void BtMesOffline_Click(object sender, RoutedEventArgs e)
        {
            runSettings.mesOnline = false;
            UiManager.SaveAppSettings();
            UpdateUI();
        }

        private void BtMesOnline_Click(object sender, RoutedEventArgs e)
        {
            runSettings.mesOnline = true;
            UiManager.SaveAppSettings();
            this.UpdateUI();
        }

        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
       ;
            UiManager.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
            
            UiManager.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU);
        }

        private void PgSystemMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            runSettings.scannerNumberTrigger = Convert.ToInt32(TbxNumberScannerTrigger.Text);
            UiManager.SaveAppSettings();
        }

        private void PgSystemMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.runSettings = UiManager.appSettings.run;
            this.oldSettings = UiManager.appSettings.run.Clone();
            this.UpdateUI();
        }
        private void UpdateUI()
        {
            if (runSettings.CheckMixLot)
            {
                this.btMixLottON.Background = Brushes.LightGreen;
                this.btMixLottOFF.Background = Brushes.White;
            }
            else
            {
                this.btMixLottON.Background = Brushes.White;
                this.btMixLottOFF.Background = Brushes.Orange;
            }

            if (runSettings.CheckDoubleCode)
            {
                this.btDoubleCodeON.Background = Brushes.LightGreen;
                this.btDoubleCodeOFF.Background = Brushes.White;
            }
            else
            {
                this.btDoubleCodeON.Background = Brushes.White;
                this.btDoubleCodeOFF.Background = Brushes.Orange;
            }

            if (runSettings.mesOnline)
            {
                this.btMesOnline.Background = Brushes.LightGreen;
                this.btMesOffline.Background = Brushes.White;
            }
            else
            {
                this.btMesOnline.Background = Brushes.White;
                this.btMesOffline.Background = Brushes.Orange;
            }
            if (runSettings.ByPassVision)
            {
                this.btByPassVisionON.Background = Brushes.LightGreen;
                this.btByPassVisionOFF.Background = Brushes.White;
            }
            else
            {
                this.btByPassVisionON.Background = Brushes.White;
                this.btByPassVisionOFF.Background = Brushes.Orange;
            }

            TbxNumberScannerTrigger.Text = runSettings.scannerNumberTrigger.ToString();
        }
    }
}
