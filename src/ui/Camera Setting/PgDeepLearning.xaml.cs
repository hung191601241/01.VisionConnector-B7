using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViDi2;
using ViDi2.Runtime;
using ViDi2.UI;
using VisionInspection;

namespace AutoLaserCuttingInput
{
    /// <summary>
    /// Interaction logic for PgDeepLearning.xaml
    /// </summary>
    public partial class PgDeepLearning : Page
    {
        MyLogger logger = new MyLogger("PgMainDeep");
        public PgDeepLearning()
        {
            InitializeComponent();
            //var control = new ViDi2.Runtime.Local.Control(ViDi2.GpuMode.Deferred);
            //control.InitializeComputeDevices(ViDi2.GpuMode.SingleDevicePerTool, new List<int>() { });
            //this.control = control;
            //DataContext = this;

            sampleViewer.DragEnter += CheckDrop;
            sampleViewer.DragOver += CheckDrop;
            sampleViewer.Drop += DoDrop;
            sampleViewer.ToolSelected += sampleViewer_ToolSelected;
        }
        ViDi2.Runtime.IControl control;
        ViDi2.IWorkspace workspace;
        ViDi2.IStream stream;

        /// <summary>
        /// Gets or sets the main control providing access to the library
        /// </summary>
        /// 

        #region Workspace manual check 
        public ViDi2.Runtime.IControl Control
        {
            get { return control; }
            set
            {
                control = value;
                RaisePropertyChanged(nameof(Control));
                RaisePropertyChanged(nameof(Workspaces));
                RaisePropertyChanged(nameof(Stream));
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
                Stream = workspace.Streams.First();
                RaisePropertyChanged(nameof(Workspace));
            }
        }

        /// <summary>
        /// Gets or sets the current stream
        /// </summary>
        public ViDi2.IStream Stream
        {
            get { return stream; }
            set
            {
                stream = value;
                sampleViewer.Sample = null;
                RaisePropertyChanged(nameof(Stream));
            }
        }

        public ISampleViewerViewModel SampleViewerViewModel => sampleViewer.ViewModel;

        void sampleViewer_ToolSelected(ViDi2.ITool tool)
        {
            RaisePropertyChanged(nameof(SampleViewer));
        }

        void CheckDrop(object sender, DragEventArgs e)
        {
            var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            bool isArchive = System.IO.Path.GetExtension(lst.First()) == ".vsa";

            e.Effects = stream != null || isArchive ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private void DoDrop(object sender, DragEventArgs e)
        {
            var lst = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);

            try
            {
                using (var image = new ViDi2.UI.WpfImage(lst.First()))
                    sampleViewer.Sample = Stream.Process(image);

                RaisePropertyChanged(nameof(ViewIndices));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to Load Image",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IRedMarking redMarking = sampleViewer.Sample.Markings[sampleViewer.ToolName] as IRedMarking;
            foreach (IRedView view in redMarking.Views)
            {
                MessageBox.Show($"This view has a score of {view.Score}");

            }
        }

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
                    Workspace = Control.Workspaces.Add(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName), fs);
                    Mouse.OverrideCursor = null;
                    Mouse.OverrideCursor = null;
                }
            }
            RaisePropertyChanged(nameof(Workspaces));
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

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
