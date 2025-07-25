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
    /// Interaction logic for PgTeachingMenu.xaml
    /// </summary>
    public partial class PgTeachingMenu : Page
    {

        MyLogger logger = new MyLogger("PG_TeachingMenu");
        bool isUpdate = false;

        int SelectButton = 0;
        private Button _previousButton;


        private Color Colo_ON;
        private Color Colo_OFF;
        private Color Colo_ON1;
        private Color Colo_OFF1;


        private List<bool> L_ListUpdateBitPLC_10000 = new List<bool>();
        private List<bool> L_ListUpdateBitPLC_12000 = new List<bool>();
        private List<bool> L_ListUpdateBitPLC_13000 = new List<bool>();
        private List<bool> X_ListUpdateBitPLC = new List<bool>();
        private List<bool> L_ListUpdateBitPLC_11000 = new List<bool>();

        private List<int> D_ListUpdateDevicePLC_2000 = new List<int>();
        private List<int> R_ListUpdateDevicePLC_2000 = new List<int>();
        private List<int> R_ListUpdateDevicePLC_12000 = new List<int>();


        int D_SetNumberPoint_Z;

        public PgTeachingMenu()
        {
            InitializeComponent();

            this.Loaded += PgTeachingMenu_Loaded;
            this.Unloaded += PgTeachingMenu_Unloaded;
            this.BtnSave.Click += BtnSave_Click;

            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;








            #region Button Ax1
            // BUTTON POS AX1
            this.btMovAx1Pos1.Click += BtMovAx1Pos1_Click;
            this.btMovAx1Pos2.Click += BtMovAx1Pos2_Click;
            this.btMovAx1Pos3.Click += BtMovAx1Pos3_Click;
            this.btMovAx1Pos4.Click += BtMovAx1Pos4_Click;
            this.btMovAx1Pos5.Click += BtMovAx1Pos5_Click;

            this.btLoadAx1Pos1.Click += BtLoadAx1Pos1_Click;
            this.btLoadAx1Pos2.Click += BtLoadAx1Pos2_Click;
            this.btLoadAx1Pos3.Click += BtLoadAx1Pos3_Click;
            this.btLoadAx1Pos4.Click += BtLoadAx1Pos4_Click;
            this.btLoadAx1Pos5.Click += BtLoadAx1Pos5_Click;

            //Tbx POS AX1
            this.TbxAx1Pos1.TouchDown += POS_TouchDown;
            this.TbxAx1Pos1.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx1Pos2.TouchDown += POS_TouchDown;
            this.TbxAx1Pos2.PreviewMouseDown += POS_PreviewMouseDown;



            this.TbxAx1Pos4.TouchDown += POS_TouchDown;
            this.TbxAx1Pos4.PreviewMouseDown += POS_PreviewMouseDown;






            this.TbxAx1SpeedPos1.TouchDown += POS_TouchDown;
            this.TbxAx1SpeedPos1.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx1SpeedPos2.TouchDown += POS_TouchDown;
            this.TbxAx1SpeedPos2.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx1SpeedPos3.TouchDown += POS_TouchDown;
            this.TbxAx1SpeedPos3.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx1SpeedPos4.TouchDown += POS_TouchDown;
            this.TbxAx1SpeedPos4.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx1SpeedPos5.TouchDown += POS_TouchDown;
            this.TbxAx1SpeedPos5.PreviewMouseDown += POS_PreviewMouseDown;







            // Button Ax1 TEACHING
            #region BUTTON AX1 TEACHING

            this.btAx1ServONOFF.Click += BtAx1ServONOFF_Click;
            this.btAx1BakeONOFF.Click += BtAx1BakeONOFF_Click;
            this.btAx1Home.Click += BtAx1Home_Click;

            this.btAx1JogUp.PreviewMouseDown += BtAx1JogUp_PreviewMouseDown;
            this.btAx1JogUp.PreviewMouseUp += BtAx1JogUp_PreviewMouseUp;
            this.btAx1JogUp.PreviewMouseMove += AX1_PreviewMouseMove;

            this.btAx1JogDown.PreviewMouseDown += BtAx1JogDown_PreviewMouseDown;
            this.btAx1JogDown.PreviewMouseUp += BtAx1JogDown_PreviewMouseUp;
            this.btAx1JogDown.PreviewMouseMove += AX1_PreviewMouseMove;


            this.TbxAx1JogSpeed.TouchDown += Tbx_TouchDown1;
            this.TbxAx1JogSpeed.PreviewMouseDown += Tbx_PreviewMouseDown1;


            #endregion

            #endregion

            #region BUTTON AX2

            this.btAx2ServONOFF.Click += BtAx2ServONOFF_Click;
            this.btAx2BakeONOFF.Click += BtAx2BakeONOFF_Click;
            this.btAx2Home.Click += BtAx2Home_Click;

            this.btAx2JogUp.PreviewMouseDown += BtAx2JogUp_PreviewMouseDown;
            this.btAx2JogUp.PreviewMouseUp += BtAx2JogUp_PreviewMouseUp;
            this.btAx2JogUp.PreviewMouseMove += Ax2_PreviewMouseMove;

            this.btAx2JogDown.PreviewMouseDown += BtAx2JogDown_PreviewMouseDown;
            this.btAx2JogDown.PreviewMouseUp += BtAx2JogDown_PreviewMouseUp;
            this.btAx2JogDown.PreviewMouseMove += Ax2_PreviewMouseMove;

            // this.TbxAx2JogSpeed.TextChanged += TextBox_TextChanged;

            this.TbxAx2JogSpeed.TouchDown += Tbx_TouchDown2;
            this.TbxAx2JogSpeed.PreviewMouseDown += Tbx_PreviewMouseDown2;


            //TBX POS AX2
            this.btMovAx2Pos1.Click += BtMovAx2Pos1_Click;
            this.btMovAx2Pos2.Click += BtMovAx2Pos2_Click;
            this.btMovAx2Pos3.Click += BtMovAx2Pos3_Click;
            this.btMovAx2Pos4.Click += BtMovAx2Pos4_Click;
            this.btMovAx2Pos5.Click += BtMovAx2Pos5_Click;

            this.btLoadAx2Pos1.Click += BtLoadAx2Pos1_Click;
            this.btLoadAx2Pos2.Click += BtLoadAx2Pos2_Click;
            this.btLoadAx2Pos3.Click += BtLoadAx2Pos3_Click;
            this.btLoadAx2Pos4.Click += BtLoadAx2Pos4_Click;
            this.btLoadAx2Pos5.Click += BtLoadAx2Pos5_Click;






            this.TbxAx2Pos1.TouchDown += POS_TouchDown;
            this.TbxAx2Pos1.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx2Pos2.TouchDown += POS_TouchDown;
            this.TbxAx2Pos2.PreviewMouseDown += POS_PreviewMouseDown;


            this.TbxAx2Pos4.TouchDown += POS_TouchDown;
            this.TbxAx2Pos4.PreviewMouseDown += POS_PreviewMouseDown;




            this.TbxAx2SpeedPos1.TouchDown += POS_TouchDown;
            this.TbxAx2SpeedPos1.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx2SpeedPos2.TouchDown += POS_TouchDown;
            this.TbxAx2SpeedPos2.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx2SpeedPos3.TouchDown += POS_TouchDown;
            this.TbxAx2SpeedPos3.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx2SpeedPos4.TouchDown += POS_TouchDown;
            this.TbxAx2SpeedPos4.PreviewMouseDown += POS_PreviewMouseDown;

            this.TbxAx2SpeedPos5.TouchDown += POS_TouchDown;
            this.TbxAx2SpeedPos5.PreviewMouseDown += POS_PreviewMouseDown;


            this.btMovePosVision.Click += BtMovePosVision_Click;
            this.btMovePosScanner.Click += BtMovePosScanner_Click;


            //LOADER BUTTON
            this.btJigClamp.Click += BtJigClamp_Click;
            this.btJigUnClamp.Click += BtJigUnClamp_Click;

            this.btVACUUM_1_ON.Click += BtVACUUM_1_ON_Click;
            this.btVACUUM_1_OFF.Click += BtVACUUM_1_OFF_Click;

            this.btLIGHT_VISION_1_ON.Click += BtLIGHT_VISION_1_ON_Click;
            this.btLIGHT_VISION_1_OFF.Click += BtLIGHT_VISION_1_OFF_Click;

            this.btLightMachineON.Click += BtLightMachineON_Click;
            this.btLightMachineOFF.Click += BtLightMachineOFF_Click;

            this.btVACUUM_2_ON.Click += BtVACUUM_2_ON_Click;
            this.btVACUUM_2_OFF.Click += BtVACUUM_2_OFF_Click;

            this.btLIGHT_VISION_2_ON.Click += BtLIGHT_VISION_2_ON_Click;
            this.btLIGHT_VISION_2_OFF.Click += BtLIGHT_VISION_2_OFF_Click;

            #endregion
        }



        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_TEACHING_ID1);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
            UiManager.SwitchPage(PAGE_ID.PAGE_MENU_TEACHING_ID);
        }
        private void BtMovePosScanner_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                int NumbarMatrix = Convert.ToInt32(LbPosMatrix.Content);
                UiManager.PLC.WriteDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_CALL_POS_QR, NumbarMatrix);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_SCANNER_POS_AX1, true);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_SCANNER_POS_AX2, true);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_SCANNER_POS_AX1, false);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_SCANNER_POS_AX2, false);
            }
        }

        private void BtMovePosVision_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                int NumbarMatrix = Convert.ToInt32(LbPosMatrix.Content);
                UiManager.PLC.WriteDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_CALL_POS_VISION, NumbarMatrix);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_VISION_POS_AX1, true);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_VISION_POS_AX2, true);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_VISION_POS_AX1, false);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_CALL_MATRIX_VISION_POS_AX2, false);
            }
        }
        private void BtLIGHT_VISION_2_OFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_2_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_2_OFF, false);
            }
        }

        private void BtLIGHT_VISION_2_ON_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_2_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_2_ON, false);
            }
        }

        private void BtVACUUM_2_OFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_2_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_2_OFF, false);
            }
        }
        private void BtVACUUM_2_ON_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_2_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_2_ON, false);
            }
        }

        private void BtLightMachineOFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_MACHINE_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_MACHINE_OFF, false);
            }
        }
        private void BtLightMachineON_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_MACHINE_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_MACHINE_ON, false);
            }
        }

        private void BtLIGHT_VISION_1_OFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_1_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_1_OFF, false);
            }
        }
        private void BtLIGHT_VISION_1_ON_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_1_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LIGHT_VISION_1_ON, false);
            }
        }


        private void BtVACUUM_1_OFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_1_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_1_OFF, false);
            }
        }

        private void BtVACUUM_1_ON_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_1_ON, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_VACUUM_1_ON, false);
            }
        }



        private void BtJigClamp_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_JIG_CLAMP, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_JIG_CLAMP, false);
            }
        }
        private void BtJigUnClamp_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_JIG_UNCLAMP, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_JIG_UNCLAMP, false);
            }
        }

        private void BtLoadAx2Pos5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 5")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX2, 4);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx2Pos5 {ex.Message}");
            }
        }

        private void BtLoadAx2Pos4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 4")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX2, 3);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx2Pos4 {ex.Message}");
            }
        }

        private void BtLoadAx2Pos3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 3")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX2, 2);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx2Pos3 {ex.Message}");
            }
        }

        private void BtLoadAx2Pos2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 2")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX2, 1);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx2Pos2 {ex.Message}");
            }
        }

        private void BtLoadAx2Pos1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 1")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX2, 0);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX2, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx2Pos1 {ex.Message}");
            }
        }



        private void BtMovAx2Pos5_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 5")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_RUN_QR_MATRIX_POS4, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_RUN_QR_MATRIX_POS4, false);
            }
        }

        private async void BtMovAx2Pos4_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 4")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y1_QR_MATRIX_POS3, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y1_QR_MATRIX_POS3, false);
            }
        }

        private void BtMovAx2Pos3_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 3")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_RUN_VISION_MATRIX_POS2, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_RUN_VISION_MATRIX_POS2, false);
            }
        }

        private void BtMovAx2Pos2_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 2")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y1_VISION_MATRIX_POS1, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y1_VISION_MATRIX_POS1, false);
            }
        }

        private void BtMovAx2Pos1_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 1")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_WAIT_POS_0, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_Y_WAIT_POS_0, false);
            }
        }

        private void POS_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    POS_TextChanged(textbox, new RoutedEventArgs());

            //}
        }
        private void POS_TouchDown(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    POS_TextChanged(textbox, new RoutedEventArgs());
            //}
        }
        private void POS_TextChanged(object sender, RoutedEventArgs e)
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
                //textBox.Background = Brushes.Red;
                //textBox.Text = "0.00";
                //textBox.Background = Brushes.Black;
            }



        }
        #region BUTTON AX2 TEACHING
        private void BtAx2JogDown_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_DOWN, false);
        }
        private void BtAx2JogDown_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_DOWN, true);
        }
        private void Ax2_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                Point position = e.GetPosition(btAx2JogUp);
                if (position.X < 0 || position.Y < 0 || position.X > btAx2JogUp.ActualWidth || position.Y > btAx2JogUp.ActualHeight)
                {
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_UP, false);
                }

                Point position1 = e.GetPosition(btAx2JogDown);
                if (position1.X < 0 || position1.Y < 0 || position1.X > btAx2JogDown.ActualWidth || position1.Y > btAx2JogDown.ActualHeight)
                {
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_DOWN, false);
                }
            }

        }
        private void BtAx2JogUp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_UP, false);
        }
        private void BtAx2JogUp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_JOG_UP, true);
        }
        private void BtAx2Home_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm comfirmYesNo = new WndConfirm();
            if (!comfirmYesNo.DoComfirmYesNo("You Want To Go Home AX2?")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_HOME, true);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_HOME, false);
            }
        }
        private void BtAx2BakeONOFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_BAKE_ON_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_BAKE_ON_OFF, false);
            }
        }
        private void BtAx2ServONOFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_ON_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX2_ON_OFF, false);
            }
        }

        #endregion

        #region BUTTON AX1 TEACHING

        private void BtAx1JogDown_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_DOWN, false);

        }
        private void BtAx1JogDown_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_DOWN, true);
        }
        private void BtAx1JogUp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_UP, false);
        }
        private void BtAx1JogUp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_UP, true);

        }
        private void AX1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                Point position = e.GetPosition(btAx1JogUp);
                if (position.X < 0 || position.Y < 0 || position.X > btAx1JogUp.ActualWidth || position.Y > btAx1JogUp.ActualHeight)
                {
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_UP, false);
                }

                Point position1 = e.GetPosition(btAx1JogDown);
                if (position1.X < 0 || position1.Y < 0 || position1.X > btAx1JogDown.ActualWidth || position1.Y > btAx1JogDown.ActualHeight)
                {
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_JOG_DOWN, false);
                }
            }
        }
        private void BtAx1Home_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm comfirmYesNo = new WndConfirm();
            if (!comfirmYesNo.DoComfirmYesNo("You Want To Go Home AX1?")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_HOME, true);
                Thread.Sleep(20);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_HOME, false);
            }

        }
        private void BtAx1BakeONOFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_BAKE_ON_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_BAKE_ON_OFF, false);
            }
        }
        private void BtAx1ServONOFF_Click(object sender, RoutedEventArgs e)
        {
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_ON_OFF, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.Servo_AX1_ON_OFF, false);
            };
        }


        private void BtLoadAx1Pos5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 5")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX1, 4);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx1Pos5 {ex.Message}");
            }
        }
        private void BtLoadAx1Pos4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 4")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX1, 3);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx1Pos4 {ex.Message}");
            }
        }
        private void BtLoadAx1Pos3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 3")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX1, 2);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {
                logger.Create($"btLoadAx1Pos3 {ex.Message}");
            }
        }
        private void BtLoadAx1Pos2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 2")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX1, 1);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx1Pos2 {ex.Message}");
            }
        }
        private void BtLoadAx1Pos1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm confirmYesNo = new WndConfirm();
                if (!confirmYesNo.DoComfirmYesNo("Do you want get data point 1")) return;
                if (UiManager.PLC.IsConnected)
                {
                    UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.ZR_POS_AX1, 0);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, true);
                    Thread.Sleep(10);
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.L_SAVE_POS_AX1, false);
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {

                logger.Create($"btLoadAx1Pos1 {ex.Message}");
            }
        }
        private void BtMovAx1Pos5_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 5")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_RUN_QR_MATRIX_POS4, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_RUN_QR_MATRIX_POS4, false);
            }
        }
        private void BtMovAx1Pos4_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 4")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_X1_QR_MATRIX_POS3, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_X1_QR_MATRIX_POS3, false);
            }
        }
        private void BtMovAx1Pos3_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 3")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_RUN_VISION_MATRIX_POS2, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_RUN_VISION_MATRIX_POS2, false);
            }
        }
        private void BtMovAx1Pos2_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 2")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_X1_VISION_MATRIX_POS1, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_X1_VISION_MATRIX_POS1, false);
            }
        }
        private void BtMovAx1Pos1_Click(object sender, RoutedEventArgs e)
        {
            WndConfirm confirmYesNo = new WndConfirm();
            if (!confirmYesNo.DoComfirmYesNo("Do you want to move to point 1")) return;
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_WAIT_POS_0, true);
                Thread.Sleep(10);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_WAIT_POS_0, false);
            }
        }

        #endregion

        #region TEACHING AX1 AX2
        private void Tbx_PreviewMouseDown1(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    // Nhập keypad xong quay lại check điều kiện
            //    TextBox_TextChanged1(textbox, new RoutedEventArgs());
            //}

        }
        private void Tbx_PreviewMouseDown2(object sender, MouseButtonEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;
            //    // Nhập keypad xong quay lại check điều kiện
            //    TextBox_TextChanged2(textbox, new RoutedEventArgs());
            //}

        }
        private void Tbx_TouchDown1(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;

            //    TextBox_TextChanged1(textbox, new RoutedEventArgs());
            //}

        }
        private void Tbx_TouchDown2(object sender, TouchEventArgs e)
        {
            //TextBox textbox = sender as TextBox;
            //Keypad keyboardWindow = new Keypad();
            //if (keyboardWindow.ShowDialog() == true)
            //{
            //    textbox.Text = keyboardWindow.Result;

            //    TextBox_TextChanged2(textbox, new RoutedEventArgs());
            //}

        }
        private void TextBox_TextChanged1(object sender, RoutedEventArgs e)
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
                //textBox.Background = Brushes.Red;
                //textBox.Text = "0.00";
                //textBox.Background = Brushes.Black;
            }

            this.WriteDeviceSpeedJOG1();

        }
        private void TextBox_TextChanged2(object sender, RoutedEventArgs e)
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
                //textBox.Background = Brushes.Red;
                //textBox.Text = "0.00";
                //textBox.Background = Brushes.Black;
            }

            this.WriteDeviceSpeedJOG2();

        }
        private void WriteDeviceSpeedJOG1()
        {

            double inPutAx1JogSpeed = Convert.ToDouble(TbxAx1JogSpeed.Text);
            int Ax1JogSpeed = (int)(inPutAx1JogSpeed * 1000);


            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX1_SPEED_JOG, Ax1JogSpeed);
                logger.Create($"PC_Write_AX1_JOG_SPEED_D: {PlcStore.D_AX1_SPEED_JOG}  = {Ax1JogSpeed}");
                this.UpdateUiOneShot();
            }
            else
            {
                MessageBox.Show("Không gửi được dữ liệu xuống PLC");
            }
        }
        private void WriteDeviceSpeedJOG2()
        {

            double inPutAx2JogSpeed = Convert.ToDouble(TbxAx2JogSpeed.Text);
            int Ax2JogSpeed = (int)(inPutAx2JogSpeed * 1000);
            if (UiManager.PLC.IsConnected)
            {
                UiManager.PLC.WriteDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX2_SPEED_JOG, Ax2JogSpeed);
                logger.Create($"PC_Write_AX2_JOG_SPEED_D: {PlcStore.D_AX2_SPEED_JOG}  = {Ax2JogSpeed}");
                this.UpdateUiOneShot();
            }
            else
            {
                MessageBox.Show("Không gửi được dữ liệu xuống PLC");
            }
        }
        #endregion




        #region BUTTON PAGE


        #endregion
        private void PgTeachingMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            isUpdate = false;

        }
        private async void PgTeachingMenu_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                isUpdate = true;

                Thread ThreadReadPLC = new Thread(() => ReadPLC());
                ThreadReadPLC.IsBackground = true;
                ThreadReadPLC.Start();

                await Task.Delay(1);

                this.InitializeColors();
                this.UpdateUiOneShot();
                this.UpdateButton();
                this.WriteRowColumnPLC();

            }
            catch (Exception ex)
            {
                logger.Create($"Load PAGE Error :{ex}");
            }

        }
        private void WriteRowColumnPLC()
        {
            if (UiManager.PLC.IsConnected)
            {
                var Column = (UInt32)(UiManager.appSettings.Jig.columnCount);
                var Row = (UInt32)(UiManager.appSettings.Jig.rowCount);


                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_X_MAXTRIX_POINT, Column);
                UiManager.PLC.WriteDoubleWord_UInt32(FENETProtocolDeviceName.BYTE_DB, PlcStore.JIG_Y_MAXTRIX_POINT, Row);

            }
        }
        private void InitializeColors()
        {
            string hexColorOn = "#66FF66"; // Mã màu ON (XANH)
            string hexColorOff = "#EEEEEE"; // Mã màu OFF (TRẮNG)
            string hexColorOff1 = "#FF0033"; // Mã màu OFF (ĐỎ)

            Colo_ON = (Color)ColorConverter.ConvertFromString(hexColorOn);
            Colo_OFF = (Color)ColorConverter.ConvertFromString(hexColorOff);
            Colo_ON1 = (Color)ColorConverter.ConvertFromString(hexColorOn);
            Colo_OFF1 = (Color)ColorConverter.ConvertFromString(hexColorOff1);
        }

        #region UpdateUIButton
        public void UpdateButton()
        {

            int columns = UiManager.appSettings.Jig.columnCount;
            int rows = UiManager.appSettings.Jig.rowCount;
            int NumberButton = columns * rows;

            // Đặt số cột và hàng cho Grid
            gridJig.ColumnDefinitions.Clear();
            gridJig.RowDefinitions.Clear();

            for (int i = 0; i < columns; i++)
            {
                gridJig.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rows; i++)
            {
                gridJig.RowDefinitions.Add(new RowDefinition());
            }

            // Tạo các nút và thêm vào Grid
            for (int i = 0; i < NumberButton; i++)
            {
                Button button = new Button
                {
                    Content = $"{i + 1}",
                    Margin = new Thickness(3),
                    Tag = i + 1 // Lưu số tương ứng vào Tag
                };

                // Đăng ký sự kiện Click cho nút
                button.Click += Button_Click;

                // Tính toán vị trí cột và hàng cho từng nút
                int row = i / columns;
                int column = i % columns;

                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                gridJig.Children.Add(button);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                // Đổi màu nền cho nút hiện tại
                clickedButton.Background = Brushes.LightGreen;

                // Đặt lại màu nền của nút trước đó về màu mặc định
                if (_previousButton != null && _previousButton != clickedButton)
                {
                    _previousButton.Background = Brushes.White; // Màu nền mặc định
                }

                // Lưu nút hiện tại làm nút đã được nhấn trước đó
                _previousButton = clickedButton;

                // Lấy số tương ứng từ Tag và in ra
                SelectButton = (int)clickedButton.Tag;

                this.LbPosMatrix.Content = SelectButton.ToString();


            }
        }
        #endregion

        #region Button
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WndConfirm comfirmYesNo = new WndConfirm();
                if (!comfirmYesNo.DoComfirmYesNo("You Want Save Setting?")) return;

                // AX1 
                int Ax1_POS0 = (int)(Convert.ToDouble(TbxAx1Pos1.Text) * 100);
                int Ax1_POS1 = (int)(Convert.ToDouble(TbxAx1Pos2.Text) * 100);

                int Ax1_POS3 = (int)(Convert.ToDouble(TbxAx1Pos4.Text) * 100);


                int Ax1_SPEED0 = (int)(Convert.ToDouble(TbxAx1SpeedPos1.Text) * 100);
                int Ax1_SPEED1 = (int)(Convert.ToDouble(TbxAx1SpeedPos2.Text) * 100);
                int Ax1_SPEED2 = (int)(Convert.ToDouble(TbxAx1SpeedPos3.Text) * 100);
                int Ax1_SPEED3 = (int)(Convert.ToDouble(TbxAx1SpeedPos4.Text) * 100);
                int Ax1_SPEED4 = (int)(Convert.ToDouble(TbxAx1SpeedPos5.Text) * 100);


                //AX2
                int Ax2_POS0 = (int)(Convert.ToDouble(TbxAx2Pos1.Text) * 100);
                int Ax2_POS1 = (int)(Convert.ToDouble(TbxAx2Pos2.Text) * 100);

                int Ax2_POS3 = (int)(Convert.ToDouble(TbxAx2Pos4.Text) * 100);


                int Ax2_SPEED0 = (int)(Convert.ToDouble(TbxAx2SpeedPos1.Text) * 100);
                int Ax2_SPEED1 = (int)(Convert.ToDouble(TbxAx2SpeedPos2.Text) * 100);
                int Ax2_SPEED2 = (int)(Convert.ToDouble(TbxAx2SpeedPos3.Text) * 100);
                int Ax2_SPEED3 = (int)(Convert.ToDouble(TbxAx2SpeedPos4.Text) * 100);
                int Ax2_SPEED4 = (int)(Convert.ToDouble(TbxAx2SpeedPos5.Text) * 100);


                if (UiManager.PLC.IsConnected)
                {

                    // AX1 
                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_WAIT_POS0, Ax1_POS0);
                    //logger.Create($"PC_Write_AX1_POS_WAIT: {PLCStore.R_AX1_WAIT_POS0}  = {Ax1_POS0}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_INPUT_MGZ_HI_POS1, Ax1_POS1);
                    //logger.Create($"PC_Write_AX1_POS2_INPUT_MGZ_HI: {PLCStore.R_AX1_INPUT_MGZ_HI_POS1}  = {Ax1_POS1}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_INPUT_MGZ_LO_POS2, Ax1_POS2);
                    //logger.Create($"PC_Write_AX1_POS3_INPUT_MGZ_LO: {PLCStore.R_AX1_INPUT_MGZ_LO_POS2}  = {Ax1_POS2}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_OUTPUT_MGZ_HIGH_POS3, Ax1_POS3);
                    //logger.Create($"PC_Write_AX1_POS4_OUTPUT_MGZ_HI: {PLCStore.R_AX1_OUTPUT_MGZ_HIGH_POS3}  = {Ax1_POS3}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_OUTPUT_MGZ_LOW_POS4, Ax1_POS4);
                    //logger.Create($"PC_Write_AX1_POS5_OUT_PUT_MGZ_LO: {PLCStore.R_AX1_OUTPUT_MGZ_LOW_POS4}  = {Ax1_POS4}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_FIX_MGZ_POS5, Ax1_POS5);
                    //logger.Create($"PC_Write_AX1_POS6_FIX_MGZ: {PLCStore.R_AX1_FIX_MGZ_POS5}  = {Ax1_POS5}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_LOADER_POS6, Ax1_POS6);
                    //logger.Create($"PC_Write_AX1_POS7_LOADER: {PLCStore.R_AX1_LOADER_POS6}  = {Ax1_POS6}", LogLevel.Information);



                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_WAIT_POS0_SPEED, Ax1_SPEED0);
                    //logger.Create($"PC_Write_AX1_SPEED1_POS_WAIT: {PLCStore.R_AX1_WAIT_POS0_SPEED}  = {Ax1_SPEED0}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_INPUT_MGZ_HI_POS1_SPEED, Ax1_SPEED1);
                    //logger.Create($"PC_Write_AX1_SPEED2_POS_INPUT_MGZ_HI: {PLCStore.R_AX1_INPUT_MGZ_HI_POS1_SPEED}  = {Ax1_SPEED1}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_INPUT_MGZ_LO_POS2_SPEED, Ax1_SPEED2);
                    //logger.Create($"PC_Write_AX1_SPEED3_POS_INPUT_MGZ_LO: {PLCStore.R_AX1_INPUT_MGZ_LO_POS2_SPEED}  = {Ax1_SPEED2}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_OUTPUT_MGZ_HIGH_POS3_SPEED, Ax1_SPEED3);
                    //logger.Create($"PC_Write_AX1_SPEED4_POS_OUTPUT_MGZ_HI: {PLCStore.R_AX1_OUTPUT_MGZ_HIGH_POS3_SPEED}  = {Ax1_SPEED3}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_OUTPUT_MGZ_LOW_POS4_SPEED, Ax1_SPEED4);
                    //logger.Create($"PC_Write_AX1_SPEED5_POS_OUTPUT_MGZ_LO: {PLCStore.R_AX1_OUTPUT_MGZ_LOW_POS4_SPEED}  = {Ax1_SPEED4}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_FIX_MGZ_POS5_SPEED, Ax1_SPEED5);
                    //logger.Create($"PC_Write_AX1_SPEED6_POS_FIX_MGZ: {PLCStore.R_AX1_FIX_MGZ_POS5_SPEED}  = {Ax1_SPEED5}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX1_LOADER_POS6_SPEED, Ax1_SPEED6);
                    //logger.Create($"PC_Write_AX1_SPEED7_POS_LOADER: {PLCStore.R_AX1_LOADER_POS6_SPEED}  = {Ax1_SPEED6}", LogLevel.Information);



                    ////AX2
                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_WAIT_POS0, Ax2_POS0);
                    //logger.Create($"PC_Write_AX2_POS1_WAIT: {PLCStore.R_AX2_WAIT_POS0}  = {Ax2_POS0}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_INPUT_MGZ_UP_POS1, Ax2_POS1);
                    //logger.Create($"PC_Write_AX2_POS2_INPUT_MGZ_UP: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS1}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_INPUT_MGZ_DN_POS2, Ax2_POS2);
                    //logger.Create($"PC_Write_AX2_POS3_INPUT_MGZ_DN: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS2}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_OUTPUT_MGZ_UP_POS3, Ax2_POS3);
                    //logger.Create($"PC_Write_AX2_POS4_OUTPUT_MGZ_UP: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS3}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_OUTPUT_MGZ_DN_POS4, Ax2_POS4);
                    //logger.Create($"PC_Write_AX2_POS5_OUTPUT_MGZ_DN: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS4}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_FIX_MGZ_POS5, Ax2_POS5);
                    //logger.Create($"PC_Write_AX2_POS6_FIX_MGZ: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS5}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_P0_LOADER_MATRIX_POS6, Ax2_POS6);
                    //logger.Create($"PC_Write_AX2_POS7_LOADER_MATRIX: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1}  = {Ax2_POS6}", LogLevel.Information);


                    ////------------
                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_WAIT_POS0_SPEED, Ax2_SPEED0);
                    //logger.Create($"PC_Write_AX2_SPEED1_POS_WAIT: {PLCStore.R_AX2_WAIT_POS0_SPEED}  = {Ax2_SPEED0}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_INPUT_MGZ_UP_POS1_SPEED, Ax2_SPEED1);
                    //logger.Create($"PC_Write_AX2_SPEED2_POS_INPUT_MGZ_UP: {PLCStore.R_AX2_INPUT_MGZ_UP_POS1_SPEED}  = {Ax2_SPEED1}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_INPUT_MGZ_DN_POS2_SPEED, Ax2_SPEED2);
                    //logger.Create($"PC_Write_AX2_SPEED3_POS_INPUT_MGZ_DN: {PLCStore.R_AX2_INPUT_MGZ_DN_POS2_SPEED}  = {Ax2_SPEED2}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_OUTPUT_MGZ_UP_POS3_SPEED, Ax2_SPEED3);
                    //logger.Create($"PC_Write_AX2_SPEED4_POS_OUTPUT_MGZ_UP: {PLCStore.R_AX2_OUTPUT_MGZ_UP_POS3_SPEED}  = {Ax2_SPEED3}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_OUTPUT_MGZ_DN_POS4_SPEED, Ax2_SPEED4);
                    //logger.Create($"PC_Write_AX2_SPEED5_POS_MGZ_DN: {PLCStore.R_AX2_OUTPUT_MGZ_DN_POS4_SPEED}  = {Ax2_SPEED4}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_FIX_MGZ_POS5_SPEED, Ax2_SPEED5);
                    //logger.Create($"PC_Write_AX2_SPEED6_POS_FIX_MGZ: {PLCStore.R_AX2_FIX_MGZ_POS5_SPEED}  = {Ax2_SPEED5}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_P0_LOADER_MATRIX_POS6_SPEED, Ax2_SPEED6);
                    //logger.Create($"PC_Write_AX2_SPEED7_POS_LOADER_MATRIX: {PLCStore.R_AX2_P0_LOADER_MATRIX_POS6_SPEED}  = {Ax2_SPEED6}", LogLevel.Information);

                    //UiManager.PLC.WriteDoubleWord(DeviceCode.R, PLCStore.R_AX2_LOADER_MATRIX_POS7, Ax2_SPEED7);
                    //logger.Create($"PC_Write_AX2_SPEED8_POS_LOADER_MATRIX: {PLCStore.R_AX2_LOADER_MATRIX_POS7}  = {Ax2_SPEED6}", LogLevel.Information);

                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow("Messenger : Save Data Successfully ");

                }
                else
                {
                    WndMessenger ShowMessenger = new WndMessenger();
                    ShowMessenger.MessengerShow("Messenger : Save Data Was NOT Successful ");
                }
                this.UpdateUiOneShot();
            }
            catch (Exception ex)
            {
                logger.Create($"BtnSave_Click: {ex.Message}");
            }
        }

        #endregion

        #region DEVICE





        #endregion

        #region UPDATE UI MACHINE

        private void UpdateUiOneShot()
        {
            if (UiManager.PLC.IsConnected)
            {
                var Ax1SpeedCurrent = GetJogSpeedAx1();
                var Ax2SpeedCurrent = GetJogSpeedAx2();


                this.Dispatcher.Invoke(() =>
                {
                    this.TbxAx1JogSpeed.Text = this.FormatNumber(Ax1SpeedCurrent);
                    this.TbxAx2JogSpeed.Text = this.FormatNumber(Ax2SpeedCurrent);

                });
            }
        }
        private async void ReadPLC()
        {
            try
            {
                while (isUpdate)
                {
                    if (UiManager.PLC.IsConnected)
                    {
                        var Ax1LimitDown = GetLimitAx1Down();
                        var Ax1LimitUp = GetLimitAx1Up();
                        var Ax1Home = GetAx1Home();
                        var Ax1DriverError = GetAx1DriverError();

                        var Ax2LimitDown = GetLimitAx2Down();
                        var Ax2LimitUp = GetLimitAx2Up();
                        var Ax2Home = GetAx2Home();
                        var Ax2DriverError = GetAx2DriverError();

                        var Ax1PositionCurrent = GetCurrentPointAx1();
                        var Ax1ErrorCode = GetErrorCodeAx1();

                        var Ax2PositionCurrent = GetCurrentPointAx2();
                        var Ax2ErrorCode = GetErrorCodeAx2();

                        var Ax1ServoOnOff = GetAx1ServoONOFF();
                        var Ax1JogUp = GetAx1JogUp();
                        var Ax1JogDown = GetAx1JogDown();
                        var Ax1Org = GetAx1ORG();
                        var Ax1BakeOnOff = GetAx1BakeOnOff();

                        var Ax2ServoOnOff = GetAx2ServoONOFF();
                        var Ax2JogUp = GetAx2JogUp();
                        var Ax2JogDown = GetAx2JogDown();
                        var Ax2Org = GetAx2ORG();
                        var Ax2BakeOnOff = GetAx2BakeOnOff();

                        var JigClamp = GetJigClamp();
                        var JigUnclamp = GetJigUnClamp();

                        var LightMachineOn = GetLightMachineON();
                        var LightMachineOff = GetLightMachineOFF();

                        var Vacuum1On = GetVacuum1On();
                        var Vacuum1Off = GetVacuum1Off();

                        var Vacuum2On = GetVacuum2On();
                        var Vacuum2Off = GetVacuum2Off();

                        var LightVision1ON = GetLightVision1On();
                        var LightVision1Off = GetLightVision1Off();

                        var LightVision2ON = GetLightVision2On();
                        var LightVision2Off = GetLightVision2Off();

                        var CallMatrixVision = GetCallMaxtrixVision();
                        var CallMatrixScanner = GetCallMaxtrixScanner();

                        var X_Wait_Pos0 = GetXPOS0();
                        var X_vision_Pos1 = GetX1VisionPos1();
                        var X_Run_Vision_Pos2 = GetXRunVisionPos2();
                        var X_QR_Pos3 = GetX1QRPos3();
                        var X_Run_QR_Pos4 = GetXRunQRPos4();

                        var Y_Wait_Pos0 = GetYPOS0();
                        var Y_vision_Pos1 = GetY1VisionPos1();
                        var Y_Run_Vision_Pos2 = GetYRunVisionPos2();
                        var Y_QR_Pos3 = GetY1QRPos3();
                        var Y_Run_QR_Pos4 = GetYRunQRPos4();


                        this.Dispatcher.Invoke(() =>
                        {
                            this.RtAx1LimitDown.Fill = new SolidColorBrush(Ax1LimitDown ? Colo_ON : Colo_OFF1);
                            this.RtAx1LimitUp.Fill = new SolidColorBrush(Ax1LimitUp ? Colo_ON : Colo_OFF1);
                            this.RtAx1ORG.Fill = new SolidColorBrush(Ax1Home ? Colo_ON : Colo_OFF1);
                            this.RtAx1Error.Fill = new SolidColorBrush(Ax1DriverError ? Colo_ON : Colo_OFF1);

                            this.RtAx2LimitDown.Fill = new SolidColorBrush(Ax2LimitDown ? Colo_ON : Colo_OFF1);
                            this.RtAx2LimitUp.Fill = new SolidColorBrush(Ax2LimitUp ? Colo_ON : Colo_OFF1);
                            this.RtAx2ORG.Fill = new SolidColorBrush(Ax2Home ? Colo_ON : Colo_OFF1);
                            this.RtAx2Error.Fill = new SolidColorBrush(Ax2DriverError ? Colo_ON : Colo_OFF1);

                            this.lbAx1PosCurrent.Content = this.FormatNumber(Ax1PositionCurrent);
                            this.lbAx1ErrorCode.Content = this.FormatNumber(Ax1ErrorCode);

                            this.lbAx2PosCurrent.Content = this.FormatNumber(Ax2PositionCurrent);
                            this.lbAx2ErrorCode.Content = this.FormatNumber(Ax2ErrorCode);

                            this.btAx1ServONOFF.Background = new SolidColorBrush(Ax1ServoOnOff ? Colo_ON : Colo_OFF1);
                            this.lbAx1ServoONOFF.Content = Ax1ServoOnOff ? "Servo ON" : "Servo OFF";
                            this.btAx1JogUp.Background = new SolidColorBrush(Ax1JogUp ? Colo_ON : Colo_OFF);
                            this.btAx1JogDown.Background = new SolidColorBrush(Ax1JogDown ? Colo_ON : Colo_OFF);
                            this.btAx1Home.Background = new SolidColorBrush(Ax1Org ? Colo_ON : Colo_OFF);
                            this.btAx1BakeONOFF.Background = new SolidColorBrush(Ax1BakeOnOff ? Colo_OFF1 : Colo_ON);
                            this.lbAx1BakeONOFF.Content = Ax1BakeOnOff ? "Bake OFF" : "Bake ON";

                            this.btAx2ServONOFF.Background = new SolidColorBrush(Ax2ServoOnOff ? Colo_ON : Colo_OFF1);
                            this.lbAx2ServoONOFF.Content = Ax2ServoOnOff ? "Servo ON" : "Servo OFF";
                            this.btAx2JogUp.Background = new SolidColorBrush(Ax2JogUp ? Colo_ON : Colo_OFF);
                            this.btAx2JogDown.Background = new SolidColorBrush(Ax2JogDown ? Colo_ON : Colo_OFF);
                            this.btAx2Home.Background = new SolidColorBrush(Ax2Org ? Colo_ON : Colo_OFF);
                            this.btAx2BakeONOFF.Background = new SolidColorBrush(Ax2BakeOnOff ? Colo_OFF1 : Colo_ON);
                            this.lbAx2BakeONOFF.Content = Ax2BakeOnOff ? "Bake OFF" : "Bake ON";

                            this.btVACUUM_1_ON.Background = new SolidColorBrush(Vacuum1On ? Colo_ON : Colo_OFF);
                            this.btVACUUM_1_OFF.Background = new SolidColorBrush(Vacuum1Off ? Colo_ON : Colo_OFF);

                            this.btVACUUM_2_ON.Background = new SolidColorBrush(Vacuum2On ? Colo_ON : Colo_OFF);
                            this.btVACUUM_2_OFF.Background = new SolidColorBrush(Vacuum2Off ? Colo_ON : Colo_OFF);


                            this.btLightMachineON.Background = new SolidColorBrush(LightMachineOn ? Colo_ON : Colo_OFF);
                            this.btLightMachineOFF.Background = new SolidColorBrush(LightMachineOff ? Colo_ON : Colo_OFF);

                            this.btJigClamp.Background = new SolidColorBrush(JigClamp ? Colo_ON : Colo_OFF);
                            this.btJigUnClamp.Background = new SolidColorBrush(JigUnclamp ? Colo_ON : Colo_OFF);

                            this.btLIGHT_VISION_1_ON.Background = new SolidColorBrush(LightVision1ON ? Colo_ON : Colo_OFF);
                            this.btLIGHT_VISION_1_OFF.Background = new SolidColorBrush(LightVision1Off ? Colo_ON : Colo_OFF);

                            this.btLIGHT_VISION_2_ON.Background = new SolidColorBrush(LightVision2ON ? Colo_ON : Colo_OFF);
                            this.btLIGHT_VISION_2_OFF.Background = new SolidColorBrush(LightVision2Off ? Colo_ON : Colo_OFF);

                            this.btMovePosVision.Background = new SolidColorBrush(CallMatrixVision ? Colo_ON : Colo_OFF);
                            this.btMovePosScanner.Background = new SolidColorBrush(CallMatrixScanner ? Colo_ON : Colo_OFF);

                            this.btMovAx1Pos1.Background = new SolidColorBrush(X_Wait_Pos0 ? Colo_ON : Colo_OFF);
                            this.btMovAx1Pos2.Background = new SolidColorBrush(X_vision_Pos1 ? Colo_ON : Colo_OFF);
                            this.btMovAx1Pos3.Background = new SolidColorBrush(X_Run_Vision_Pos2 ? Colo_ON : Colo_OFF);
                            this.btMovAx1Pos4.Background = new SolidColorBrush(X_QR_Pos3 ? Colo_ON : Colo_OFF);
                            this.btMovAx1Pos5.Background = new SolidColorBrush(X_Run_QR_Pos4 ? Colo_ON : Colo_OFF);

                            this.btMovAx2Pos1.Background = new SolidColorBrush(Y_Wait_Pos0 ? Colo_ON : Colo_OFF);
                            this.btMovAx2Pos2.Background = new SolidColorBrush(Y_vision_Pos1 ? Colo_ON : Colo_OFF);
                            this.btMovAx2Pos3.Background = new SolidColorBrush(Y_Run_Vision_Pos2 ? Colo_ON : Colo_OFF);
                            this.btMovAx2Pos4.Background = new SolidColorBrush(Y_QR_Pos3 ? Colo_ON : Colo_OFF);
                            this.btMovAx2Pos5.Background = new SolidColorBrush(Y_Run_QR_Pos4 ? Colo_ON : Colo_OFF);

                        });

                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {

                logger.Create($"Thread Read PLC :{ex}");
            }
        }

        public string FormatNumber(double number)
        {
            bool isNegative = number < 0;
            double dividedNumber = number / 1.0;
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
        #endregion

        #region BIT LAMP PLC
        public bool GetLimitAx1Down()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_LIMIT_DOWN);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetLimitAx1Up()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_LIMIT_UP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1Home()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_HOME);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1DriverError()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_DRIVER_ERROR);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetLimitAx2Down()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_LIMIT_DOWN);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetLimitAx2Up()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_LIMIT_UP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2Home()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_HOME);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2DriverError()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_DRIVER_ERROR);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public double GetCurrentPointAx1()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX1_POSITION_CURRENT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetJogSpeedAx1()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX1_SPEED_JOG) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetErrorCodeAx1()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.K_LAMP_AX1_DRIVER_ERROR) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }

        public double GetCurrentPointAx2()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX2_POSITION_CURRENT) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetJogSpeedAx2()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_AX2_SPEED_JOG) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double GetErrorCodeAx2()
        {
            double ret = 0;
            Int16 SCALE = 1000;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.K_LAMP_AX2_DRIVER_ERROR) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }

        public bool GetAx1ServoONOFF()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_SERVO_ON_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1JogUp()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_JOG_UP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1JogDown()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_JOG_DOWN);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1ORG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_ORG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx1BakeOnOff()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX1_BAKE_ON_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetAx2ServoONOFF()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_SERVO_ON_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2JogUp()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_JOG_UP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2JogDown()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_JOG_DOWN);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2ORG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_ORG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetAx2BakeOnOff()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_AX2_BAKE_ON_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }



        public bool GetJigClamp()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_JIG_CLAMP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetJigUnClamp()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_JIG_UNCLAMP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetLightMachineON()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_MACHINE_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetLightMachineOFF()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_MACHINE_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetVacuum1On()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_VACUUM_1_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetVacuum1Off()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_VACUUM_1_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetVacuum2On()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_VACUUM_2_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetVacuum2Off()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_VACUUM_2_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetLightVision1On()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_VISION_1_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetLightVision1Off()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_VISION_1_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }

        public bool GetLightVision2On()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_VISION_2_ON);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetLightVision2Off()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_LIGHT_VISION_2_OFF);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetCallMaxtrixVision()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_CALL_MATRIX_VISION_POS);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetCallMaxtrixScanner()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_CALL_MATRIX_SCANNER_POS);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetXPOS0()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_WAIT_POS_0);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetX1VisionPos1()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_X1_VISION_MATRIX_POS1);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetXRunVisionPos2()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_RUN_VISION_MATRIX_POS2);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetX1QRPos3()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_X1_QR_MATRIX_POS3);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetXRunQRPos4()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_RUN_QR_MATRIX_POS4);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetYPOS0()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_Y_WAIT_POS_0);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetY1VisionPos1()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_Y1_VISION_MATRIX_POS1);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetYRunVisionPos2()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_Y_RUN_VISION_MATRIX_POS2);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetY1QRPos3()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_Y1_QR_MATRIX_POS3);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        public bool GetYRunQRPos4()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.K_LAMP_Y_RUN_QR_MATRIX_POS4);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Get Status Axis X Ready Error: " + ex.Message));
                return false;
            }
        }
        #endregion

    }
}
