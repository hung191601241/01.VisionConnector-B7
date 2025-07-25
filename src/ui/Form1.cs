using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Text;
using DeviceSource;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace VisionInspection
{
    partial class Form1 : Form
    {

        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        CameraOperator m_pOperator;
        bool m_bGrabbing;

        UInt32 m_nBufSizeForDriver = 3072 * 2048 * 3;
        byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];            // Buffer for getting image from driver

        UInt32 m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
        byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];         // Buffer for saving image

        public Form1()
        {
            InitializeComponent();
            m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            m_pOperator = new CameraOperator();
            m_bGrabbing = false;
            DeviceListAcq();
            //this.Text = String.Format("About {0}", AssemblyTitle);
            this.Text = String.Format("Hik Camera Viewer", AssemblyTitle);
        }

        private void bnEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
        }

        private void DeviceListAcq()
        {
            int nRet;
            /*Create Device List*/
            System.GC.Collect();
            cbDeviceList.Items.Clear();
            nRet = CameraOperator.EnumDevices(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                MessageBox.Show("Enumerate devices fail!");
                return;
            }

            //Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                }
            }

            //Select the first item
            if (m_pDeviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void SetCtrlWhenOpen()
        {
            bnOpen.Enabled = false;

            bnClose.Enabled = true;
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;
            bnContinuesMode.Enabled = true;
            bnContinuesMode.Checked = true;
            bnTriggerMode.Enabled = true;
            cbSoftTrigger.Enabled = false;
            bnTriggerExec.Enabled = false;

            tbExposure.Enabled = true;
            tbGain.Enabled = true;
            tbFrameRate.Enabled = true;
            bnGetParam.Enabled = true;
            bnSetParam.Enabled = true;

        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            if (m_pDeviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
            {
                MessageBox.Show("No device, please select");
                return;
            }
            int nRet = -1;
            //Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device =
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure((IntPtr)(Convert.ToInt64(m_pDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex].ToString())),
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));
            MessageBox.Show(m_pDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex].ToString());
            //Open device
            nRet = m_pOperator.Open(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Device open fail!");
                return;
            }

            //Set Continues Aquisition Mode
            m_pOperator.SetEnumValue("AcquisitionMode", 2);
            m_pOperator.SetEnumValue("TriggerMode", 0);

            bnGetParam_Click(null, null);//Get parameters
            //Control operation
            SetCtrlWhenOpen();

        }

        private void SetCtrlWhenClose()
        {
            bnOpen.Enabled = true;

            bnClose.Enabled = false;
            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = false;
            bnContinuesMode.Enabled = false;
            bnTriggerMode.Enabled = false;
            cbSoftTrigger.Enabled = false;
            bnTriggerExec.Enabled = false;

            bnSaveBmp.Enabled = false;
            bnSaveJpg.Enabled = false;
            tbExposure.Enabled = false;
            tbGain.Enabled = false;
            tbFrameRate.Enabled = false;
            bnGetParam.Enabled = false;
            bnSetParam.Enabled = false;

        }

        private void bnClose_Click(object sender, EventArgs e)
        {

            //Close Device
            m_pOperator.Close();

            //Control Operation
            SetCtrlWhenClose();

            //Reset flow flag bit
            m_bGrabbing = false;
        }

        private void bnContinuesMode_CheckedChanged(object sender, EventArgs e)
        {
            if (bnContinuesMode.Checked)
            {
                m_pOperator.SetEnumValue("TriggerMode", 0);
                cbSoftTrigger.Enabled = false;
                bnTriggerExec.Enabled = false;
            }

        }

        private void bnTriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            //Open Trigger Mode
            if (bnTriggerMode.Checked)
            {
                m_pOperator.SetEnumValue("TriggerMode", 1);

                //Trigger source select:0 - Line0;
                //                      1 - Line1;
                //                      2 - Line2;
                //                      3 - Line3;
                //                      4 - Counter;
                //                      7 - Software;
                if (cbSoftTrigger.Checked)
                {
                    m_pOperator.SetEnumValue("TriggerSource", 7);
                    if (m_bGrabbing)
                    {
                        bnTriggerExec.Enabled = true;
                    }
                }
                else
                {
                    m_pOperator.SetEnumValue("TriggerSource", 0);
                }
                cbSoftTrigger.Enabled = true;
            }

        }

        private void SetCtrlWhenStartGrab()
        {


            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = true;

            if (bnTriggerMode.Checked && cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
            }

            bnSaveBmp.Enabled = true;
            bnSaveJpg.Enabled = true;
        }

        private void bnStartGrab_Click(object sender, EventArgs e)
        {
            int nRet;

            //Start Grabbing
            nRet = m_pOperator.StartGrabbing();
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Start Grabbing Fail!");
                return;
            }

            //Control Operation
            SetCtrlWhenStartGrab();

            //Set position bit true
            m_bGrabbing = true;


            //Display
            nRet = m_pOperator.Display(pictureBox1.Handle);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Display Fail!");
            }
        }

        private void cbSoftTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSoftTrigger.Checked)
            {

                //Set trigger source as Software
                m_pOperator.SetEnumValue("TriggerSource", 7);
                if (m_bGrabbing)
                {
                    bnTriggerExec.Enabled = true;
                }
            }
            else
            {
                m_pOperator.SetEnumValue("TriggerSource", 0);
                bnTriggerExec.Enabled = false;
            }
        }

        private void bnTriggerExec_Click(object sender, EventArgs e)
        {
            int nRet;

            //Trigger command
            nRet = m_pOperator.CommandExecute("TriggerSoftware");
            if (CameraOperator.CO_OK != nRet)
            {
                MessageBox.Show("Trigger Fail!");
            }
        }

        private void SetCtrlWhenStopGrab()
        {
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;

            bnTriggerExec.Enabled = false;


            bnSaveBmp.Enabled = false;
            bnSaveJpg.Enabled = false;
        }
        private void bnStopGrab_Click(object sender, EventArgs e)
        {
            int nRet = -1;
            //Stop Grabbing
            nRet = m_pOperator.StopGrabbing();
            if (nRet != CameraOperator.CO_OK)
            {
                MessageBox.Show("Stop Grabbing Fail!");
            }

            //Set flag bit false
            m_bGrabbing = false;

            //Control Operation
            SetCtrlWhenStopGrab();

        }

        private void bnSaveBmp_Click(object sender, EventArgs e)
        {
            int nRet;
            UInt32 nPayloadSize = 0;
            nRet = m_pOperator.GetIntValue("PayloadSize", ref nPayloadSize);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get PayloadSize failed");
                return;
            }
            if (nPayloadSize + 2048 > m_nBufSizeForDriver)
            {
                m_nBufSizeForDriver = nPayloadSize + 2048;
                m_pBufForDriver = new byte[m_nBufSizeForDriver];

                // Determine the buffer size to save image
                // BMP image size: width * height * 3 + 2048 (Reserved for BMP header)
                m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
            }

            IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
            UInt32 nDataLen = 0;
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

            //Get one frame timeout, timeout is 1 sec
            nRet = m_pOperator.GetOneFrameTimeout(pData, ref nDataLen, m_nBufSizeForDriver, ref stFrameInfo, 1000);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("No Data!");
                return;
            }

            /************************Mono8 to Bitmap*******************************
            Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pData);

            ColorPalette cp = bmp.Palette;
            // init palette
            for (int i = 0; i < 256; i++)
            {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }
            // set palette back
            bmp.Palette = cp;

            bmp.Save("D:\\test.bmp", ImageFormat.Bmp);

            *********************RGB8 to Bitmap**************************
            for (int i = 0; i < stFrameInfo.nHeight; i++ )
            {
                for (int j = 0; j < stFrameInfo.nWidth; j++)
                {
                    byte chRed = m_buffer[i * stFrameInfo.nWidth * 3 + j * 3];
                    m_buffer[i * stFrameInfo.nWidth * 3 + j * 3] = m_buffer[i * stFrameInfo.nWidth * 3 + j * 3 + 2];
                    m_buffer[i * stFrameInfo.nWidth * 3 + j * 3 + 2] = chRed;
                }
            }
            Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pData);
            bmp.Save("D:\\test.bmp", ImageFormat.Bmp);

            ************************************************************************/

            IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
            stSaveParam.enPixelType = stFrameInfo.enPixelType;
            stSaveParam.pData = pData;
            stSaveParam.nDataLen = stFrameInfo.nFrameLen;
            stSaveParam.nHeight = stFrameInfo.nHeight;
            stSaveParam.nWidth = stFrameInfo.nWidth;
            stSaveParam.pImageBuffer = pImage;
            stSaveParam.nBufferSize = m_nBufSizeForSaveImage;
            stSaveParam.nJpgQuality = 80;
            nRet = m_pOperator.SaveImage(ref stSaveParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Save Fail!");
                return;
            }

            FileStream file = new FileStream("image.bmp", FileMode.Create, FileAccess.Write);
            file.Write(m_pBufForSaveImage, 0, (int)stSaveParam.nImageLen);
            file.Close();

            MessageBox.Show("Save Succeed!");
        }

        private void bnSaveJpg_Click(object sender, EventArgs e)
        {
            int nRet;
            UInt32 nPayloadSize = 0;
            nRet = m_pOperator.GetIntValue("PayloadSize", ref nPayloadSize);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get PayloadSize failed");
                return;
            }
            if (nPayloadSize + 2048 > m_nBufSizeForDriver)
            {
                m_nBufSizeForDriver = nPayloadSize + 2048;
                m_pBufForDriver = new byte[m_nBufSizeForDriver];

                // Determine the buffer size to save image
                // BMP image size: width * height * 3 + 2048 (Reserved for BMP header)
                m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
            }

            IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
            UInt32 nDataLen = 0;
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

            //Get one frame timeout, timeout is 1 sec
            nRet = m_pOperator.GetOneFrameTimeout(pData, ref nDataLen, m_nBufSizeForDriver, ref stFrameInfo, 1000);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("No Data!");
                return;
            }

            IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Jpeg;
            stSaveParam.enPixelType = stFrameInfo.enPixelType;
            stSaveParam.pData = pData;
            stSaveParam.nDataLen = nDataLen;
            stSaveParam.nHeight = stFrameInfo.nHeight;
            stSaveParam.nWidth = stFrameInfo.nWidth;
            stSaveParam.pImageBuffer = pImage;
            stSaveParam.nBufferSize = m_nBufSizeForSaveImage;
            stSaveParam.nJpgQuality = 80;
            nRet = m_pOperator.SaveImage(ref stSaveParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Save Fail!");
                return;
            }

            FileStream file = new FileStream("image.jpg", FileMode.Create, FileAccess.Write);
            file.Write(m_pBufForSaveImage, 0, (int)stSaveParam.nImageLen);
            file.Close();

            MessageBox.Show("Save Succeed!");
        }

        private void bnGetParam_Click(object sender, EventArgs e)
        {
            float fExposure = 0;
            m_pOperator.GetFloatValue("ExposureTime", ref fExposure);
            tbExposure.Text = fExposure.ToString("F1");

            float fGain = 0;
            m_pOperator.GetFloatValue("Gain", ref fGain);
            tbGain.Text = fGain.ToString("F1");

            float fFrameRate = 0;
            m_pOperator.GetFloatValue("ResultingFrameRate", ref fFrameRate);
            tbFrameRate.Text = fFrameRate.ToString("F1");
        }

        private void bnSetParam_Click(object sender, EventArgs e)
        {
            int nRet;
            m_pOperator.SetEnumValue("ExposureAuto", 0);

            try
            {
                float.Parse(tbExposure.Text);
                float.Parse(tbGain.Text);
                float.Parse(tbFrameRate.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct type!");
                return;
            }

            nRet = m_pOperator.SetFloatValue("ExposureTime", float.Parse(tbExposure.Text));
            if (nRet != CameraOperator.CO_OK)
            {
                MessageBox.Show("Set Exposure Time Fail!");
            }

            m_pOperator.SetEnumValue("GainAuto", 0);
            nRet = m_pOperator.SetFloatValue("Gain", float.Parse(tbGain.Text));
            if (nRet != CameraOperator.CO_OK)
            {
                MessageBox.Show("Set Gain Fail!");
            }

            nRet = m_pOperator.SetFloatValue("AcquisitionFrameRate", float.Parse(tbFrameRate.Text));
            if (nRet != CameraOperator.CO_OK)
            {
                MessageBox.Show("Set Frame Rate Fail!");
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
    }
}
