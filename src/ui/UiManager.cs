using AutoLaserCuttingInput;
using ITM_Semiconductor;
using MvCamCtrl.NET;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace VisionInspection
{
    enum PAGE_ID
    {
        PAGE_MAIN_ID = 0,

        PAGE_MENU_ID,
        PAGE_MENU_TEACHING_ID,
        PAGE_MENU_TEACHING_ID1,
        PAGE_MENU_TEACHING_ID2,

        PAGE_SYSTEM_MENU,
        PAGE_SYSTEM_MENU_SYSTEM_MACHINE,


        PAGE_MENNU_SYSTEM_VISUAL,

        PAGE_MENU_MECHANICAL_PLC,
        PAGE_MENU_MECHANICAL_BARCODE1,
        PAGE_MENU_MECHANICAL_BARCODE2,
        PAGE_MENU_MECHANICAL_MES,

        PAGE_MENU_MANUAL_ID,
        PAGE_MENU_MANUALCH2_ID,
        PAGE_MENU_STATUS_LOG,
        PAGE_MENU_STATUS_SPC_OUTPUT,
        PAGE_MENU_STATUS_SPC_SEARCH,
        PAGE_MENU_SAVE_ID,
        PAGE_MENU_LOAD_ID,

        PAGE_SUPER_USER_MENU,
        PAGE_SUPER_USER_MENU_DELAY_MACHINE,
        PAGE_SUPER_USER_MENU_SETTING_ALARM,
        PAGE_SUPER_USER_MENU_SETTING_SERVO,



        PAGE_IO_ID,
        PAGE_LAST_JAM_ID,
        PAGE_CAMERA_SETTING,
        PAGE_RECIPE,
        PAGE_DEEPLEARNING
    };

    class UiManager
    {
        private const String APP_SETTINGS_FILE_NAME = "app_settings.json";
        private static MyLogger logger = new MyLogger("UiManager");

        public static AppSettings appSettings { get; set; }
        private static Hashtable pageTable = new Hashtable();
        private static WndMain wndMain;

        public static FENETProtocol PLC;

        public static ScannerTCP Scanner1;
        public static ScannerTCP Scanner2;


        private static Object LockerPLC = new object();

        public const String INPUT_IO_FILE_NAME = "input_machine.ini";
        public static String[] SectionNameInput;
        public static INIFile fileInput;

        public const String OUTPUT_IO_FILE_NAME = "output_machine.ini";
        public static String[] SectionNameOutput;
        public static INIFile fileOutput;

        //Camera
        public static HikCamDevice hikCamera = new HikCamDevice();
        public static HikCam Cam1 = new HikCam();
        public static HikCam Cam2 = new HikCam();

        public static MESProtocol MES;
        private static Object LockerMES = new object();

        public static void Startup() 
        {
            logger.Create("Startup:");
            try
            {

                // Load global settings:
                loadAppSettings(APP_SETTINGS_FILE_NAME);
                if (appSettings == null)
                {
                    appSettings = new AppSettings();
                }

                // Create Database if not existed
                Dba.createDatabaseIfNotExisted();

                // Create Main window:
                wndMain = new WndMain();

                // Create all pages and add to the local table:
                initPages();

                // Start Main window:
                wndMain.mainContent.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                wndMain.Show();

                //Connect Camera
                CameraListAcq();

                // Connect PLC
                ConnectionToPLC();

                // Connect Scanner 1
                ConnectionToScanner1();

                // Connect Scanner 2
                ConnectionToScanner2();

                //Connect Mes
                ConnectionToMES();

                // Load File I/O INI Of Machine
                loadFileINIIO_INPUT(INPUT_IO_FILE_NAME);
                loadFileINIIO_OUTPUT(OUTPUT_IO_FILE_NAME);

                // Load Alarm 
                AlarmList.LoadAlarm();
            }
            
            catch (Exception ex)
            {
                logger.Create("Startup error:" + ex.Message);
            }
        }

        #region CAMERA CONTROL

        public static void CameraListAcq()
        {
            hikCamera.DeviceListAcq();
            ConectCamera1();
            ConectCamera2();
        }
        public static void ConectCamera1()
        {
            MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList = hikCamera.m_pDeviceList;

            MyCamera.MV_CC_DEVICE_INFO device1 = UiManager.appSettings.connection.camera1.device;
            int ret1 = Cam1.Open(device1, HikCam.AquisMode.AcquisitionMode);
            Cam1.SetExposeTime((int)UiManager.appSettings.connection.camera1.ExposeTime);
            Thread.Sleep(2);
            if (ret1 == MyCamera.MV_OK)
            {
                //return true;
            }
            else
            {
                //return false;
            }

        }
        public static void ConectCamera2()
        {
            MyCamera.MV_CC_DEVICE_INFO device2 = UiManager.appSettings.connection.camera2.device;
            int ret2 = Cam2.Open(device2, HikCam.AquisMode.AcquisitionMode);
            Cam2.SetExposeTime((int)UiManager.appSettings.connection.camera2.ExposeTime);
            Thread.Sleep(2);
            if (ret2 == MyCamera.MV_OK)
            {
                //return true;
            }
            else
            {
                //return false;
            }


        }

        public static int Camera1Close()
        {
            int ret = MyCamera.MV_OK;
            if (Cam1 != null)
            {
                ret = Cam1.Close();
            }
            return ret;
        }
        public static int Camera2Close()
        {
            int ret = MyCamera.MV_OK;
            if (Cam1 != null)
            {
                ret = Cam2.Close();
            }
            return ret;
        }
        public static int Camera1Dispose()
        {
            int ret = MyCamera.MV_OK;
            if (Cam1 != null)
            {
                ret = Cam1.DisPose();
            }
            return ret;
        }
        public static int Camera2Dispose()
        {
            int ret = MyCamera.MV_OK;
            if (Cam1 != null)
            {
                ret = Cam2.DisPose();
            }
            return ret;
        }

        #endregion
        public static void SwitchPage(PAGE_ID pgId) {
            if (pageTable.ContainsKey(pgId)) {
                var pg = (System.Windows.Controls.Page)pageTable[pgId];
                wndMain.UpdateMainContent(pg);

                // Update Main status bar:
                //if (pgId == PAGE_ID.PAGE_MAIN_ID) {
                //    wndMain.btMain.Background = Brushes.DarkBlue;
                //} else {
                //    wndMain.btMain.ClearValue(Label.BackgroundProperty);
                //}
                //if (pgId == PAGE_ID.PAGE_MENU_ID ) {
                //    wndMain.btPlc.Background = Brushes.DarkBlue;
                //} else {
                //    wndMain.btPlc.ClearValue(Label.BackgroundProperty);
                //}
                //if (pgId == PAGE_ID.PAGE_IO_ID) {
                //    wndMain.btCamera.Background = Brushes.DarkBlue;
                //} else {
                //    wndMain.btCamera.ClearValue(Label.BackgroundProperty);
                //}
                //if (pgId == PAGE_ID.PAGE_MENU_STATUS_SPC_OUTPUT)
                //{
                //    wndMain.btScanner.Background = Brushes.DarkBlue;
                //}
                //else
                //{
                //    wndMain.btScanner.ClearValue(Label.BackgroundProperty);
                //}
                //if (pgId == PAGE_ID.PAGE_MENU_STATUS_LOG)
                //{
                //    wndMain.btMesPage.Background = Brushes.DarkBlue;
                //}
                //else
                //{
                //    wndMain.btMesPage.ClearValue(Label.BackgroundProperty);
                //}
            }
        }

        public static PgMain GetMainPage()
        {
            return (PgMain)pageTable[PAGE_ID.PAGE_MAIN_ID];
        }

        public static void SaveAppSettings()
        {
            try
            {
                String filePath = Path.Combine(Directory.GetCurrentDirectory(), APP_SETTINGS_FILE_NAME);
                var js = appSettings.TOJSON();
                File.WriteAllText(filePath, js);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("saveAppSettings error:" + ex.Message));
            }
        }

        public static void SaveCurrentModelSettings()
        {
            //ModelStore.UpdateModelSettings(currentModel);
        }



        public static Boolean IsRunningAuto()
        {
            var pg = (PgMain)pageTable[PAGE_ID.PAGE_MAIN_ID];
            if (pg != null)
            {
                return pg.IsRunningAuto();
            }
            return false;
        }

        private static void loadAppSettings(String fileName)
        {
            try
            {
                String filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileName);
                if (File.Exists(filePath))
                {
                    using (StreamReader file = File.OpenText(filePath))
                    {
                        appSettings = AppSettings.FromJSON(file.ReadToEnd());
                    }
                }
                else
                {
                    appSettings = new AppSettings();
                }
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("loadAppSettings error:" + ex.Message));
            }
        }


        private static void initPages() {
            pageTable.Add(PAGE_ID.PAGE_MAIN_ID, new PgMain());

            pageTable.Add(PAGE_ID.PAGE_MENU_ID, new PgPlc());

            pageTable.Add(PAGE_ID.PAGE_MENU_TEACHING_ID, new PgTeachingMenu());
            pageTable.Add(PAGE_ID.PAGE_MENU_TEACHING_ID1, new PgTeachingMenu2());



            pageTable.Add(PAGE_ID.PAGE_SYSTEM_MENU, new PgSystemMenu());
            pageTable.Add(PAGE_ID.PAGE_SYSTEM_MENU_SYSTEM_MACHINE, new PgSystemMenu01());


            pageTable.Add(PAGE_ID.PAGE_MENU_MECHANICAL_PLC, new PgMechanicalMenu());
            pageTable.Add(PAGE_ID.PAGE_MENU_MECHANICAL_BARCODE1, new PgMechanicalMenu1());
            pageTable.Add(PAGE_ID.PAGE_MENU_MECHANICAL_BARCODE2, new PgMechanicalMenu2());
            pageTable.Add(PAGE_ID.PAGE_MENU_MECHANICAL_MES, new PgMechanicalMenu3());


            pageTable.Add(PAGE_ID.PAGE_MENU_STATUS_LOG, new PgPlcStatusLog());
            pageTable.Add(PAGE_ID.PAGE_MENU_STATUS_SPC_OUTPUT, new PgMenuStatusSPCOutput());
            pageTable.Add(PAGE_ID.PAGE_MENU_STATUS_SPC_SEARCH, new PgPlcStatusSPCSearch());



            pageTable.Add(PAGE_ID.PAGE_SUPER_USER_MENU, new PgSuperUserMenu());
            pageTable.Add(PAGE_ID.PAGE_SUPER_USER_MENU_DELAY_MACHINE, new PgSuperUserMenu1());
            pageTable.Add(PAGE_ID.PAGE_SUPER_USER_MENU_SETTING_ALARM, new PgSuperUserMenu2());
            pageTable.Add(PAGE_ID.PAGE_SUPER_USER_MENU_SETTING_SERVO, new PgSuperUserMenu3());




            pageTable.Add(PAGE_ID.PAGE_IO_ID, new PgIO());


            pageTable.Add(PAGE_ID.PAGE_CAMERA_SETTING, new PgCamera());
            pageTable.Add(PAGE_ID.PAGE_RECIPE, new PgRecipe());
            pageTable.Add(PAGE_ID.PAGE_LAST_JAM_ID, new PgPlcStatusLog());
            pageTable.Add(PAGE_ID.PAGE_DEEPLEARNING, new PgDeepLearning());
        }
        #region Load File I/O
        private static void loadFileINIIO_INPUT(String fileName)
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IO Machine", "Input");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var filePath = System.IO.Path.Combine(folder, fileName);

                fileInput = new INIFile(filePath);
                if (!File.Exists(filePath))
                {
                    fileInput.Write("P000", "Name Of Adrress P000", "00");
                    SectionNameInput = fileInput.GetSectionNames();
                }
                SectionNameInput = fileInput.GetSectionNames();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Load File {0} Error:", fileName + ex.Message));
            }
        }
        private static void loadFileINIIO_OUTPUT(String fileName)
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IO Machine", "Output");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var filePath = System.IO.Path.Combine(folder, fileName);

                fileOutput = new INIFile(filePath);
                if (!File.Exists(filePath))
                {
                    fileOutput.Write("P000", "Name Of Adrress P000", "00");
                    SectionNameOutput = fileOutput.GetSectionNames();
                }
                SectionNameOutput = fileOutput.GetSectionNames();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Load File {0} Error:", fileName + ex.Message));
            }
        }
        #endregion
        #region Connect PLC
        public static void ConnectionToPLC()
        {
            try
            {
                PLC = null;
                InitialPLC();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Connection To PLC Error: " + ex.Message));
            }
        }
        private static void InitialPLC()
        {
            try
            {
                var settingPLC = new FENETProtocolSettings();
                settingPLC.PLCIp = appSettings.PLCTCP.PLCip;
                settingPLC.PLCPort = appSettings.PLCTCP.PLCport;
                settingPLC.SLOT_No = appSettings.PLCTCP.PLCSlot;
                PLC = new FENETProtocol(settingPLC, null, null);
                PLC.Start();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Initialze Com PLC Error: " + ex.Message));
            }
        }
        public static Boolean IsConnectionPLCFENETProtocol()
        {
            lock (LockerPLC)
            {
                if (PLC != null) { return PLC.IsConnected; }
                else { return false; }
            }
        }
        public static void StopConnectionPLC()
        {
            if (PLC != null)
            {
                PLC.Stop();
            }
        }
        #endregion
        #region Connect Scanner 1
        public static void ConnectionToScanner1()
        {
            try
            {
                Scanner1 = null;
                InitialScanner1();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Connection To PLC Error: " + ex.Message));
            }
        }
        private static void InitialScanner1()
        {
            Scanner1 = new ScannerTCP(UiManager.appSettings.connection.scanner1.IpAddr, UiManager.appSettings.connection.scanner1.TcpPort);
            try
            {
                Scanner1.Start();
            }
            catch (Exception ex)
            {
                logger.Create($"Connect Scanner1 Async: {ex.Message}");
            }
        }
        public static void StopConnectionScanner1()
        {
            if (Scanner1 != null)
            {
                Scanner1.Stop();
            }
        }
        #endregion
        #region Connect Scanner 2
        public static void ConnectionToScanner2()
        {
            try
            {
                Scanner2 = null;
                InitialScanner2();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Connection To PLC Error: " + ex.Message));
            }
        }
        private static void InitialScanner2()
        {
            Scanner2 = new ScannerTCP(UiManager.appSettings.connection.scanner2.IpAddr, UiManager.appSettings.connection.scanner2.TcpPort);
            try
            {
                Scanner2.Start();
            }
            catch (Exception ex)
            {
                logger.Create($"Connect Scanner1 Async: {ex.Message}");
            }
        }
        public static void StopConnectionScanner2()
        {
            if (Scanner2 != null)
            {
                Scanner2.Stop();
            }
        }
        #endregion
       
        #region Connect MES
        public static void ConnectionToMES()
        {
            try
            {
                MES = null;
                InitialMES();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Connection To MES Error: " + ex.Message));
            }
        }
        private static void InitialMES()
        {
            try
            {
                if (MES != null) return;
                string IP = appSettings.MesSettings1.localIp;
                int port = appSettings.MesSettings1.localPort;
                MES = new MESProtocol(IP, port);
                MES.Start();
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Initialze Com MES Error: " + ex.Message));
            }
        }
        public static Boolean IsConnectionMESProtocol()
        {
            lock (LockerMES)
            {
                if (MES != null) { return MES.IsConnected; }
                else { return false; }
            }
        }
        public static void StopConnectionMES()
        {
            if (MES != null) { MES.Stop(); }
        }
        #endregion
    } 
}
