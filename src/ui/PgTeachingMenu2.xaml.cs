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
    /// Interaction logic for PgTeachingMenu2.xaml
    /// </summary>
    public partial class PgTeachingMenu2 : Page
    {
        MyLogger logger = new MyLogger("PG_TeachingMenu1");
        public PgTeachingMenu2()
        {
            InitializeComponent();

            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;

            this.Loaded += PgTeachingMenu2_Loaded;
            this.btSave.Click += BtSave_Click;
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            UiManager.appSettings.Jig.rowCount = Convert.ToInt32(tbxRow.Text);
            UiManager.appSettings.Jig.columnCount = Convert.ToInt32(tbxColum.Text);

            UiManager.SaveAppSettings();

            this.WriteRowColumnPLC();
            this.WritePitchPLC();

            MessageBox.Show("Save Complete");
        }

        private void PgTeachingMenu2_Loaded(object sender, RoutedEventArgs e)
        {
           this.tbxRow.Text = UiManager.appSettings.Jig.rowCount.ToString();
           this.tbxColum.Text = UiManager.appSettings.Jig.columnCount.ToString();

           
            this.ReadPitchRowColumnPLC();
            this.ReadRowColumnPLC();


        }
        private void WriteRowColumnPLC()
        {
            if(UiManager.PLC.IsConnected)
            {
                var Column = (UInt32)(UiManager.appSettings.Jig.columnCount);
                var Row = (UInt32)(UiManager.appSettings.Jig.rowCount);

            
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_X_MAXTRIX_POINT, Column);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_Y_MAXTRIX_POINT, Row);

            }
        }
        private void WritePitchPLC()
        {
            if (UiManager.PLC.IsConnected)
            {
                var PitchX = (UInt32)(Convert.ToUInt32(tbxXPitch.Text));
                var PitchY = (UInt32)(Convert.ToUInt32(tbxYPitch.Text));


                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_X_MAXTRIX_PITCH, PitchX);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_Y_MAXTRIX_PITCH, PitchY);

            }
        }
        private void ReadPitchRowColumnPLC()
        {
            if (UiManager.PLC.IsConnected)
            {
                var PitchX = GetPitchX();
                var Pitchy = GetPitchY();

                this.tbxXPitch.Text = PitchX.ToString();
                this.tbxYPitch.Text = Pitchy.ToString();
            }
        }
        private void ReadRowColumnPLC()
        {
            if (UiManager.PLC.IsConnected)
            {
                var PointX = GetCurrentX();
                var Pointy = GetCurrentY();

                this.tbxColum.Text = PointX.ToString();
                this.tbxRow.Text = Pointy.ToString();
            }
        }
        public double GetPitchX()
        {
            double ret = 0;
            Int16 SCALE = 100;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_X_MAXTRIX_PITCH) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetPitchY()
        {
            double ret = 0;
            Int16 SCALE = 100;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_Y_MAXTRIX_PITCH) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetCurrentX()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_X_MAXTRIX_POINT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetCurrentY()
        {
            double ret = 0;
            Int16 SCALE = 1;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_Y_MAXTRIX_POINT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_TEACHING_ID1);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_TEACHING_ID);
        }
    }
}
