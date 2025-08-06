using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Data;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Imaging; 
using System.ComponentModel;
using MvCamCtrl.NET;
using System.Collections;
using ViDi2.UI;
using ViDi2;
using AutoLaserCuttingInput;
using ITM_Semiconductor;

// Alias để tránh xung đột
using DrawingColor = System.Drawing.Color;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using System.ServiceModel.Channels;
using OpenCvSharp.Cuda;

//using System.Diagnostics;

namespace VisionInspection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PgMain : Page
    {
        MyLogger logger = new MyLogger("PgMain");
        Object mLock = new object();
        private readonly object lockObj = new object();
        private object _captureLock = new object(); // Khóa đồng bộ

        //PLC

        private int[] PlcDeviceDoubleWord;
        private List<bool> PlcDeviceMultiBitM;

        //private Stopwatch stopWatch = new Stopwatch();
        private System.Timers.Timer clock;
        private AppSettings appSettings;
        private ConnectionSettings connection;

        //VIDI
        //List<IComputeDevice> deviceList;
        ViDi2.Runtime.IControl control;
        ViDi2.IWorkspace workspace;
        ViDi2.IStream stream1;
        ViDi2.IStream stream2;
        public static List<DLJob> dlJob = UiManager.appSettings.CurrentModel.dLJobs;

        //VisionPro
        //public List<Tuple<string, CogToolBlock>> cogTbJobList = new List<Tuple<string, CogToolBlock>>();
        //public static List<VsProJob> vsProJob = UiManager.appSettings.CurrentModel.vsProJobs;


        //Camera
        private HikCam myCam1 = new HikCam();
        VisionAL vision1 = new VisionAL(VisionAL.Chanel.Ch1);
        VisionAL vision2 = new VisionAL(VisionAL.Chanel.Ch2);
        private int End_Pr1 = 0;

        //Private 
        Boolean IsRunning = false;
        Boolean PLcIsConnect = false;
        Boolean Camera1IsConnect = false;
        Boolean Auto = true;
        Boolean Manual = false;
        Boolean Ready = false;

        // Cycle Time
        private bool cycleRunning = false;

        // Caculator Cycle Time
        private DateTime cycleStart;
        private System.Timers.Timer cycleTimer;
        #region New
        private bool isRunning = false;
        private bool isRunning1 = false;
        private bool isRunning2 = false;
        private bool isUpdate = false;
        private bool MesCheck = false;


        private bool Flag1 = false;
        private bool Flag2 = false;
        private bool Flag3 = false;
        private bool Flag4 = false;

        // Running thread:
        private Thread runThread3;
        private Thread runThread1;
        private Thread runThread2;

        // New LOT flag:
        private Boolean requireNewLot = false;

        private ManualResetEvent exitFlag = new ManualResetEvent(false);

        // AUTO test index: 0=JIG, 1=PKG[1], 2=PKG[2],..., N = PKG[N].
        private int testIndexCH1 = 0;
        private int testIndexCH2 = 0;
        private Boolean testAllPkgBad = false;

        //List QR 

        private ListData listData = new ListData();


        private bool READ_QR1_TRIG;
        private bool READ_QR2_TRIG;


        private bool READ_VISION1_TRIG;
        private bool READ_VISION2_TRIG;

        private bool READ_CLR_ALL;

        private bool READ_CHECK_MES;

        private bool READ_MACHINE_RUNNING;

        private bool READ_LAMP_RESET;

        private short READ_ALARM_01;
        private short READ_ALARM_02;
        private short READ_ALARM_03;
        private short READ_ALARM_04;
        private short READ_ALARM_05;
        private short READ_ALARM_06;
        private short READ_ALARM_07;
        private short READ_ALARM_08;
        private short READ_ALARM_09;
        private short READ_ALARM_10;
       
     

        //bool PreviewVISION1 = false;
        //bool PreviewQR1 = false;
        //bool PreviewCLR1 = false;

        //bool PreviewVISION = false;
        //bool PreviewQR = false;
        //bool PreviewCLR = false;


        private readonly object _lock = new object();

        private Color Colo_ON;
        private Color Colo_OFF;
        private Color Colo_ON1;
        private Color Colo_OFF1;

        private CancellationTokenSource cancellationTokenSource1;
        private CancellationTokenSource cancellationTokenSource2;

        private LotStatus lotStatus;
        private RunSettings runSettings;

        //Index Jig
        private int mulIdx1 = 0;
        private List<string> lstIdxJig1 = new List<string>();
        private int mulIdx2 = 0;
        private List<string> lstIdxJig2 = new List<string>();

        // Test Result
        private DataCheckPKG[] ResultRunning;

       

        private const int MES_CHECKING_TIMEOUT = 10000;

        // Define Color Display Result When Final
        private Brush PKG_QR_FINAL_OK_BACKGROUND = Brushes.Green;
        private Brush PKG_QR_FINAL_NG_BACKGROUND = Brushes.Red;
        private Brush PKG_QR_FINAL_EMPTY_BACKGROUND = Brushes.Yellow;
        #endregion

        // Cờ kiểm tra xem ClearError() đã chạy hay chưa
        private bool hasClearedError = false;
        public PgMain()
        {
            this.DataContext = this;

            InitializeComponent();

            InitializeErrorCodes();
            ActionClearAlarm.ClearErrorAction = ClearError;

            this.clock = new System.Timers.Timer(1000);
            this.clock.AutoReset = true;
            this.clock.Elapsed += this.Clock_Elapsed;
            //var control = new ViDi2.Runtime.Local.Control(ViDi2.GpuMode.Deferred);
            //control.InitializeComputeDevices(ViDi2.GpuMode.SingleDevicePerTool, new List<int>() { 0 });
            //this.control = control;
            //var computeDevices = control.ComputeDevices;
            //this.deviceList = computeDevices;
            InitControl();

            DataContext = this;
            sampleViewer.DragEnter += CheckDrop;
            sampleViewer.DragOver += CheckDrop;
            sampleViewer.Drop += DoDrop;
            sampleViewer.ToolSelected += sampleViewer_ToolSelected;
            sampleViewer1.DragEnter += CheckDrop1;
            sampleViewer1.DragOver += CheckDrop1;
            sampleViewer1.Drop += DoDrop1;
            sampleViewer1.ToolSelected += sampleViewer_ToolSelected1;


            //sampleViewer.RaiseEvent(new RoutedEventArgs(SampleViewer.DropEvent));

            #region Event Page
          
            this.Loaded += PgMain_Loaded;
            this.Unloaded += PgMain_Unloaded;

            this.btnLotIn.Click += BtnLotIn_Click;
           
            //this.btnAuto.Click += BtnAuto_Click;
            //this.btnManual.Click += BtnManual_Click;

            this.btnMainStart.Click += BtnMainStart_Click;
            this.btnMainStop.Click += BtnMainStop_Click;
            this.btnMainHome.Click += BtnMainHome_Click;
            this.btnMainReset.Click += BtnMainReset_Click;
            #endregion
        }

        private void InitControl()
        {
            if (this.control != null)
            {
                this.Control.Dispose();
                this.control.Dispose();
            }
            var control = new ViDi2.Runtime.Local.Control(ViDi2.GpuMode.Deferred);
            //control.InitializeComputeDevices(ViDi2.GpuMode.SingleDevicePerTool, new List<int>() { 0, 1 });
            control.InitializeComputeDevices(ViDi2.GpuMode.SingleDevicePerTool, new List<int>() { 0 });
            this.control = control;
        }
        private void updateLotDataUI()
        {
            var lotData = UiManager.appSettings.lotData;
            this.txtProductId.Text = lotData.deviceId;
            this.txtLotId.Text = lotData.lotId;
            this.lblLotqty.Content = lotData.lotQty;

            this.lblQrNGCount.Content = lotData.QRNG;
            this.lblQrOKCount.Content = lotData.QROK;
            this.lblLotCount.Content = lotData.lotCount;
            this.lblQrYield.Content = lotData.QrYield;

            UiManager.MES.Setup(UiManager.appSettings.connection.EquipmentName, lotData.lotId);
        }

        private void BtnLotIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                UserManager.createUserLog(UserActions.MAIN_BUTTON_LOTIN);

                var wnd = new WndLotInput(); ;
                var lotData = new LotInData();
                lotData.workGroup = UiManager.appSettings.lotData.workGroup;

                var newSettings = wnd.DoSettings(System.Windows.Window.GetWindow(this), lotData);
                if (newSettings != null)
                {
                    UiManager.appSettings.lotData = newSettings.Clone();
                    this.lotStatus = new LotStatus(newSettings.lotId);
                    this.lotStatus.InputCount = newSettings.lotQty;
                    UiManager.SaveAppSettings();
                    this.updateLotDataUI();
                    //UpdateResult();
                    DeleteResult();

                }
            }
            catch (Exception ex)
            {
                logger.Create("BtLotIn_Click error: " + ex.Message);
            }
        }

        #region Event Action
        //private void BtnManual_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsRunning)
        //        return;
        //    this.Manual = true;
        //    this.Auto = false;
        //    this.btnManual.Background = Brushes.Green;
        //    this.btnAuto.Background = Brushes.Transparent;
        //}

        //private void BtnAuto_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsRunning)
        //        return;
        //    this.Manual = false;
        //    this.Auto = true;
        //    this.btnManual.Background = Brushes.Transparent;
        //    this.btnAuto.Background = Brushes.Green;
        //}
        private void PgMain_Loaded(object sender, RoutedEventArgs e)
        {
            //InitControl();
            Init();

            #region New
            InitializeColors();

            GenerateGrids(UiManager.appSettings.Jig.rowCount, UiManager.appSettings.Jig.columnCount);
            ClearResultCH1();
            ClearResultCH2();

            CreateCellGril2(UiManager.appSettings.Jig.rowCount, UiManager.appSettings.Jig.columnCount);
            CreateCellGril2cCH2(UiManager.appSettings.Jig.rowCount, UiManager.appSettings.Jig.columnCount);
          
            CH1();
            CH2();
            isUpdate = true;
            CallReadPLC();
        
            WriteRowColumnPLC();
            UiManager.MES.Setup(UiManager.appSettings.connection.EquipmentName, UiManager.appSettings.lotData.lotId);
            clock.Start();
            UpdateUIMES();
            updateLotDataUI();
            #endregion
        }
        private void PgMain_Unloaded(object sender, RoutedEventArgs e)
        {

            clock.Stop();

            if (myCam1 != null)
            {
                myCam1.Close();
                myCam1.DisPose();
            }
            //clock.Stop();
            #region New
            isRunning1 = false;
            isRunning2 = false;
            isUpdate = false;
            cancellationTokenSource1?.Cancel();
            cancellationTokenSource2?.Cancel();
            UiManager.appSettings.currentModel = this.txtCurrentModel.Text.ToString();
            #endregion
            SaveQTY();
            UiManager.SaveAppSettings();
        }
        private void SaveQTY()
        {
            var lotData = UiManager.appSettings.lotData;
            lotData.deviceId = this.txtProductId.Text;
            lotData.lotId = this.txtLotId.Text;
            lotData.lotQty = Convert.ToInt32(this.lblLotqty.Content);

            lotData.QRNG = Convert.ToInt32(this.lblQrNGCount.Content);
            lotData.QROK = Convert.ToInt32(this.lblQrOKCount.Content);
            lotData.lotCount = Convert.ToInt32(this.lblLotCount.Content);
            lotData.QrYield = this.lblQrYield.Content.ToString();
            UiManager.SaveAppSettings();
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

        #region Old
        private void btnMainStart1_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
                return;
            UserManager.createUserLog(UserActions.MAIN_BUTTON_START);
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Start Auto Running", System.Windows.Window.GetWindow(this)))
            {
                CheckReady();
            }
            return;
        }
        private void btnMainStop1_Click(object sender, RoutedEventArgs e)
        {
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Stop Machine", System.Windows.Window.GetWindow(this)))
            {
                StopEvent();
            }
            return;
        }
        #endregion

        #endregion

        #region Initial & Release
        private bool isLoadWsSuccess = false;
        void Init()
        {
            Task tsk2 = new Task(() =>
            {
                isLoadWsSuccess = false;
                if (control.Workspaces.Count > 0)
                {
                    // Lưu danh sách Workspace cần xóa
                    var toRemove = control.Workspaces.ToList();
                    foreach (var ws in toRemove)
                    {
                        string name = ws.UniqueName;

                        // Nếu có stream liên kết, gán null trước khi remove workspace
                        if (Stream1 != null && Workspaces.Contains(ws) && Stream1 == ws.Streams.First())
                        {
                            Stream1 = null;
                            Stream2 = null;
                        }

                        try
                        {
                            control.Workspaces.Remove(name);
                        }
                        catch (Exception ex)
                        {
                            addLog($"Remove Workspace {name} failed: {ex.Message}");
                        }
                    }
                    Workspaces.Clear(); // gán danh sách Workspace của bạn rỗng nếu có
                }
                for (int i = 0; i < dlJob.Count; i++)
                {
                    if (dlJob[i].Wspace.Contains(".vrws"))
                    {
                        try
                        {
                            OpenWorkSpace(dlJob[i].Wspace);
                            addLog(String.Format(@"Open Workspace {0} Success", dlJob[i].name));
                        }
                        catch
                        {
                            addLog(String.Format(@"Open Workspace {0} Error....", dlJob[i].name));
                        }
                    }
                }
                isLoadWsSuccess = true;
            });
            tsk2.Start();
        }

        public void OpenToolBlock(string nameTB, string filePath)
        {
            //CogToolBlock cogTB = CogSerializer.LoadObjectFromFile(filePath) as CogToolBlock;
            //var cogTbJob = new Tuple<string, CogToolBlock>(nameTB, cogTB);
            //cogTbJobList.Add(cogTbJob);
        }

        private void CheckReady()
        {
            if (!this.Ready)
            {
                //displayAlarm(30000);
            }
            if (!UiManager.PLC.IsConnected)
            {
                //displayAlarm(30001);
                return;
            }
            if (!Camera1IsConnect)
            {
                //displayAlarm(30002);
                return;
            }

            //ui
            var converter = new BrushConverter();
            IsRunning = true;
            btnMainStart.IsEnabled = false;
            btnMainStop.IsEnabled = true;
            lbl_status.Background = (Brush)converter.ConvertFromString("#228b22");
            lbl_status.Content = "RUNNING";
            RunManager();
        }
        #endregion

        #region Camera Connect
        private bool InnitialCamera1()
        {
            MyCamera.MV_CC_DEVICE_INFO device = UiManager.appSettings.connection.camera1.device;
            int ret = myCam1.Open(device, HikCam.AquisMode.AcquisitionMode);
            Thread.Sleep(10);
            if (ret == MyCamera.MV_OK)
            {
                Camera1IsConnect = true;
                myCam1.SetExposeTime((int)UiManager.appSettings.connection.camera1.ExposeTime);
                return true;
            }
            else
            {
                Camera1IsConnect = false;
                return false;
            }

        }
        #endregion

        #region Workspace manual check 
        public ViDi2.Runtime.IControl Control
        {
            get { return control; }
            set
            {
                control = value;
                RaisePropertyChanged(nameof(Control));
                RaisePropertyChanged(nameof(Workspaces));
                RaisePropertyChanged(nameof(Stream1));
                RaisePropertyChanged(nameof(Stream2));
            }
        }

        public IList<ViDi2.Runtime.IWorkspace> Workspaces => Control.Workspaces.ToList();

        /// <summary>
        /// Gets or sets the current workspace
        /// </summary>
        public ViDi2.IWorkspace Workspace
        {
            get { return workspace; }
            set
            {
                workspace = value;
                Mat src = Cv2.ImRead("temp2.bmp");
                IImage img = vision1.getVidiImage(src);
                Stream1 = workspace.Streams.First();
                // warm up operation, first call to process takes additionnal time
                Stream1?.Process(img);

                Stream2 = workspace.Streams.First();
                // warm up operation, first call to process takes additionnal time
                Stream2?.Process(img);
                RaisePropertyChanged(nameof(Workspace));
            }
        }

        /// <summary>
        /// Gets or sets the current stream
        /// </summary>
        public ViDi2.IStream Stream1
        {
            get { return stream1; }
            set
            {
                stream1 = value;
                sampleViewer.Sample = null;
                RaisePropertyChanged(nameof(Stream1));
            }
        }
        public ViDi2.IStream Stream2
        {
            get { return stream2; }
            set
            {
                stream2 = value;
                sampleViewer.Sample = null;
                RaisePropertyChanged(nameof(Stream2));
            }
        }

        public ISampleViewerViewModel SampleViewerViewModel => sampleViewer.ViewModel;
        public ISampleViewerViewModel SampleViewerViewModel1 => sampleViewer1.ViewModel;

        void sampleViewer_ToolSelected(ViDi2.ITool tool)
        {
            //RaisePropertyChanged(nameof(SampleViewer));
        }
        void sampleViewer_ToolSelected1(ViDi2.ITool tool)
        {
            //RaisePropertyChanged(nameof(SampleViewer));
        }

        void CheckDrop(object sender, DragEventArgs e)
        {
            //var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            //bool isArchive = System.IO.Path.GetExtension(lst.First()) == ".vsa";

            //e.Effects = stream != null || isArchive ? DragDropEffects.All : DragDropEffects.None;
            //e.Handled = true;
        }
        void CheckDrop1(object sender, DragEventArgs e)
        {
            //var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            //bool isArchive = System.IO.Path.GetExtension(lst.First()) == ".vsa";

            //e.Effects = stream != null || isArchive ? DragDropEffects.All : DragDropEffects.None;
            //e.Handled = true;
        }

      
        private void DoDrop(object sender, DragEventArgs e)
        {
            var setting = UiManager.appSettings;
           
            List<SampleViewer> lstView = new List<SampleViewer> { sampleViewer, sampleViewer1 };
            //var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            //var image = new ViDi2.UI.WpfImage(@"C:\Users\Admin\Desktop\No Wire\31.png");
            //gọi ảnh Deep Learning
            var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            var image = new ViDi2.UI.WpfImage(lst.First());

            //chạy các Job DL đã khời tạo
            if (Workspaces.Count < UiManager.appSettings.CurrentModel.dLJobs.Count)
                addLog(String.Format("Workspace Open total {0}", Workspaces.Count));
            for (int j = 0; j < Workspaces.Count; j++)
            {
                Stream1 = Workspaces[j].Streams.First();
                //double score = DeepCheck(image, sampleViewer);
            }
        }
        private void DoDrop1(object sender, DragEventArgs e)
        {
            //var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            var image = new ViDi2.UI.WpfImage(@"C:\Users\Admin\Desktop\No Wire\31.png");
            DeepCheck(image, sampleViewer1);

        }
        private ISample DeepCheck(WpfImage src, SampleViewer input)
        {
            double ret = 0;
            ISample e ; 
            try
            {
                
                e = Stream1.Process(src, "1");
                //input.Sample = e;
                //RaisePropertyChanged(nameof(ViewIndices));
                IRedMarking redMarking = e.Markings["Analyze"] as IRedMarking;
                foreach (IRedView view in redMarking.Views)
                {
                    ret = view.Score;
                }
                return e;
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
                return null;
            }
            
        }
        private ISample DeepCheckMultiGPU(WpfImage src, int GPUNo)
        {
           // int count = control.ComputeDevices.Count;
            //double ret = 0;
            try
            {
                //ISample e;
                if (GPUNo == 0)
                {
                    return Stream1.Process(src, "1", new List<int>() { 0 });
                }
                else
                {
                    return Stream2.Process(src, "1", new List<int>() { 1 });
                }

                //IRedMarking redMarking = e.Markings["Analyze"] as IRedMarking;
                //foreach (IRedView view in redMarking.Views)
                //{
                //    ret = view.Score;
                //}
                //return e;
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
                return null;
            }

        }
        //private double DeepCheck(WpfImage src, SampleViewer input)
        //{
        //    double ret = 0;
        //    try
        //    {
        //        input.Sample = Dispatcher.Invoke(() =>
        //        {
        //            return Stream.Process(src, "1");
        //        });


        //        Dispatcher.Invoke(() =>
        //        {
        //            RaisePropertyChanged(nameof(ViewIndices));
        //        });


        //        if (input.Sample?.Markings != null && input.Sample.Markings.ContainsKey(input.ToolName))
        //        {
        //            IRedMarking redMarking = input.Sample.Markings[input.ToolName] as IRedMarking;
        //            foreach (IRedView view in redMarking.Views)
        //            {
        //                ret = view.Score;
        //            }
        //        }
        //        else
        //        {
        //            addLog("input.Sample.Markinng");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        addLog($"Error in DeepCheck: {ex.Message}");
        //    }

        //    return ret;
        //}


        private void open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".vrws",
                Filter = "ViDi Runtime Workspaces (*.vrws)|*.vrws"
            };

            if ((bool)dialog.ShowDialog() == true)
            {
                using (var fs = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Open, FileAccess.Read))
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    //Workspace = Control.Workspaces.Add(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName), fs);
                    Mouse.OverrideCursor = null;
                    Mouse.OverrideCursor = null;
                    try
                    {
                        OpenWorkSpace(dialog.FileName);

                    }
                    catch (Exception ex)
                    {
                        addLog($"{ex.Message}");
                    }
                }
            }
            RaisePropertyChanged(nameof(Workspaces));
        }

        private void OpenWorkSpace(String fileName)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read))
            {
                Workspace = Control.Workspaces.Add(System.IO.Path.GetFileNameWithoutExtension(fileName), fs);
            }
        }

        public Dictionary<int, string> ViewIndices
        {
            get
            {
                var indices = new Dictionary<int, string>();
                if (sampleViewer.Sample != null && sampleViewer.ToolName != "")
                {
                    var views = sampleViewer.Sample.Markings[sampleViewer.ToolName].Views;
                    indices.Add(-1, "all");
                    for (int i = 0; i < views.Count; ++i)
                        indices.Add(i, i.ToString());
                }
                return indices;
            }
        }
        #region Properties
        public static int JigColumns { get => UiManager.appSettings.Jig.columnCount;}
        public static int JigRows { get => UiManager.appSettings.Jig.rowCount;}
        #endregion

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Timer
      
        private void Clock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if(UiManager.PLC.IsConnected)
                {
                    //UpdateError();
                }
                UpdateStatusConnectMES(UiManager.MES.IsConnected);
                CheckConnect();

                if(READ_MACHINE_RUNNING)
                {
                    isRunning = true;
                }    
                else
                {
                    isRunning = false;
                }
               
                var converter = new BrushConverter();
            }
            catch (Exception ex)
            {
                logger.Create(ex.Message.ToString());
            }
        }
        #endregion

        #region Auto
        void RunManager()
        {
            callThreadStartLoopCH1();
        }
        private void callThreadStartLoopCH1()
        {
            try
            {
                Thread startThread = new Thread(new ThreadStart(waitTriggerCH1));
                startThread.IsBackground = true;
                startThread.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString());
            }

        }
        private void waitTriggerCH1()
        {

           
        }
        void RunManager2()
        {
            callThreadStartLoopCH2();
        }
        #endregion
        private void callThreadStartLoopCH2()
        {
            try
            {
                Thread startThread = new Thread(new ThreadStart(waitTriggerCH2));
                startThread.IsBackground = true;
                startThread.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString());
            }

        }
        private void waitTriggerCH2()
        {
          
        }
        #region stop

        private void StopEvent()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var converter = new BrushConverter();
                    IsRunning = false;
                    btnMainStart.IsEnabled = true;
                    btnMainStop.IsEnabled = false;
                    lbl_status.Background = (Brush)converter.ConvertFromString("#FF4500");
                    lbl_status.Content = "STOP";
                });
            }
            catch (Exception ex)
            {
                logger.Create("Main Start Err : " + ex.ToString());
            }
        }

        #endregion
        public Boolean IsRunningAuto()
        {
            return isRunning;
        }

        #region BUTTON PG
        private void BtnMainReset_Click(object sender, RoutedEventArgs e)
        {
            
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_RESET, true);
            Thread.Sleep(20);
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_RESET, false);
            addLog("WRITE BIT RESET");
        }
        private void BtnMainHome_Click(object sender, RoutedEventArgs e)
        {
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_HOME, true);
            Thread.Sleep(20);
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_HOME, false);
            addLog("WRITE BIT HOME");

           
        }
        private void BtnMainStop_Click(object sender, RoutedEventArgs e)
        {
            
            isRunning1 = false;
            isRunning2 = false;

            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, true);
            Thread.Sleep(20);
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, false);
            addLog("WRITE BIT STOP ");
        }
        private void BtnMainStart_Click(object sender, RoutedEventArgs e)
        {
            //ProcessVisionProCH1(new Mat(), new Mat(), out int rl, out string mess);
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_START, true);
            Thread.Sleep(20);
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_START, false);
            addLog("WRITE BIT START");
        }
        #endregion

        #region INTAL COLO
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
        #endregion

        #region Read PLC
        private void CallReadPLC()
        { 
                runThread3 = new Thread(new ThreadStart(ReadPLC));
                runThread3.IsBackground = true;
                runThread3.Start();

        }
        private void ReadPLC()
        {
            try
            {
               
                    if (UiManager.PLC.IsConnected)
                    {
                        READ_MACHINE_RUNNING = Get_MACHINE_RUNNING();

                        var Read_bt_Start = Get_BT_START();
                        var Read_bt_Stop = Get_BT_STOP();
                        var Read_bt_Home = Get_BT_HOME();
                        READ_LAMP_RESET = Get_RESET();
                        var Read_bt_Reset = Get_BT_RESET();

                        var PLC_TIME = PLCRunTime();
                        var ONE_CYCLE = PLCCycleTime();

                        READ_ALARM_01 = GetAlarm01();
                        //READ_ALARM_02 = GetAlarm02();
                        //READ_ALARM_03 = GetAlarm03();
                        //READ_ALARM_04 = GetAlarm04();
                        //READ_ALARM_05 = GetAlarm05();
                        //READ_ALARM_06 = GetAlarm06();
                        //READ_ALARM_07 = GetAlarm07();
                        //READ_ALARM_08 = GetAlarm08();
                        //READ_ALARM_09 = GetAlarm09();
                        //READ_ALARM_10 = GetAlarm10();


                        this.Dispatcher.Invoke(() =>
                        {
                           
                            this.btnMainStart.Background = new SolidColorBrush(Read_bt_Start ? Colo_ON : Colo_OFF);
                            this.btnMainStop.Background = new SolidColorBrush(Read_bt_Stop ? Colo_ON : Colo_OFF);
                            this.btnMainHome.Background = new SolidColorBrush(Read_bt_Home ? Colo_ON : Colo_OFF);
                            this.btnMainReset.Background = new SolidColorBrush(Read_bt_Reset ? Colo_ON : Colo_OFF);

                            this.lbl_status.Background = new SolidColorBrush(READ_MACHINE_RUNNING ? Colo_ON : Colo_OFF1);
                            this.lbl_status.Content = READ_MACHINE_RUNNING ? "MACHINE RUN" : "MACHINE STOP";

                            this.lbPLCRunTime.Content = PLC_TIME.ToString();
                            this.lbCycleTime.Content = ONE_CYCLE.ToString();


                           


                        });
                        UpdateError();


                    }
                    Thread.Sleep(10);
                    CallReadPLC();


            }
            catch (Exception ex)
            {

                logger.Create($"Thread Read PLC :{ex}");
            }
        }
        public short GetAlarm01()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_01);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm02()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_02);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm03()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_03);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm04()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_04);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm05()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_05);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm06()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_06);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm07()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_07);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm08()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_08);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm09()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_09);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public short GetAlarm10()
        {
            short ret = 0;
            try
            {
                ret = UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.ALARM_10);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double PLCRunTime()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadDoubleWord_Int32(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_PLC_RUN_Time) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public double PLCCycleTime()
        {
            double ret = 0;
            Int16 SCALE = 10;
            try
            {
                ret = (double)UiManager.PLC.ReadWord_Int16(FENETProtocolDeviceName.BYTE_DB, PlcStore.D_ONE_CYCLE) / SCALE;
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Read Position Current Axis Y Error: " + ex.Message));
                return ret;
            }
            return ret;
        }
        public bool Get_MACHINE_RUNNING()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_MACHINE_RUNING);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_QR1_TRIG: " + ex.Message));
                return false;
            }
        }
        public bool Get_QR1_TRIG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_QR1_TRIG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_QR1_TRIG: " + ex.Message));
                return false;
            }
        }
        public bool Get_QR2_TRIG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_QR2_TRIG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_QR2_TRIG: " + ex.Message));
                return false;
            }
        }
        public bool Get_VISION1_TRIG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION1_TRIG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_VISION1_TRIG: " + ex.Message));
                return false;
            }
        }
        public bool Get_VISION2_TRIG()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION2_TRIG);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_VISION2_TRIG: " + ex.Message));
                return false;
            }
        }
        public bool Get_CLR_ALL()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_CLR_ALL);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_CHECK: " + ex.Message));
                return false;
            }
        }
        public bool Get_CHECK_MES()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_CHECK_MES);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_CHECK: " + ex.Message));
                return false;
            }
        }
        public bool Get_BT_START()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.READ_BT_START);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_BT_START: " + ex.Message));
                return false;
            }
        }
        public bool Get_BT_STOP()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.READ_BT_STOP);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_BT_STOP: " + ex.Message));
                return false;
            }
        }
        public bool Get_BT_HOME()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.READ_BT_HOME);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_BT_HOME: " + ex.Message));
                return false;
            }
        }
        public bool Get_BT_RESET()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_KX, PlcStore.READ_BT_RESET);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_BT_RESET: " + ex.Message));
                return false;
            }
        }
        public bool Get_RESET()
        {
            try
            {
                return UiManager.PLC.ReadBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_RESET);
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("READ_BT_RESET: " + ex.Message));
                return false;
            }
        }
        #endregion

        #region Auto RUN
        private void CH1()
        {
            lock (_lock)
            {
                try
                {
                    if (isRunning1)
                    {
                        addLog("already RUNNING!");
                        return;
                    }
                    if (requireNewLot)
                    {
                        return;
                    }

                    Qr1Manager.StartEvent();

                    isRunning1 = true;

                    RunThreadCH1();
                }
                catch (Exception ex)
                {
                    logger.Create("btStart.click error: " + ex.Message);
                }
            }
        }
        private void RunThreadCH1()
        {

            listData.PkgQrListCH1 = new List<String>();
            listData.PkgVisionListCH1 = new List<bool>();
            listData.ResultVisionListCH1 = new List<String>();
            listData.ColoVisionCH1 = new List<int>();

            testIndexCH1 = 0;
          
            CreateNewTestResult();

            ClearResultCH1();

            callThreadStartCH1();
        }
        private void callThreadStartCH1()
        {
            try
            {
                runThread1 = new Thread(RunManagerCH1);
                runThread1.IsBackground = true;
                runThread1.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString());
            }

        }
        private void RunManagerCH1()
        {
            //lock (_lock)
            {
                try
                {
                    if (!isRunning1 && !isLoadWsSuccess)
                    {
                        return;
                    }
                    READ_VISION1_TRIG = Get_VISION1_TRIG();
                    if (READ_VISION1_TRIG && !Flag1 && isLoadWsSuccess)
                    {
                        Flag1 = true;

                        //Task.Run(() => { this.RunManageVisionCH1();});
                        Task.Factory.StartNew(() => this.RunManageVisionCH1());
                       
                    }


                    READ_QR1_TRIG = Get_QR1_TRIG();
                    if (READ_QR1_TRIG && !Flag2)
                    {
                        Flag2 = true;
                        
                        //Task.Run(() => { this.RunManageQRCH1(); });
                        Task.Factory.StartNew(() => this.RunManageQRCH1());

                    }



                    READ_CLR_ALL = Get_CLR_ALL();
                    if (READ_CLR_ALL)
                    {
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_CLR_ALL, false);
                        this.CLRALL();
                        CreateNewTestResult();
                        CaculatorCycleTime();
                        Thread.Sleep(30);
                        MesCheck = false;

                        READ_CLR_ALL = false;
                    }

                    READ_CHECK_MES = Get_CHECK_MES();
                    if (READ_CHECK_MES)
                    {
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_CHECK_MES, false);

                        CheckMes();
                        cycleRunning = false;
                        MesCheck = true;
                        READ_CHECK_MES= false;

                        //InitControl();
                        //Init();
                    }

                    Thread.Sleep(10);
                    callThreadStartCH1();

                }
                catch (Exception ex)
                {

                    logger.Create($"Auto Run CH1 Error : +{ex}");
                }

            }
        }
       
        List<Mat> ImageLst1 = new List<Mat>();
        Mat[] ImageViewLst1 = new Mat[JigColumns * JigRows * JigColumns];
        private void RunManageVisionCH1()
        {
            addLog("Read:PLC > PC TRIG VISION 1");
            // Result Vision True
            int ResultVision = 1;
            String Mess = "OK";

            // 1 = true
            // 2 = false
            // 3 = Emty
            int columns = UiManager.appSettings.Jig.columnCount;

            if(!UiManager.appSettings.run.ByPassVision)
            {
                ProcessVisionVidiCH1(out ResultVision, out Mess);
            }    
            else
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION1_TRIG, false);
            }
            
            if (ResultVision == 1)
            {
                listData.ResultVisionListCH1.Add(Mess);
                DisplayResultVisionCH1(listData.ResultVisionListCH1);

                listData.PkgVisionListCH1.Add(true);
                listData.ColoVisionCH1.Add(ResultVision);
                SetCellColorCH1( listData.ColoVisionCH1);

                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION1_OK, true);
                addLog("Send:PC > PLC VISION 1 OK");

                CheckOK();
            }
            // Result Vision False
            if (ResultVision == 2)
            {
                listData.ResultVisionListCH1.Add(Mess);
                DisplayResultVisionCH1(listData.ResultVisionListCH1);

                listData.PkgVisionListCH1.Add(false);
                listData.ColoVisionCH1.Add(ResultVision);
                SetCellColorCH1( listData.ColoVisionCH1);


                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION1_NG, true);
                addLog("Send:PC > PLC VISION 1 NG");

                CheckNG();

            }
            if (ResultVision == 3)
            {
                listData.ResultVisionListCH1.Add(Mess);
                DisplayResultVisionCH1(listData.ResultVisionListCH1);

                listData.PkgVisionListCH1.Add(false);

                listData.ColoVisionCH1.Add(ResultVision);
                SetCellColorCH1( listData.ColoVisionCH1);
                //listData.PkgQrListCH1.Add("EMTY");
 

                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION1_EMTY, true);
                addLog("Send:PC > PLC VISION 1 EMTY");

               
            }
            this.Dispatcher.Invoke(() =>
            {
                lblResCh1.Content = Mess;
                if (ResultVision == 1)
                {
                    lblResCh1.Background = Brushes.Green;
                }
                else { lblResCh1.Background = Brushes.Red; }
            });

            Flag1 = false;


        }
        public void ProcessVisionVidiCH1(out int ResultVision, out string mess)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                lblResCh1.Background = Brushes.White;
                lblResCh1.Content = "";
            }));
            ResultVision = 1;
            mess = "OK";
            ISample maxModel = null;
            try
            {
                Mat src = UiManager.Cam1.CaptureImage();
                //Mat src = Cv2.ImRead("D:\\20250116\\Short\\Image_20250114141334149.bmp", ImreadModes.Color);
                //Mat src = Cv2.ImRead("temp2.bmp", ImreadModes.Color);
                addLog("CH1 capture Image Finish");

                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION1_TRIG, false);

                Mat display = src.Clone();
                ImageLst1.Add(display);
                ImageViewLst1[(ImageLst1.Count - 1) + mulIdx1] = display;

                if ((ImageLst1.Count - 1) % JigColumns != 0)
                {
                    mulIdx1 += JigColumns;
                }

                this.Dispatcher.Invoke(() =>
                {
                    imgViewCh1.Source = display.ToWriteableBitmap(PixelFormats.Bgr24);
                });

                vision1.Image1 = src;

                var wsEmpty = Workspaces.FirstOrDefault(s => s.UniqueName.Contains("Empty"));
                using (WpfImage vidiMatLst = vision1.getVidiImage(src.Clone()))
                {
                    Stream1 = wsEmpty.Streams.First();
                    double score = 0;
                    using (ISample eE = DeepCheckMultiGPU(vidiMatLst, 0))
                    {
                        IRedMarking redMarking = eE.Markings["Analyze"] as IRedMarking;

                        foreach (IRedView view in redMarking.Views)
                        {
                            score = view.Score;
                        }

                        DLJob dlJobEmpty = dlJob.FirstOrDefault(x => x.name.Contains("Empty"));
                        if (score > (double)dlJobEmpty.Score / 100.0)
                        {
                            addLog($"Score CH1 - {wsEmpty.UniqueName}: {score}");
                            ResultVision = 3;
                            mess = "NAN";
                            goto End;
                        }
                    }
                }
                addLog("CH1 yes/no Check Finish");
                // Chạy Step VIDI
                //if (!vision1.YesNoVIDICheck(src))
                //{
                //    ResultVision = 3;
                //    mess = "NAN";
                //    goto End;
                //}
                //addLog("CH1 yes/no Check Finish");

                // Chạy các Job DL đã khởi tạo
                if (Workspaces.Count < UiManager.appSettings.CurrentModel.dLJobs.Count)
                {
                    addLog(string.Format("Workspace Open total {0}", Workspaces.Count));
                }

                double maxScore = 0;
                using (WpfImage vidiMatLst = vision1.getVidiImage(src.Clone()))
                {
                    for (int i = 0; i < Workspaces.Count; i++)
                    {
                        if (Workspaces[i].UniqueName.Contains("Empty")) { continue; }
                        Stream1 = Workspaces[i].Streams.First();
                        double score = 0;
                        using (ISample e = DeepCheckMultiGPU(vidiMatLst, 0))
                        {
                            IRedMarking redMarking = e.Markings["Analyze"] as IRedMarking;

                            foreach (IRedView view in redMarking.Views)
                            {
                                score = view.Score;
                            }

                            if (score > (double)dlJob[i].Score / 100.0)
                            {
                                if (score > maxScore)
                                {
                                    maxScore = score;
                                    // Dispose sample trước đó nếu có
                                    maxModel?.Dispose();
                                    maxModel = e;
                                    mess = Workspaces[i].UniqueName;
                                }

                                if (maxModel == null && i == Workspaces.Count - 1)
                                {
                                    maxModel = e;
                                }
                                addLog($"Score CH1 - {Workspaces[i].UniqueName}: {score}");
                                ResultVision = 2;
                            }
                        }
                    }
                }

                if (ResultVision == 2)
                {
                    SaveImage(display, @"ImageSave\CH1", mess);
                }
            End:
                addLog("Loop end CH1");
            }
            catch (Exception ex)
            {
                logger.Create($"Vision CH1 Error :{ex}");
                addLog($"Vision CH1 Error :{ex}");
                ResultVision = 3;
                mess = "Error";
            }
        }
        private void RunManageQRCH1()
        {
          
            addLog("Read:PLC > PC TRIG QR 1");
            var qr = UiManager.Scanner1.ReadQR();
            //var qr = "QR Test CH1";
            int CountScanner = 0;

            while (CountScanner < UiManager.appSettings.run.scannerNumberTrigger)
            {

                if (!string.IsNullOrEmpty(qr))
                {
                    break;
                }
                qr = UiManager.Scanner1.ReadQR();
                CountScanner++;
                Thread.Sleep(300);
            }
           
            if (!string.IsNullOrEmpty(qr))
            {
               if(UiManager.appSettings.run.CheckMixLot)
                {
                    var Lot = UiManager.appSettings.lotData.lotId;
                    var SubQR = qr.Substring(0,5);

                    if(!SubQR.Equals(Lot))
                    {
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, true);
                        Thread.Sleep(20);
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, false);
                        AddError(9000);
                    }    

                }    
                listData.PkgQrListCH1.Add(qr);
                DisplayQRCH1(listData.PkgQrListCH1);
                addLog("Start WRITE_QR1_OK");
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_QR1_OK, true);
               
                addLog("Send:PC > PLC TRIG QR 1 OK");
                testIndexCH1++;
            }
            else
            {
                qr = "ERROR";
                listData.PkgQrListCH1.Add(qr);
                DisplayQRCH1(listData.PkgQrListCH1);
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_QR1_NG, true);
                addLog("Send:PC > PLC TRIG QR 1 NG");
                testIndexCH1++;
            }
            //Lưu ảnh NG
            //string resultVision = listData.ResultVisionListCH1[listData.PkgQrListCH1.Count - 1];
            //if (!resultVision.Contains("OK"))
            //{
            //    SaveImage(ImageLst1[listData.PkgQrListCH1.Count - 1], @"ImageSave\CH1", resultVision, qr);
            //}
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_QR1_TRIG, false);
            Flag2 = false;
         
        }
        //private void ProcessVisionProCH1(Mat srcSIPLeft, Mat srcSIPRight, out int resultVision, out string mess)
        //{
        //    resultVision = 1;
        //    mess = "OK";
        //    srcSIPLeft = BitmapConverter.ToMat(new System.Drawing.Bitmap("C:\\Users\\Admin\\MVS\\Data\\SIP01_MASTER\\Image_20250204160539118.bmp"));
        //    srcSIPRight = BitmapConverter.ToMat(new System.Drawing.Bitmap("C:\\Users\\Admin\\MVS\\Data\\SIP02_MASTER\\Image_20250204160622082.bmp"));
        //    try
        //    {
        //        CogToolBlock cogTbCheckSIP = new CogToolBlock();

        //        //Phần chụp và hiển thị ảnh lên màn hình

        //        //Phần xử lý trong CogToolBlock
        //        foreach (var cogTbJob in cogTbJobList)
        //        {
        //            if (cogTbJob.Item1.Contains("SIP"))
        //            {
        //                cogTbCheckSIP = cogTbJob.Item2 as CogToolBlock;
        //                break;
        //            }
        //        }
        //        if (cogTbCheckSIP.Name == string.Empty)
        //            return;
        //        //Import Images
        //        cogTbCheckSIP.Inputs["InputImage1"].Value = new CogImage24PlanarColor(srcSIPLeft.ToBitmap());
        //        cogTbCheckSIP.Inputs["InputImage2"].Value = new CogImage24PlanarColor(srcSIPRight.ToBitmap());
        //        cogTbCheckSIP.Run();
        //        if (cogTbCheckSIP.Outputs["Exception"].Value.ToString() != "")
        //        {
        //            addLog("CogToolBlock Check SIP Error: " + cogTbCheckSIP.Outputs["Exception"].Value.ToString());
        //            logger.Create("CogToolBlock Check SIP Error: " + cogTbCheckSIP.Outputs["Exception"].Value.ToString());
        //            return;
        //        }
        //        if(cogTbCheckSIP.Outputs["Result"].Value.ToString().Contains("OK"))
        //        {
        //            resultVision = 1;
        //        }
        //        else if(cogTbCheckSIP.Outputs["Result"].Value.ToString().Contains("NG"))
        //        {
        //            resultVision = 2;
        //        }
        //        mess = cogTbCheckSIP.Outputs["Result"].Value.ToString();
        //    }
        //    catch(Exception ex)
        //    {
        //        addLog($"Process VisionPro CH1 Error: {ex}");
        //        logger.Create($"Process VisionPro CH1 Error: {ex}");
        //    }

        //}    
        private void CLRALL()
        {
            addLog("Read:PLC > PC Clear All");
            ImageLst1.Clear();
            ImageLst2.Clear();
            Array.Clear(ImageViewLst1, 0, ImageViewLst1.Length);
            Array.Clear(ImageViewLst2, 0, ImageViewLst2.Length);
            mulIdx1 = 0;
            mulIdx2 = 0;

            listData.PkgQrListCH1.Clear();
            listData.PkgVisionListCH1.Clear();
            listData.ResultVisionListCH1.Clear();
            listData.ColoVisionCH1.Clear();

            listData.PkgQrListCH2.Clear();
            listData.PkgVisionListCH2.Clear();
            listData.ResultVisionListCH2.Clear();
            listData.ColoVisionCH2.Clear();

            testIndexCH2 = 0;
            testIndexCH1 = 0;
            ClearResultCH1();
            ClearResultCH2();
            Dispatcher.Invoke(() =>
            {
                this.lbResultMes.Content = "Wait....";
                this.lbResultMes.Background = Brushes.Orange;
            });

            
            GC.Collect();

            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_CLR_ALL_OK, true);
            addLog("Send:PC > PLC Clear All OK");
            return;
        }
        private void SaveImage(Mat imgSave, string path, string errName, string qrCode)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string lotID = UiManager.appSettings.lotData.lotId;
                string config = UiManager.appSettings.lotData.deviceId;

                var folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogData", currentDate, lotID, path);
                var checkSpacePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogData");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                if (!Directory.Exists(checkSpacePath))
                {
                    Directory.CreateDirectory(checkSpacePath);
                }
                string fileName = $"{currentDate} {DateTime.Now.ToString("HH-mm-ss ")} {lotID}-{qrCode}-{errName}.png";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                DriveInfo driveInfo = new DriveInfo(System.IO.Path.GetPathRoot(folderPath));
                long freeSpace = driveInfo.AvailableFreeSpace;

                long minimumFreeSpace = 1024L * 1024L * 1024L * 20L;
                if (freeSpace < minimumFreeSpace)
                {
                    DeleteOldestFile(folderPath);
                }

                // 20000pcs * 15MB = 300000MB = 300GB. 2CH => 150GB = 150.000MB
                //CheckFolderSize(folderPath, 150000);
                bool a = Cv2.ImWrite(fullPath, imgSave);
                addLog("Save Image NG " + (a ? "Success!" : "Fail!"));
            }
            catch(Exception ex)
            {
                addLog("Save Image NG Error: " + ex.Message);
            } 
            
        }
        private void SaveImage(Mat imgSave, string path, string errName)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string lotID = UiManager.appSettings.lotData.lotId;
                string config = UiManager.appSettings.lotData.deviceId;

                var folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogData", currentDate, lotID, path);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string fileName = $"{currentDate} {DateTime.Now.ToString("HH-mm-ss ")} {lotID}-{errName}.png";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                //Đoạn code kiểm tra dung lượng ổ đĩa
                var checkSpacePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "LogData");
                if (!Directory.Exists(checkSpacePath))
                {
                    Directory.CreateDirectory(checkSpacePath);
                }
                CheckFolderSize(checkSpacePath);

                // 20000pcs * 15MB = 300000MB = 300GB. 2CH => 150GB = 150.000MB
                //CheckFolderSize(folderPath, 150000);
                bool a = Cv2.ImWrite(fullPath, imgSave);
                addLog("Save Image NG " + (a ? "Success!" : "Fail!"));
            }
            catch (Exception ex)
            {
                addLog("Save Image NG Error: " + ex.Message);
            }

        }
        private void CheckFolderSize(string folderPath, long maxSizeInMB)
        {
            try
            {
                long totalSize = Directory.GetFiles(folderPath).Sum(file => new System.IO.FileInfo(file).Length);
                long totalSizeInMB = (totalSize / (1024 * 1024));

                if (totalSizeInMB >= maxSizeInMB)
                {
                    var oldestFile = Directory.GetFiles(folderPath)
                        .Select(f => new System.IO.FileInfo(f))
                        .OrderBy(f => f.LastWriteTime)
                        .FirstOrDefault();

                    if (oldestFile != null)
                    {
                        File.Delete(oldestFile.FullName);
                        logger.Create("Deleted oldest file");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Create("Delete oldest Logs file error: " + ex.Message);
            }
        }
        private void DeleteOldestFile(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var files = directoryInfo.GetFiles();

            if (files.Length == 0)
            {
                logger.Create("Không có file nào trong thư mục để xóa.");
                return;
            }

            // Tìm file cũ nhất
            var oldestFile = files[0];

            foreach (var file in files)
            {
                if (file.LastWriteTime < oldestFile.LastWriteTime)
                {
                    oldestFile = file;
                }
            }

            // Xóa file cũ nhất
            logger.Create($"Đang xóa file: {oldestFile.Name}");
            oldestFile.Delete();
        }
        private void CheckFolderSize(string folderPath)
        {
            DriveInfo driveInfo = new DriveInfo(System.IO.Path.GetPathRoot(folderPath));
            long freeSpace = driveInfo.AvailableFreeSpace;
            //luôn để trống 20GB
            long minimumFreeSpace = 1024L * 1024L * 1024L * 20L;
            if (freeSpace > minimumFreeSpace)
            {
                return;
            }
            DirectoryInfo[] folders = new DirectoryInfo(folderPath).GetDirectories();
            if (folders.Length == 0)
            {
                logger.Create("Không có folder nào trong thư mục để xóa.");
                return;
            }
            //Lấy ra List folder được sắp xếp theo thời gian từ cũ đến mới nhất
            var childenDir = folders
                .OrderBy(dir => dir.CreationTime).ToList();
            List<string> searchFiles = new List<string>();
            DirectoryInfo oldestDir = childenDir[0];
            //Tìm kiếm xem folder nào cũ nhất mà có chứa ảnh
            foreach (var dir in childenDir)
            {
                try
                {
                    var filesArr = Directory.GetFiles(dir.FullName, "*.png", SearchOption.AllDirectories);
                    if (filesArr.Length > 0)
                    {
                        searchFiles = filesArr.ToList();
                        oldestDir = dir;
                        break;
                    }
                }
                catch(Exception ex)
                {
                    logger.Create("Lỗi khi tìm kiếm file trong folder: " + ex.Message);
                    break;
                } 
            } 
            //Nếu file cũ nhất lại là file mới nhất thì thực hiện xóa folder thay vì xóa ảnh
            if(oldestDir.CreationTime.Date == DateTime.Today)
            {
                try
                {
                    Directory.Delete(childenDir[0].FullName, true);
                    logger.Create("Đã xóa folder cũ nhất");
                }
                catch (Exception ex)
                {
                    logger.Create($"Không thể xóa folder {childenDir[0]}: {ex.Message}");
                }
                //Gọi lại dệ quy để kiểm tra không gian ổ đĩa
                CheckFolderSize(folderPath);
            }
            else
            {
                //Tìm và xóa tất cả các ảnh trong folder cũ nhất
                foreach (var file in searchFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        logger.Create($"Không thể xóa file {file}: {ex.Message}");
                    }
                }
                logger.Create("Đã xóa các ảnh trong folder cũ nhất");
            }
        }
        private void CH2()
        {
            
                try
                {
                    if (isRunning2)
                    {
                        addLog("already RUNNING!");
                        return;
                    }

                    // Request user to enter new LOT:
                    if (requireNewLot)
                    {
                        //displayAlarm("The new Lot will be changed.", 0, AlarmInfo.NEW_LOT_REQUIRE);
                        return;
                    }

                    // START event:
                    Qr1Manager.StartEvent();

                    // Start running task asynchronously:
                    isRunning2 = true;
                    RunThreadCH2();
                }
                catch (Exception ex)
                {
                    logger.Create("btStart.click error: " + ex.Message);
                
            }
        }
        private void RunThreadCH2()
        {
            listData.PkgQrListCH2 = new List<String>();
            listData.ResultVisionListCH2 = new List<String>();
            listData.PkgVisionListCH2 = new List<bool>();
            listData.ColoVisionCH2 = new List<int>();

            testIndexCH2 = 0;
            ClearResultCH2();

            callThreadStartCH2();
        }
        private void callThreadStartCH2()
        {
            try
            {
                runThread2 = new Thread(RunManagerCH2);
                runThread2.IsBackground = true;
                runThread2.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString());
            }

        }
        private void RunManagerCH2()
        {
            //lock (_lock)
            {
                try
                {
                    if (!isRunning2 && !isLoadWsSuccess)
                    {
                        return;
                    }

                    READ_VISION2_TRIG = Get_VISION2_TRIG();
                    if (READ_VISION2_TRIG &&!Flag3 && isLoadWsSuccess)
                    {
                        Flag3 = true;
                       
                        Task.Run(() => { this.RunManageVisionCH2();});
                      
                    }

                    READ_QR2_TRIG = Get_QR2_TRIG();
                    if (READ_QR2_TRIG && !Flag4)
                    {
                        Flag4 = true;
                        Task.Run(() => { this.RunManageQRCH2(); });
                       
                    }

                    Thread.Sleep(10);
                    callThreadStartCH2();
                }
                catch (Exception ex)
                {

                    logger.Create($"Auto Run CH1 Error : +{ex}");
                }
            }         
        }
       
        List<Mat> ImageLst2 = new List<Mat>();
        Mat[] ImageViewLst2 = new Mat[JigColumns * JigRows * JigColumns];
        private void RunManageVisionCH2()
        {
            addLog("Read:PLC > PC TRIG VISION 2");
            // Result Vision True
            int ResultVision = 1;
            string Mess = "OK";

            Stopwatch timeVidi = new Stopwatch();
            timeVidi.Reset();
            timeVidi.Start();
            if (!UiManager.appSettings.run.ByPassVision)
            {
                ProcessVisionVidiCH2(out ResultVision, out Mess);
            }
            else
            {
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION2_TRIG, false);
            }
          
            if (ResultVision == 1)
            {
                
                listData.ResultVisionListCH2.Add(Mess);
                DisplayResultVisionCH2(listData.ResultVisionListCH2);

                listData.PkgVisionListCH2.Add(true);
                listData.ColoVisionCH2.Add(ResultVision);
                SetCellColorCH2( listData.ColoVisionCH2);



                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION2_OK, true);
                addLog("Send:PC > PLC VISION 2 OK");

                CheckOK();
            }
            // Result Vision False
            if (ResultVision == 2)
            {
                listData.ResultVisionListCH2.Add(Mess);
                DisplayResultVisionCH2(listData.ResultVisionListCH2);

                listData.PkgVisionListCH2.Add(false);
                listData.ColoVisionCH2.Add(ResultVision);
                SetCellColorCH2( listData.ColoVisionCH2);



                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION2_NG, true);
                addLog("Send:PC > PLC VISION 2 NG");

                CheckNG();
            }
            if (ResultVision == 3)
            {
                listData.ResultVisionListCH2.Add(Mess);
                DisplayResultVisionCH2(listData.ResultVisionListCH2);

                listData.PkgVisionListCH2.Add(false);
                listData.ColoVisionCH2.Add(ResultVision);
                SetCellColorCH2( listData.ColoVisionCH2);
                
               
                
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_VISION2_EMTY, true);
                addLog("Send:PC > PLC VISION 2 EMTY");

            }
            this.Dispatcher.Invoke(() =>
            {
                lblResCh2.Content = Mess;
                if (ResultVision == 1) {
                    lblResCh2.Background = Brushes.Green;
                }
                else { lblResCh2.Background = Brushes.Red; }
            });

            Flag3 = false;
            

        }
        public void ProcessVisionVidiCH2(out int ResultVision, out string Mess)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                lblResCh2.Background = Brushes.White;
                lblResCh2.Content = "";
            }));
            ResultVision = 1;
            Mess = "OK";
            ISample maxModel = null;
            try
            {
                Mat src = UiManager.Cam2.CaptureImage();

                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_VISION2_TRIG, false);

                addLog("CH2 capture Image Finish");
                Mat display = src.Clone();
                ImageLst2.Add(display);
                ImageViewLst2[(ImageLst2.Count - 1) + mulIdx2 + 2] = display;
                if ((ImageLst2.Count - 1) % JigColumns != 0)
                {
                    mulIdx2 += JigColumns;
                }
                this.Dispatcher.Invoke(() =>
                {
                    imgViewCh2.Source = display.ToWriteableBitmap(PixelFormats.Bgr24);
                });

                vision2.Image1 = src;

                foreach (var wsEmpty in Workspaces)
                {
                    if (wsEmpty.UniqueName.Contains("Empty"))
                    {
                        using (WpfImage vidiMatLst = vision2.getVidiImage(src.Clone()))
                        {
                            Stream2 = wsEmpty.Streams.First();
                            double score = 0;
                            using (ISample e = DeepCheckMultiGPU(vidiMatLst, 1))
                            {
                                IRedMarking redMarking = e.Markings["Analyze"] as IRedMarking;

                                foreach (IRedView view in redMarking.Views)
                                {
                                    score = view.Score;
                                }

                                DLJob dlJobEmpty = dlJob.Find(x => x.name.Contains("Empty"));
                                if (score > (double)dlJobEmpty.Score / 100.0)
                                {
                                    addLog($"Score CH2 - {wsEmpty.UniqueName}: {score}");
                                    ResultVision = 3;
                                    Mess = "NAN";
                                    goto End;
                                }
                            }
                        }
                        break;
                    }
                }
                addLog("CH2 yes/no Check Finish");

                //Chạy Step VIDI
                //if (!vision2.YesNoVIDICheck(src))
                //{
                //    ResultVision = 3;
                //    Mess = "NAN";
                //    goto End;
                //}
                //addLog("CH2 yes/no Check Finish");
                //chạy các Job DL đã khời tạo
                if (Workspaces.Count < UiManager.appSettings.CurrentModel.dLJobs.Count)
                    addLog(String.Format("Workspace Open total {0}", Workspaces.Count));

                double maxScore = 0;

                using (WpfImage vidiMatLst = vision2.getVidiImage(src.Clone()))
                {
                    for (int i = 0; i < Workspaces.Count; i++)
                    {
                        if (Workspaces[i].UniqueName.Contains("Empty")) { continue; }
                        Stream2 = Workspaces[i].Streams.First();
                        double score = 0;
                        using (ISample e = DeepCheckMultiGPU(vidiMatLst, 1))
                        {
                            IRedMarking redMarking = e.Markings["Analyze"] as IRedMarking;
                            foreach (IRedView view in redMarking.Views)
                            {
                                score = view.Score;
                            }

                            if (score > (double)dlJob[i].Score / (double)100)
                            {
                                if (score > maxScore)
                                {
                                    maxScore = score;
                                    // Dispose sample trước đó nếu có
                                    maxModel?.Dispose();
                                    maxModel = e;
                                    Mess = Workspaces[i].UniqueName;
                                }

                                if (maxModel == null && i == Workspaces.Count - 1)
                                {
                                    maxModel = e;
                                }

                                addLog($"Score CH2 - {Workspaces[i].UniqueName}: {score}");
                                ResultVision = 2;
                            }
                        }
                    }
                }
                if (ResultVision == 2)
                {
                    SaveImage(display, @"ImageSave\CH2", Mess);
                }
            //ImageLst2.Add(maxModel);
            End:
                addLog("Loop end CH2");
                
            }
            catch (Exception ex)
            {
                logger.Create($"Vision CH2 Error :{ex}");
                addLog($"Vision CH2 Error :{ex}");
                ResultVision = 3;
                Mess = "Error";
            }
        }
        private void RunManageQRCH2()
        {
            addLog("Read:PLC > PC TRIG QR 2");
            var qr = UiManager.Scanner2.ReadQR();
            //var qr = "QR Test CH2";

            int CountScanner = 0;

            while (CountScanner < UiManager.appSettings.run.scannerNumberTrigger)
            {
                if (!string.IsNullOrEmpty(qr))
                {
                    break;
                }
                qr = UiManager.Scanner2.ReadQR();
                CountScanner++;
                Thread.Sleep(300);
            };

            if (!string.IsNullOrEmpty(qr))
            {
                if (UiManager.appSettings.run.CheckMixLot)
                {
                    var Lot = UiManager.appSettings.lotData.lotId;
                    var SubQR = qr.Substring(0, 5);

                    if (!SubQR.Equals(Lot))
                    {
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, true);
                        Thread.Sleep(20);
                        UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_KX, PlcStore.WRITE_BT_STOP, false);
                        AddError(9000);
                    }

                }
                listData.PkgQrListCH2.Add(qr);
                testAllPkgBad = false;
                DisplayQRCH2(listData.PkgQrListCH2);
                addLog("Start WRITE_QR2_OK");
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_QR2_OK, true);
                addLog("Send:PC > PLC TRIG QR 2 OK");
                testIndexCH2++;
            }
            else
            {
                qr = "ERROR";
                listData.PkgQrListCH2.Add(qr);

                DisplayQRCH2(listData.PkgQrListCH2);
            

                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_QR2_NG, true);
                addLog("Send:PC > PLC TRIG QR 2 NG");
                testIndexCH2++;
            }
            //Lưu ảnh NG
            //string resultVision = listData.ResultVisionListCH2[listData.PkgQrListCH2.Count - 1];
            //if (!resultVision.Contains("OK"))
            //{
            //    SaveImage(ImageLst2[listData.PkgQrListCH2.Count - 1], @"ImageSave\CH2", resultVision, qr);
            //}
            UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.READ_QR2_TRIG, false);
            Flag4 = false;

        }
        //private void ProcessVisionProCH2(Mat srcSIPLeft, Mat srcSIPRight, out int resultVision, out string mess)
        //{
        //    resultVision = 1;
        //    mess = "OK";
        //    try
        //    {
        //        CogToolBlock cogTbCheckSIP = new CogToolBlock();

        //        //Phần chụp và hiển thị ảnh lên màn hình

        //        //Phần xử lý trong CogToolBlock
        //        foreach (var cogTbJob in cogTbJobList)
        //        {
        //            if (cogTbJob.Item1.Contains("SIP"))
        //            {
        //                cogTbCheckSIP = cogTbJob.Item2 as CogToolBlock;
        //                break;
        //            }
        //        }
        //        if (cogTbCheckSIP.Name == string.Empty)
        //            return;
        //        //Import Images
        //        cogTbCheckSIP.Inputs["InputImage1"].Value = new CogImage24PlanarColor(srcSIPLeft.ToBitmap());
        //        cogTbCheckSIP.Inputs["InputImage2"].Value = new CogImage24PlanarColor(srcSIPRight.ToBitmap());
        //        cogTbCheckSIP.Run();
        //        if (cogTbCheckSIP.Outputs["Exception"].Value.ToString() != "")
        //        {
        //            addLog("CogToolBlock Check SIP Error: " + cogTbCheckSIP.Outputs["Exception"].Value.ToString());
        //            logger.Create("CogToolBlock Check SIP Error: " + cogTbCheckSIP.Outputs["Exception"].Value.ToString());
        //            return;
        //        }
        //        if (cogTbCheckSIP.Outputs["Result"].Value.ToString().Contains("OK"))
        //        {
        //            resultVision = 1;
        //        }
        //        else if (cogTbCheckSIP.Outputs["Result"].Value.ToString().Contains("NG"))
        //        {
        //            resultVision = 2;
        //        }
        //        mess = cogTbCheckSIP.Outputs["Result"].Value.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        addLog($"Process VisionPro CH2 Error: {ex}");
        //        logger.Create($"Process VisionPro CH2 Error: {ex}");
        //    }

        //}

        #region initialize Gird

        private void GenerateGrids(int rows, int columns)
        {
            DynamicGrid1.RowDefinitions.Clear();
            DynamicGrid1.ColumnDefinitions.Clear();
            DynamicGrid1.Children.Clear();

            DynamicGrid2.RowDefinitions.Clear();
            DynamicGrid2.ColumnDefinitions.Clear();
            DynamicGrid2.Children.Clear();


            for (int i = 0; i < rows; i++)
            {
                DynamicGrid1.RowDefinitions.Add(new RowDefinition());
                DynamicGrid2.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < columns; j++)
            {
                DynamicGrid1.ColumnDefinitions.Add(new ColumnDefinition());
                DynamicGrid2.ColumnDefinitions.Add(new ColumnDefinition());
            }


            int number = 1; // Bắt đầu từ 1

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {

                    AddButtonToGrid(DynamicGrid1, i, j, number++);
                }

                for (int j = 0; j < columns; j++)
                {

                    AddButtonToGrid(DynamicGrid2, i, j, number++);
                }
            }
        }
        private void AddButtonToGrid(Grid grid, int row, int column, int number)
        {
            var contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Hàng đầu tiên
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Hàng thứ hai
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Cột đầu tiên
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Cột thứ hai

            // Text1 (ở cột 0, hàng 0)
            var text1 = new TextBlock
            {
                Text = number.ToString() + " : ",
                Foreground = Brushes.White
            };
            Grid.SetRow(text1, 0);
            Grid.SetColumn(text1, 0);
            contentGrid.Children.Add(text1);

            // Text2 (ở cột 1, hàng 0)
            var text2 = new TextBlock
            {
                Text = "", 
                Foreground = Brushes.White
            };
            Grid.SetRow(text2, 0);
            Grid.SetColumn(text2, 1);
            contentGrid.Children.Add(text2);

            // Text3 (ở cột 0, hàng 1, kéo dài cả 2 cột)
            var text3 = new TextBlock
            {
                Text = "", 
                Foreground = Brushes.White
            };
            Grid.SetRow(text3, 1);
            Grid.SetColumnSpan(text3, 2); // Kéo dài Text3 qua 2 cột
            contentGrid.Children.Add(text3);



            var button = new Button
            {
                Content = contentGrid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(1, 1, 1, 1),
                Tag = number
            };

            // Gắn sự kiện Click nếu cần
            if (grid == DynamicGrid1)
                button.Click += Button_Click1;
            else
                button.Click += Button_Click2;

            Grid.SetRow(button, row);
            Grid.SetColumn(button, column);
            grid.Children.Add(button);
        }

        private void SetCellColorCH1(List<int> qrStatus)
        {
            int startIndex = 0;
            int colStart = 0;
            int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

            this.Dispatcher.Invoke(() =>
            {
                for (int index = 0; index < qrStatus.Count; index++)
                {
                    int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                    int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                    // Lấy ô nút trong DynamicGrid1
                    var cell = DynamicGrid1.Children
                        .Cast<UIElement>()
                        .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                    if (cell is Button button)
                    {
                        // Đặt màu nền dựa trên giá trị trong qrStatus
                        switch (qrStatus[index])
                        {
                            case 1:
                                button.Background = Brushes.Green;
                                break;
                            case 2:
                                button.Background = Brushes.Red;
                                break;
                            case 3:
                                button.Background = Brushes.Yellow;
                                break;
                            default:
                                button.Background = Brushes.Gray; // Màu mặc định nếu không hợp lệ
                                break;
                        }
                    }
                }
            });
        }
        private void SetCellColorCH2(List<int> qrStatus)
        {
            int startIndex = 0;
            int colStart = 0;
            int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

            this.Dispatcher.Invoke(() =>
            {
                for (int index = 0; index < qrStatus.Count; index++)
                {
                    int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                    int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                    // Lấy ô nút trong DynamicGrid1
                    var cell = DynamicGrid2.Children
                        .Cast<UIElement>()
                        .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                    if (cell is Button button)
                    {
                        // Đặt màu nền dựa trên giá trị trong qrStatus
                        switch (qrStatus[index])
                        {
                            case 1:
                                button.Background = Brushes.Green;
                                break;
                            case 2:
                                button.Background = Brushes.Red;
                                break;
                            case 3:
                                button.Background = Brushes.Yellow;
                                break;
                            default:
                                button.Background = Brushes.Gray; // Màu mặc định nếu không hợp lệ
                                break;
                        }
                    }
                }
            });
        }
        //private void SetCellColorCH1(int index, List<int> qrStatus)
        //{
        //    //lock (_lock)
        //    {
        //        if (index < 0 || index >= qrStatus.Count)
        //        {
        //            return;
        //        }

        //        int colStart = 0;
        //        int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

        //        this.Dispatcher.Invoke(() =>
        //        {
        //            int currentRow = index / (colEnd - colStart + 1);
        //            int currentCol = colStart + (index % (colEnd - colStart + 1));

        //            // Lấy ô nút trong DynamicGrid1
        //            var cell = DynamicGrid1.Children
        //                .Cast<UIElement>()
        //                .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

        //            if (cell is Button button)
        //            {
        //                // Đặt màu nền dựa trên giá trị trong qrStatus
        //                switch (qrStatus[index])
        //                {
        //                    case 1:
        //                        button.Background = Brushes.Green;
        //                        break;
        //                    case 2:
        //                        button.Background = Brushes.Red;
        //                        break;
        //                    case 3:
        //                        button.Background = Brushes.Yellow;
        //                        break;
        //                    default:
        //                        button.Background = Brushes.Gray; // Màu mặc định nếu không hợp lệ
        //                        break;
        //                }
        //            }
        //        });
        //    }
        //}
        //private void SetCellColorCH2(int index, List<int> qrStatus)
        //{
        //    //lock (_lock)
        //    {
        //        if (index < 0 || index >= qrStatus.Count)
        //        {
        //            return;
        //        }

        //        int colStart = 0;
        //        int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid2 trừ 1

        //        this.Dispatcher.Invoke(() =>
        //        {
        //            int currentRow = index / (colEnd - colStart + 1);
        //            int currentCol = colStart + (index % (colEnd - colStart + 1));

        //            // Lấy ô nút trong DynamicGrid2
        //            var cell = DynamicGrid2.Children
        //                .Cast<UIElement>()
        //                .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

        //            if (cell is Button button)
        //            {
        //                // Đặt màu nền dựa trên giá trị trong qrStatus
        //                switch (qrStatus[index])
        //                {
        //                    case 1:
        //                        button.Background = Brushes.Green;
        //                        break;
        //                    case 2:
        //                        button.Background = Brushes.Red;
        //                        break;
        //                    case 3:
        //                        button.Background = Brushes.Yellow;
        //                        break;
        //                    default:
        //                        button.Background = Brushes.Gray; // Màu mặc định nếu không hợp lệ
        //                        break;
        //                }
        //            }
        //        });
        //    }
        //}

        private void DisplayQRCH1(List<string> resultData)
        {
            //lock (_lock)
            {
                int startIndex = 0;
                int colStart = 0;
                int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

                this.Dispatcher.Invoke(() =>
                {
                    for (int index = 0; index < resultData.Count; index++)
                    {
                        int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                        int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                        // Lấy ô nút
                        var cell = DynamicGrid1.Children
                            .Cast<UIElement>()
                            .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Lấy Text3 (ở hàng 1, cột 0, kéo dài cả 2 cột)
                            var resultTextBlock = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 1 && Grid.GetColumnSpan(e) == 2) as TextBlock;

                            if (resultTextBlock != null)
                            {
                                resultTextBlock.Text = resultData[index];
                            }
                        }
                    }
                });
            }
        }
        private void DisplayResultVisionCH1(List<string> resultData)
        {
            //lock (_lock)
            {
                int startIndex = 0;
                int colStart = 0;
                int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

                this.Dispatcher.Invoke(() =>
                {
                    for (int index = 0; index < resultData.Count; index++)
                    {
                        int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                        int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                        // Lấy ô nút
                        var cell = DynamicGrid1.Children
                            .Cast<UIElement>()
                            .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Lấy Text2 (nằm ở cột 1, hàng 0)
                            var resultTextBlock = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 1) as TextBlock;

                            if (resultTextBlock != null)
                            {
                                resultTextBlock.Text = resultData[index];
                            }
                        }
                    }
                });
            }
        }

        private void DisplayQRCH2(List<string> resultData)
        {
            //lock (_lock)
            {
                int startIndex = 0;
                int colStart = 0;
                int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

                this.Dispatcher.Invoke(() =>
                {
                    for (int index = 0; index < resultData.Count; index++)
                    {
                        int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                        int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                        // Lấy ô nút
                        var cell = DynamicGrid2.Children
                            .Cast<UIElement>()
                            .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Lấy Text3 (ở hàng 1, cột 0, kéo dài cả 2 cột)
                            var resultTextBlock = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 1 && Grid.GetColumnSpan(e) == 2) as TextBlock;

                            if (resultTextBlock != null)
                            {
                                resultTextBlock.Text = resultData[index];
                            }
                        }
                    }
                });
            }
        }
        private void DisplayResultVisionCH2(List<string> resultData)
        {
            //lock (_lock)
            {
                int startIndex = 0;
                int colStart = 0;
                int colEnd = UiManager.appSettings.Jig.columnCount - 1; // Số cột trong DynamicGrid1 trừ 1

                this.Dispatcher.Invoke(() =>
                {
                    for (int index = 0; index < resultData.Count; index++)
                    {
                        int currentRow = (startIndex + index) / (colEnd - colStart + 1);
                        int currentCol = colStart + ((startIndex + index) % (colEnd - colStart + 1));

                        // Lấy ô nút
                        var cell = DynamicGrid2.Children
                            .Cast<UIElement>()
                            .FirstOrDefault(e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol);

                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Lấy Text2 (nằm ở cột 1, hàng 0)
                            var resultTextBlock = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 1) as TextBlock;

                            if (resultTextBlock != null)
                            {
                                resultTextBlock.Text = resultData[index];
                            }
                        }
                    }
                });
            }
        }

        private void ClearResultCH2()
        {
            lock (_lock)
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var cell in DynamicGrid2.Children.Cast<UIElement>())
                    {
                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Xóa nội dung của Text2 (hàng 0, cột 1)
                            var text2 = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 1) as TextBlock;
                            if (text2 != null)
                            {
                                text2.Text = ""; // Xóa nội dung Text2
                            }

                            // Xóa nội dung của Text3 (hàng 1, kéo dài cả hai cột)
                            var text3 = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 1 && Grid.GetColumnSpan(e) == 2) as TextBlock;
                            if (text3 != null)
                            {
                                text3.Text = ""; // Xóa nội dung Text3
                            }

                            // Đặt lại màu nền của nút về mặc định (Transparent)
                            button.Background = Brushes.Transparent;
                        }
                    }
                });
            }
        }
        private void ClearResultCH1()
        {
            lock (_lock)
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var cell in DynamicGrid1.Children.Cast<UIElement>())
                    {
                        if (cell is Button button && button.Content is Grid contentGrid)
                        {
                            // Xóa nội dung của Text2 (hàng 0, cột 1)
                            var text2 = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 1) as TextBlock;
                            if (text2 != null)
                            {
                                text2.Text = ""; // Xóa nội dung Text2
                            }

                            // Xóa nội dung của Text3 (hàng 1, kéo dài cả hai cột)
                            var text3 = contentGrid.Children
                                .Cast<UIElement>()
                                .FirstOrDefault(e => Grid.GetRow(e) == 1 && Grid.GetColumnSpan(e) == 2) as TextBlock;
                            if (text3 != null)
                            {
                                text3.Text = ""; // Xóa nội dung Text3
                            }

                            // Đặt lại màu nền của nút về mặc định (Transparent)
                            button.Background = Brushes.Transparent;
                        }
                    }
                });
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (!MesCheck)
                return;
            Button clickedButton = sender as Button;
            try
            {
                if (clickedButton != null)
                {

                    // Lấy số tương ứng từ Tag và in ra
                    int ImageNo = (int)clickedButton.Tag;

                    imgViewCh2.Source = ImageViewLst2[ImageNo - 1].ToWriteableBitmap(PixelFormats.Bgr24);

                }
            }
            catch (Exception ex)
            {
                addLog("Button2 Error: " + ex.Message);
            }
            //Show Image 


        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (!MesCheck)
                return;
            Button clickedButton = sender as Button;
            try
            {
                if (clickedButton != null)
                {
                    int ImageNo = (int)clickedButton.Tag;

                    imgViewCh1.Source = ImageViewLst1[ImageNo - 1].ToWriteableBitmap(PixelFormats.Bgr24);

                }
            }
            catch (Exception ex)
            {
                addLog("Button1 Error: " + ex.Message);
            }
            //Show Image 


        }
        #endregion

        #endregion
        private void UpdateTestResult(List<bool> ResultVision, List<bool> ResultVision2)
        {
     
            if (ResultVision == null || ResultVision2 == null || this.ResultRunning == null)
            {
                MessageBox.Show("ResultVision, ResultVision2 hoặc ResultRunning không được null.");
                return;
            }

         
            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell * 2;

            List<bool> combinedResult = ResultVision.Concat(ResultVision2).ToList();

            int minCount = Math.Min(combinedResult.Count, Number);

            for (int i = 0; i < minCount; i++)
            {
                
                this.ResultRunning[i].ResultVision = combinedResult[i];
         
                this.ResultRunning[i].TimePKG_Read_QRCode = DateTime.Now;
            }
        }
        private void UpdateTestResult_Scanner(List<string> PkgQrListCH1, List<string> PkgQrListCH2)
        {
            
            if (PkgQrListCH1 == null || PkgQrListCH2 == null || this.ResultRunning == null)
            {
                MessageBox.Show("PkgQrListCH1, PkgQrListCH2 hoặc ResultRunning không được null.");
                return;
            }
            List<string> combinedList = PkgQrListCH1.Concat(PkgQrListCH2).ToList();

            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell * 2;

            
            int minCount = Math.Min(combinedList.Count, Number);

            for (int i = 0; i < minCount; i++)
            {
                this.ResultRunning[i].QrCodePKG = combinedList[i];
                this.ResultRunning[i].TimePKG_Read_QRCode = DateTime.Now;
            }
        }
        private void UpdateTestResult_Vision(List<string> PkgQrListCH1, List<string> PkgQrListCH2)
        {

            if (PkgQrListCH1 == null || PkgQrListCH2 == null || this.ResultRunning == null)
            {
                MessageBox.Show("PkgQrListCH1, PkgQrListCH2 hoặc ResultRunning không được null.");
                return;
            }
            List<string> combinedList = PkgQrListCH1.Concat(PkgQrListCH2).ToList();

            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell * 2;


            int minCount = Math.Min(combinedList.Count, Number);

            for (int i = 0; i < minCount; i++)
            {
                this.ResultRunning[i].ResultVision_String = combinedList[i];
            }
        }
        private void CreateNewTestResult()
        {
            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount ;
            int Number = NumberCell + NumberCell;
            this.ResultRunning = Enumerable.Range(0, Number)
                                .Select(i => new DataCheckPKG())
                                .ToArray();
        }
        private void CreateCellGril2(int rows , int columns)
        {
            grid2nd.RowDefinitions.Clear();
            grid2nd.ColumnDefinitions.Clear();
            grid2nd.Children.Clear();

            for (int i = 0; i < rows; i++)
            {
                grid2nd.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < columns; j++)
            {
                grid2nd.ColumnDefinitions.Add(new ColumnDefinition());
            }
            int NumberLabel = 1;
           
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Label label = new Label
                    {
                        Content = "", 
                        Background = Brushes.LightGray, 
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                  
                    Grid.SetRow(label, i);
                    Grid.SetColumn(label, j);

                    
                    grid2nd.Children.Add(label);
                    NumberLabel++;
                }
                
            }

        }
        private void UpdateGridData()
        {
            int NumberLabel = 0;
            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell;
            Dispatcher.Invoke(() => 
            {
                foreach (var child in grid2nd.Children)
                {
                    if (child is Label label && NumberLabel < Number)
                    {

                        var result = this.ResultRunning[NumberLabel];


                        label.Content = result.ResultMES ? $"{result.QrCodePKG}" : $"{result.QrCodePKG}^{result.MESFeedback}" ;
                        label.Background = result.ResultMES ? Brushes.LightGreen : Brushes.IndianRed;

                        NumberLabel++;
                    }
                }
            });
           

            
        }
        private void UpdateGridDataOffline()
        {
            int NumberLabel = 0;
            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell;
            Dispatcher.Invoke(() =>
            {
                foreach (var child in grid2nd.Children)
                {
                    if (child is Label label && NumberLabel < Number)
                    {

                        var result = this.ResultRunning[NumberLabel];


                        label.Content =  $"{result.QrCodePKG}: MES OFFLINE";
                        label.Background = Brushes.LightGreen;

                        NumberLabel++;
                    }
                }
            });



        }
        private void CreateCellGril2cCH2(int rows, int columns)
        {
            grid2nd1.RowDefinitions.Clear();
            grid2nd1.ColumnDefinitions.Clear();
            grid2nd1.Children.Clear();

            for (int i = 0; i < rows; i++)
            {
                grid2nd1.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < columns; j++)
            {
                grid2nd1.ColumnDefinitions.Add(new ColumnDefinition());
            }
            int NumberLabel = 1;

            for (int i = 0; i < rows; i++)
            {

                for (int j = 0; j < columns; j++)
                {
                    Label label = new Label
                    {
                        Content = "",
                        Background = Brushes.LightGray,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };


                    Grid.SetRow(label, i);
                    Grid.SetColumn(label, j);


                    grid2nd1.Children.Add(label);
                    NumberLabel++;
                }

            }

        }
        private void UpdateGridDataCH2()
        {
            
            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell * 2;
            int NumberLabel = NumberCell;
            Dispatcher.Invoke(() =>
            {
                foreach (var child in grid2nd1.Children)
                {
                    if (child is Label label && NumberLabel < Number)
                    {

                        var result = this.ResultRunning[NumberLabel];


                        label.Content = result.ResultMES ? $"{result.QrCodePKG}" : $"{result.QrCodePKG}^{result.MESFeedback}";
                        label.Background = result.ResultMES ? Brushes.LightGreen : Brushes.IndianRed;

                        NumberLabel++;
                    }
                }
            });
        }
        private void UpdateGridDataCH2Offline()
        {

            int NumberCell = UiManager.appSettings.Jig.rowCount * UiManager.appSettings.Jig.columnCount;
            int Number = NumberCell * 2;
            int NumberLabel = NumberCell;
            Dispatcher.Invoke(() =>
            {
                foreach (var child in grid2nd1.Children)
                {
                    if (child is Label label && NumberLabel < Number)
                    {

                        var result = this.ResultRunning[NumberLabel];


                        label.Content = $"{result.QrCodePKG}:MES OFFLINE";
                        label.Background = Brushes.LightGreen;

                        NumberLabel++;
                    }
                }
            });



        }
        private void CheckMes()
        {
            var lotData = UiManager.appSettings.lotData;
            if (UiManager.appSettings.run.mesOnline)
            {
                #region MES CHECK ONINE
                UpdateTestResult_Scanner(listData.PkgQrListCH1, listData.PkgQrListCH2);
                UpdateTestResult_Vision(listData.ResultVisionListCH1, listData.ResultVisionListCH2);
                UpdateTestResult(listData.PkgVisionListCH1, listData.PkgVisionListCH2);
                testIndexCH1 = 100;
                testIndexCH2 = 100;
                Dispatcher.Invoke(() =>
                {
                    this.lbResultMes.Content = "Wait MES Check.........";
                    this.lbResultMes.Background = Brushes.LightYellow;
                });

                var resultCheckMES = UiManager.MES.CheckQRCodes(this.ResultRunning, MES_CHECKING_TIMEOUT);
                if (resultCheckMES == null)
                {
                    addLog(String.Format("MES Check Result Timeout."));
                    Dispatcher.Invoke(() =>
                    {
                        this.lbResultMes.Content = "MES Check Result Timeout.";
                        this.lbResultMes.Background = Brushes.Red;
                    });

                    return;
                }
                bool hasError = false;
                for (int i = 0; i < resultCheckMES.Count; i++)
                {
                    #region 
                    var sortDataFeedback = resultCheckMES[i].Split('^');
                    {
                        if (sortDataFeedback.Length == 2)
                        {
                            this.ResultRunning[i].ResultMES = String.Equals(sortDataFeedback[0], "OK") ? true : false;
                            this.ResultRunning[i].MESFeedback = sortDataFeedback[1];


                        }
                        if (this.ResultRunning[i].QrCodePKG != "ERROR")
                        {
                            if (!this.ResultRunning[i].ResultMES)
                            {
                                hasError = true;
                            }
                            logger.CreateMES(ResultRunning[i].QrCodePKG, ResultRunning[i].ResultVision_String, resultCheckMES[i]);
                        }
                    }
                    #endregion     
                }
                if (hasError)
                {
                    UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_MES_NG, true);
                }
                UiManager.PLC.WriteBit(FENETProtocolDeviceName.BIT_MX, PlcStore.WRITE_MES_OK, true);
                Dispatcher.Invoke(() =>
                {
                    this.lbResultMes.Content = "MES Check Complete";
                    this.lbResultMes.Background = Brushes.LightGreen;
                });

                UpdateGridData();
                UpdateGridDataCH2();
                UpdateResult();



                #endregion
            }
            else
            {
                #region MES CHECK OFFLINE
                Dispatcher.Invoke(() =>
                {
                    this.lbResultMes.Content = "MES Check Offline Complete";
                    this.lbResultMes.Background = Brushes.LightGreen;
                });
                UpdateGridDataOffline();
                UpdateGridDataCH2Offline();
                #endregion
            }

        }
        private void UpdateResult()
        {
            Dispatcher.Invoke(() => 
            {
                var lotData = UiManager.appSettings.lotData;
                this.lblLotqty.Content = lotData.lotQty;
                this.lblQrNGCount.Content = lotData.QRNG;
                this.lblQrOKCount.Content = lotData.QROK;
                this.lblLotCount.Content = lotData.lotCount;
                double yield = (Convert.ToDouble(lotData.QROK) / Convert.ToDouble(lotData.lotCount)) * 100d;
                this.lblQrYield.Content = String.Format(yield.ToString("F2") + "%");
            });
        }
        private void DeleteResult()
        {
            Dispatcher.Invoke(() =>
            {
                var lotData = UiManager.appSettings.lotData;
                lotData.QRNG = 0;
                lotData.QROK = 0;
                lotData.lotCount = 0;
                this.lblLotqty.Content = lotData.lotQty;
                this.lblQrNGCount.Content = lotData.QRNG;
                this.lblQrOKCount.Content = lotData.QROK;
                this.lblLotCount.Content = lotData.lotCount;
                double yield = (Convert.ToDouble(lotData.QROK) / Convert.ToDouble(lotData.lotCount)) * 100d;
                this.lblQrYield.Content = String.Format(yield.ToString("F2") + "%");
            });
        }
        private void CaculatorCycleTime()
        {
            // Datetime Start
            this.cycleStart = DateTime.Now;

            // Start cycle time:
            this.cycleRunning = true;
        }
        private void CheckOK()
        {
            var lotData = UiManager.appSettings.lotData;
            lotData.QROK++;
            lotData.lotCount++;
        }
        private void CheckNG()
        {
            var lotData = UiManager.appSettings.lotData;
            lotData.QRNG++;
            lotData.lotCount++;
        }
        #region CheckConnect
        private void CheckConnect()
        {
            this.Dispatcher.Invoke(() =>
            {
                if(UiManager.PLC.IsConnected)
                {
                    lblPLCOnline.Content = "Connect";
                    lblPLCOnline.Background = Brushes.Green;
                }    
                else
                {
                    lblPLCOnline.Content = "Disconnect";
                    lblPLCOnline.Background = Brushes.Red;
                }
                if (UiManager.Scanner1.IsConnected)
                {
                    lblScannerOnline.Content = "Connect";
                    lblScannerOnline.Background = Brushes.Green;
                }
                else
                {
                    lblScannerOnline.Content = "Disconnect";
                    lblScannerOnline.Background = Brushes.Red;
                }
                if (UiManager.Scanner2.IsConnected)
                {
                    lblScannerOnlineCH2.Content = "Connect";
                    lblScannerOnlineCH2.Background = Brushes.Green;
                }
                else
                {
                    lblScannerOnlineCH2.Content = "Disconnect";
                    lblScannerOnlineCH2.Background = Brushes.Red;
                }
            });
        }
        private void UpdateStatusConnectMES(bool status)
        {
            this.Dispatcher.Invoke(() => {
                if (status && UiManager.appSettings.run.mesOnline)
                {
                    this.lblMesOnline.Content = "Connected";
                    this.lblMesOnline.Background = Brushes.Green;
                    this.chkMcsAccepted.IsChecked = true;
                    this.chkMcsListen.IsChecked = false;
                    this.lblMcsStatus.Content = String.Format("MES: Connected.");
                    this.lblMcsStatus.Background = Brushes.Cyan;
                    return;
                }
                else if (!status && UiManager.appSettings.run.mesOnline)
                {
                    this.lblMesOnline.Content = "Disconnect";
                    this.lblMesOnline.Background = Brushes.Red;
                    this.chkMcsListen.IsChecked = true;
                    this.chkMcsAccepted.IsChecked = false;
                    var msg = "Listening MES Connect...";
                    this.lblMcsStatus.Content = msg;
                    this.lblMcsStatus.Background = Brushes.Orange;
                }


                else if (!UiManager.appSettings.run.mesOnline)
                {
                    this.lblMesOnline.Content = "Run Offline";
                    this.lblMesOnline.Background = Brushes.DarkCyan;
                    this.chkMcsListen.IsChecked = true;
                    this.chkMcsAccepted.IsChecked = false;
                    var msg = " Run Offline";
                    this.lblMcsStatus.Content = msg;
                    this.lblMesOnline.Background = Brushes.Pink;
                }


            });
        }
        private void UpdateUIMES()
        {
            if (UiManager.appSettings.connection.EquipmentName != null)
            {
                this.lbEquipmentId.Text = UiManager.appSettings.connection.EquipmentName;
            }
            var arr = UiManager.appSettings.MesSettings1.localIp.Split('.');
            if (arr.Length == 4)
            {
                this.txtLocalIp1.Text = arr[0];
                this.txtLocalIp2.Text = arr[1];
                this.txtLocalIp3.Text = arr[2];
                this.txtLocalIp4.Text = arr[3];
            }
            this.txtMcsLocalPort.Text = UiManager.appSettings.MesSettings1.localPort.ToString();
            this.txtCurrentModel.Text = UiManager.appSettings.currentModel.ToString();
        }
        #endregion

        private void UpdateError()
        {

            Dispatcher.Invoke(() =>
            {
                if (UiManager.PLC.IsConnected)
                {
                    this.AddError(READ_ALARM_01);
                    ////this.AddError(READ_ALARM_02);
                    ////this.AddError(READ_ALARM_03);
                    ////this.AddError(READ_ALARM_04);
                    ////this.AddError(READ_ALARM_05);
                    ////this.AddError(READ_ALARM_06);
                    ////this.AddError(READ_ALARM_07);
                    ////this.AddError(READ_ALARM_08);
                    ////this.AddError(READ_ALARM_09);
                    ////this.AddError(READ_ALARM_10);


                    if ((READ_ALARM_01 == 0) && !hasClearedError)
                    {
                        this.ClearError();
                        hasClearedError = true; // Đặt cờ để ngăn chạy lại hàm này
                    }
                    else if (READ_ALARM_01 != 0)
                    {
                        // Khi D_ListShortDevicePLC_0[200] khác 0, reset lại cờ
                        hasClearedError = false;
                    }
                    if (READ_LAMP_RESET)
                    {
                        this.ClearError();
                    }


                }

            });

        }
        #region ALARM LOG
        private List<int> errorCodes;
        List<DateTime> timeerror = new List<DateTime>();
        private void InitializeErrorCodes()
        {
            errorCodes = new List<int>();
            timeerror = new List<DateTime>();

        }
       
        private void Number_Alarm()
        {
            int NumberAlarm = errorCodes.Count;
            this.CbShow.Content = NumberAlarm > 0 ? $"Errors : {NumberAlarm}" : "Not Show";
        }
        private void AlarmCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isAlarmWindowOpen = true;
        }
        private void AlarmCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isAlarmWindowOpen = false;
        }
        private bool isAlarmWindowOpen = false;
        private void ShowAlarm()
        {
            WndAlarm wndAlarm = new WndAlarm();
            wndAlarm.UpdateErrorList(errorCodes);
            wndAlarm.UpdateTimeList(timeerror);
            if (!isAlarmWindowOpen)
            {
                wndAlarm.Show();
            }
        }
        private void AddError(short errorCode)
        {
            Dispatcher.Invoke(() =>
            {
                if (errorCode == 0)
                {
                    return;
                }

                if (errorCodes.Contains(errorCode))
                {
                    return;
                }

                else if (errorCodes.Count >= 31)
                {
                    errorCodes.Add(1);
                    return;
                }

                //Thêm lỗi vào SQL
                if (errorCode <= 100)
                {
                    string mes = AlarmInfo.getMessage(errorCode);
                    string sol = AlarmList.GetSolution(errorCode);
                    var alarm = new AlarmInfo(errorCode, mes, sol);
                    DbWrite.createAlarm(alarm);
                }
                else
                {
                    string mes = AlarmList.GetMes(errorCode);
                    string sol = AlarmList.GetSolution(errorCode);
                    var alarm = new AlarmInfo(errorCode, mes, sol);
                    DbWrite.createAlarm(alarm);
                }
                errorCodes.Add(errorCode);
                timeerror.Add(DateTime.Now);
                for (int i = 0; i < errorCodes.Count; i++)

                {
                    int code = errorCodes[i];
                    switch (i)
                    {
                        case 0: this.DisplayAlarm(1, code); break;
                        case 1: this.DisplayAlarm(2, code); break;
                        case 2: this.DisplayAlarm(3, code); break;
                        case 3: this.DisplayAlarm(4, code); break;
                        case 4: this.DisplayAlarm(5, code); break;
                        case 5: this.DisplayAlarm(6, code); break;
                        case 6: this.DisplayAlarm(7, code); break;
                        case 7: this.DisplayAlarm(8, code); break;
                        case 8: this.DisplayAlarm(9, code); break;
                        case 9: this.DisplayAlarm(10, code); break;
                        case 10: this.DisplayAlarm(11, code); break;
                        case 11: this.DisplayAlarm(12, code); break;
                        case 12: this.DisplayAlarm(13, code); break;
                        case 13: this.DisplayAlarm(14, code); break;
                        case 14: this.DisplayAlarm(15, code); break;
                        case 15: this.DisplayAlarm(16, code); break;
                        case 16: this.DisplayAlarm(17, code); break;
                        case 17: this.DisplayAlarm(18, code); break;
                        case 18: this.DisplayAlarm(19, code); break;
                        case 19: this.DisplayAlarm(20, code); break;
                        case 20: this.DisplayAlarm(21, code); break;
                        case 21: this.DisplayAlarm(22, code); break;
                        case 22: this.DisplayAlarm(23, code); break;
                        case 23: this.DisplayAlarm(24, code); break;
                        case 24: this.DisplayAlarm(25, code); break;
                        case 25: this.DisplayAlarm(26, code); break;
                        case 26: this.DisplayAlarm(27, code); break;
                        case 27: this.DisplayAlarm(28, code); break;
                        case 28: this.DisplayAlarm(29, code); break;
                        case 29: this.DisplayAlarm(30, code); break;

                        default:
                            break;
                    }
                }
                if (!isAlarmWindowOpen)
                {
                    this.ShowAlarm();
                }

                this.Number_Alarm();
            });


        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (errorCodes == null)
                return;
            if (errorCodes.Count >= 1)
            {
                WndAlarm wndAlarm = new WndAlarm();
                wndAlarm.UpdateErrorList(errorCodes);
                wndAlarm.UpdateTimeList(timeerror);
                wndAlarm.Show();
            }

        }
        public void ClearError()
        {
            timeerror.Clear();
            errorCodes.Clear();
            Dispatcher.Invoke(new Action(() =>
            {
                for (int i = 1; i <= 30; i++)
                {
                    var label = (Label)FindName("lbMes" + i);
                    label.Content = "";
                    label.Background = Brushes.Black;
                }
            }));

            WndAlarm wndAlarm = new WndAlarm();
            wndAlarm.UpdateErrorList(errorCodes);
            wndAlarm.UpdateTimeList(timeerror);
            this.Number_Alarm();
        }
        private void DisplayAlarm(int index, int code)
        {
            try
            {
                if (code <= 100)
                {
                    Label label = (Label)FindName($"lbMes{index}");
                    if (label != null)
                    {
                        string mes = AlarmInfo.getMessage(code);
                        this.Dispatcher.Invoke(() =>
                        {
                            DateTime currentTime = DateTime.Now;
                            string currentTimeString = currentTime.ToString();
                            string newContent = currentTime.ToString() + " : " + mes;

                            label.Content = newContent;
                            label.Background = Brushes.Red;
                            //label.FontWeight = FontWeights.ExtraBold;
                            //label.Foreground = Brushes.Black;
                        });
                    }
                }
                else
                {
                    Label label = (Label)FindName($"lbMes{index}");
                    if (label != null)
                    {
                        string mes = AlarmList.GetMes(code);
                        this.Dispatcher.Invoke(() =>
                        {
                            string currentTime = DateTime.Now.ToString("HH:mm:ss");
                            string currentTimeString = currentTime.ToString();
                            string newContent = currentTime.ToString() + " : " + mes;

                            label.Content = newContent;
                            label.Background = Brushes.Red;
                            //label.FontWeight = FontWeights.ExtraBold;
                            //label.Foreground = Brushes.Black;
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Create($"DisplayAlarm PgMain: {ex.Message}");
            }
        }
        #endregion

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ProcessVisionVidiCH1(out int a, out string b);
        //}
    }
}
