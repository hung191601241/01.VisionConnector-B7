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
    /// Interaction logic for PgMechanicalMenu3.xaml
    /// </summary>
    public partial class PgMechanicalMenu3 : Page
    {
        public PgMechanicalMenu3()
        {
            InitializeComponent();
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.btSetting3.Click += BtSetting3_Click;
            this.btSetting4.Click += BtSetting4_Click;

            this.Loaded += PgMechanicalMenu3_Loaded;
            this.btSave.Click += BtSave_Click;
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm comfirmYesNo = new WndConfirm();
            if (!comfirmYesNo.DoComfirmYesNo("You Want Save Setting ?")) return;

            UiManager.appSettings.MesSettings1.localIp = String.Format("{0}.{1}.{2}.{3}",
                   txtLocalIp1.Text, txtLocalIp2.Text, txtLocalIp3.Text, txtLocalIp4.Text);
            UiManager.appSettings.MesSettings1.localPort = int.Parse(txtLocalPort.Text);
            UiManager.SaveAppSettings();
        }

        private void PgMechanicalMenu3_Loaded(object sender, RoutedEventArgs e)
        {
            var arr = UiManager.appSettings.MesSettings1.localIp.Split('.');
            if (arr.Length == 4)
            {
                this.txtLocalIp1.Text = arr[0];
                this.txtLocalIp2.Text = arr[1];
                this.txtLocalIp3.Text = arr[2];
                this.txtLocalIp4.Text = arr[3];
            }
            this.txtLocalPort.Text = UiManager.appSettings.MesSettings1.localPort.ToString();
        }

        private void BtSetting4_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_MECHANICAL_MES);
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
    }
}
