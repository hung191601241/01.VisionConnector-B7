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
    /// Interaction logic for PgMechanicalMenu2.xaml
    /// </summary>
    public partial class PgMechanicalMenu1 : Page
    {
        private MyLogger logger = new MyLogger("PG_Mechanical_ScannerTCP1");
        public PgMechanicalMenu1()
        {
            InitializeComponent();
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.btSetting3.Click += BtSetting3_Click;
            this.btSetting4.Click += BtSetting4_Click;
            this.BtnSave.Click += BtnSave_Click;

            this.Loaded += PgMechanicalMenu1_Loaded;
            this.btnReadQrCode.Click += BtnReadQrCode_Click;

        }

        private void BtSetting4_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_MECHANICAL_MES);
        }

        private void BtnReadQrCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UiManager.Scanner1.IsConnected)
                {
                    UserManager.createUserLog(UserActions.MN_MECHANICAL_BARCODE_READING_PKG);

                    // Select Bank 01:


                    // Signal User:
                    this.UpdateLogs("Reading ...");

                    Thread.Sleep(10);

                    // Start reading:
                    var qr = UiManager.Scanner1.ReadQR();
                    this.UpdateLogs(qr);
                }

            }
            catch (Exception ex)
            {
                logger.Create(("BtPkgRead_Click error:" + ex.Message));
            }
        }

        private void PgMechanicalMenu1_Loaded(object sender, RoutedEventArgs e)
        {
            if (UiManager.Scanner1.IsConnected)
            {
                this.UpdateLogs("Scanner TCP CONNECTED SUCCESSFULLY");
            }
            else
            {
                this.UpdateLogs("Scanner TCP CONNECTION FAILED");
            }    
            txbIp.Text = UiManager.appSettings.connection.scanner1.IpAddr.ToString();
            txbPort.Text = UiManager.appSettings.connection.scanner1.TcpPort.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            UiManager.appSettings.connection.scanner1.IpAddr = txbIp.Text;
            UiManager.appSettings.connection.scanner1.TcpPort = Convert.ToInt32(txbPort.Text);
            UiManager.SaveAppSettings();
        }

        private void BtSetting3_Click(object sender, RoutedEventArgs e)
        {

            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_MECHANICAL_BARCODE2);
        }

        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_MECHANICAL_BARCODE1);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_MECHANICAL_PLC);
        }
        private void UpdateLogs(string notify)
        {
            this.Dispatcher.Invoke(() => {
                this.txtLogs.Text += "\r\n" + notify;
                this.txtLogs.ScrollToEnd();
            });
        }
    }
}
