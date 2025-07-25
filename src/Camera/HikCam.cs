using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using DeviceSource;
using System.Windows.Interop;
using System.Windows;
using System.Drawing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;

namespace VisionInspection
{
    class HikCam
    {
        public enum AquisMode
        {
            AcquisitionMode,
            TriggerMode
        }
        MyCamera myCam;
        CameraOperator m_pOperator;
        public MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        public MyCamera.MV_CC_DEVICE_INFO device;
        bool m_bGrabbing;

        UInt32 m_nBufSizeForDriver = 3072 * 2048 * 3;
        byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];            // Buffer for getting image from driver

        UInt32 m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
        byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];         // Buffer for saving image
        public bool isOpen { get; set; }

        public HikCam()
        {
            InitializeCamera();
        }

        bool InitializeCamera()
        {
            myCam = new MyCamera();
            m_pOperator = new CameraOperator();
            m_bGrabbing = false;
            //DeviceListAcq();
            return true;
        }
        public void DeviceListAcq()
        {
            int nRet;
            /*Create Device List*/
            System.GC.Collect();
            nRet = CameraOperator.EnumDevices(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                return;
            }
        }
        public int Open(MyCamera.MV_CC_DEVICE_INFO device, AquisMode mode)
        {
            this.device = device;
            int ret = m_pOperator.Open(ref device);
            m_pOperator.SetEnumValue("AcquisitionMode", 2);
            m_pOperator.SetEnumValue("TriggerMode", 0);
            ret = StartGrab(device, AquisMode.AcquisitionMode);
            if (ret == MyCamera.MV_OK)
            {
                isOpen = true;
            }
            else
            {
                isOpen = false;
            }
            return ret;
        }
        public bool IsOpen()
        {
            return this.isOpen;
        }
        public int Close()
        {
            int ret = m_pOperator.Close();
            return ret;
        }

        public int DisPose()
        {
            int ret = m_pOperator.DisPose();
            return ret;
        }

        public int StartGrab(MyCamera.MV_CC_DEVICE_INFO device, AquisMode mode)
        {
            int nRet1;

            //Start Grabbing
            nRet1 = m_pOperator.StartGrabbing();
            if (MyCamera.MV_OK != nRet1)
            {
                return nRet1;
            }

            //nRet1 = m_pOperator.Display(img.Handle);

            //HwndSource hwndSource = PresentationSource.FromVisual(img) as HwndSource;

            //if (hwndSource != null)
            //{
            //    nRet1 = m_pOperator.Display(hwndSource.Handle);
            //}
            if (MyCamera.MV_OK != nRet1)
            {
                return nRet1;
            }
            return nRet1;
        }
        Mat srcImage;


        public Mat CaptureImage()
        {
            int nRet;
            UInt32 nPayloadSize = 0;
            nRet = m_pOperator.GetIntValue("PayloadSize", ref nPayloadSize);
            if (MyCamera.MV_OK != nRet)
            {
                //MessageBox.Show("Get PayloadSize failed");
                GC.Collect();
                return null;
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
                //MessageBox.Show("No Data!");
                GC.Collect( );
                return null;
            }

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
                //MessageBox.Show("Save Fail!");
                GC.Collect();
                return null;
            }
            //FileStream file = new FileStream("image.bmp", FileMode.Create, FileAccess.Write);
            //file.Write(m_pBufForSaveImage, 0, (int)stSaveParam.nImageLen);
            //file.Close();
            byte[] imageData = m_pBufForSaveImage;
            Bitmap bmp;
            using (var ms = new MemoryStream(imageData))
            {
                bmp = new Bitmap(ms);
            }
            Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            bmp.Dispose();
            GC.Collect();
            return src;
        }

        public Mat CaptureImageOld()
        {  //MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet;
            UInt32 nPayloadSize = 0;
            nRet = m_pOperator.GetIntValue("PayloadSize", ref nPayloadSize);
            if (MyCamera.MV_OK != nRet)
            {
                return null;
            }
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                m_nBufSizeForDriver = nPayloadSize;
                m_pBufForDriver = new byte[m_nBufSizeForDriver];
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
                //MessageBox.Show("No Data!");
                return null;
            }
            srcImage = new Mat(stFrameInfo.nHeight, stFrameInfo.nWidth, MatType.CV_8UC3, pData);
            return srcImage;
        }

        public bool SetExposeTime(int Exp)
        {
            int nRet = m_pOperator.SetFloatValue("ExposureTime", (float)Exp);
            if (nRet != CameraOperator.CO_OK)
            {
                return false;
            }
            return true;
        }
        public string GetserialNumber()
        {
            string SerialNumber = "";
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.device.SpecialInfo.stGigEInfo, 0);
                MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                SerialNumber = gigeInfo.chSerialNumber;

            }
            else if (this.device.nTLayerType == MyCamera.MV_USB_DEVICE)

            {
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.device.SpecialInfo.stUsb3VInfo, 0);
                MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                SerialNumber = usbInfo.chSerialNumber;


            }
            return SerialNumber;

        }
        public string GetserialNumber(MyCamera.MV_CC_DEVICE_INFO deviceT)
        {
            string SerialNumber = "";
            if (deviceT.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceT.SpecialInfo.stGigEInfo, 0);
                MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                SerialNumber = gigeInfo.chSerialNumber;

            }
            else if (deviceT.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceT.SpecialInfo.stUsb3VInfo, 0);
                MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                SerialNumber = usbInfo.chSerialNumber;

            }
            return SerialNumber;

        }

    }
}