using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;
using System.Xml.Linq;
using System.Windows.Media.Media3D;
using AutoLaserCuttingInput.src.MyCanvas;

namespace VisionInspection
{
    public class ShapeEditor : Grid
    {
        #region Fields & props 

        public Rectangle rLT, rCT, rRT, rLC, rRC, rLB, rCB, rRB, rCover;
        public CircleArrow cirArrow;
        double rectSize = 13;
        Label lb = new Label();
        public bool moving = false, resizing = false, rotating = false;
        System.Windows.Point mysPosStart, objPos, objSizeStart;
        //RotateTransform rotTrans = new RotateTransform(0);
        private double preAngle = 0;
        public KeyValuePair<int, double> rotList = new KeyValuePair<int, double>();
        private System.Windows.Point rectCenter = new System.Windows.Point(0, 0);
        private System.Windows.Point rectTC = new System.Windows.Point(0, 0);

        public FrameworkElement CapturedElement { get; private set; }

        #endregion

        #region Constructor

        public ShapeEditor()
        {
            rectSize = (double)UiManager.appSettings.Property.rectSize.Width;
        }

        // Method for creating small rectangles at the corners and sides
        private Rectangle AddRect(string name, HorizontalAlignment ha, VerticalAlignment va, Cursor crs)
        {


            var rect = new Rectangle()   // small rectangles at the corners and sides
            {
                HorizontalAlignment = ha,
                VerticalAlignment = va,
                Width = rectSize,
                Height = rectSize,
                Stroke = new SolidColorBrush(Colors.Aqua), // small rectangles color
                StrokeThickness = 9,
                Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255)),
                Cursor = crs,
                Name = name,
            };

            rect.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
            rect.MouseLeftButtonUp += Rect_MouseLeftButtonUp;
            rect.MouseMove += Rect_MouseMove;
            Children.Add(rect);


            //Children.Add(label);
            return rect;
        }
        private CircleArrow AddCirArrow()
        {
            var arrow = new CircleArrow()
            {
                Name = "cirArrow",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -80, 0, 0),
                Width = rectSize + 10,
                Height = rectSize + 10,
                BorderBrush = new SolidColorBrush(Colors.Aqua), // small rectangles color
                BorderThickness = new Thickness(9)
            };
            arrow.MouseLeftButtonDown += Arrow_MouseLeftButtonDown;
            arrow.MouseLeftButtonUp += Arrow_MouseLeftButtonUp;
            arrow.MouseMove += Arrow_MouseMove;
            Children.Add(arrow);

            var line = new Line()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -30, 0, 0),
                X1 = 0, Y1 = 0,
                X2 = 0, Y2 = 35,
                Fill = new SolidColorBrush(Colors.Aqua),
                Stroke = new SolidColorBrush(Colors.Aqua),
                StrokeThickness = 5,
                StrokeDashArray = new DoubleCollection() {1, 0.6}
            };
            Children.Add(line);

            return arrow;
        }

        private void Arrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rotating = true;
            var cirArr = sender as FrameworkElement;
            mysPosStart = e.GetPosition(this.Parent as Canvas);
            objPos = new System.Windows.Point(Canvas.GetLeft(this), Canvas.GetTop(this));

            // Lấy tâm hình chữ nhật trong tọa độ của Canvas
            rectCenter = this.TransformToAncestor(this.Parent as Canvas).Transform(new System.Windows.Point(ActualWidth / 2, ActualHeight / 2));
            // Lấy tọa độ Top-Center trong hệ tọa độ Canvas
            rectTC.X = rectCenter.X;
            rectTC.Y = 0;

            cirArr.CaptureMouse(); // Bắt sự kiện chuột
            cirArr.Cursor = Cursors.Cross;
            e.Handled = true;
        }

        private void Arrow_MouseMove(object sender, MouseEventArgs e)
        {
            if (rotating)
            {
                // Lấy tọa độ chuột
                mysPosStart = e.GetPosition(this.Parent as Canvas);

                // Vector từ tâm đến Top-Center
                double dx1 = rectTC.X - rectCenter.X;
                double dy1 = rectTC.Y - rectCenter.Y;
                // Vector từ tâm đến chuột
                double dx2 = mysPosStart.X - rectCenter.X;
                double dy2 = mysPosStart.Y - rectCenter.Y;
                // Tính góc bằng atan2 và đổi giá trị góc từ rad sang deg
                RotateTransform rotateTransform = new RotateTransform(0);
                rotateTransform.Angle = (Math.Atan2(dy2, dx2) - Math.Atan2(dy1, dx1)) * (180 / Math.PI);
                this.RenderTransform = rotateTransform;
            }    
        }

        private void Arrow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rotating = false;
            var cirArr = sender as FrameworkElement;
            cirArr.ReleaseMouseCapture();
            cirArr.Cursor = Cursors.Arrow;
        }

        void enableImage(System.Windows.Controls.Image img, String path)
        {

            var folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(folder);
            bitmap.EndInit();
            img.Source = bitmap;
        }

        #endregion

        #region Click on the shape - moving

        private void RCover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            moving = true;
            mysPosStart = e.GetPosition(this.Parent as Canvas);
            objPos = new System.Windows.Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            objSizeStart = new System.Windows.Point(rCover.ActualWidth, rCover.ActualHeight);
            rCover.CaptureMouse();
            rCover.Cursor = Cursors.SizeAll;
            e.Handled = true;
        }

        private void RCover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
            rCover.ReleaseMouseCapture();
            rCover.Cursor = Cursors.Arrow;
        }

        private void RCover_MouseMove(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                var mysPosTed = e.GetPosition(this.Parent as Canvas);
                double x = mysPosTed.X - mysPosStart.X + objPos.X;
                Canvas.SetLeft(this, x);
                double y = mysPosTed.Y - mysPosStart.Y + objPos.Y;
                Canvas.SetTop(this, y);
            }
        }

        #endregion

        #region Click on the conner or side of the shape - resizing

        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            resizing = true;
            var rec = sender as FrameworkElement;
            mysPosStart = e.GetPosition(this.Parent as Canvas);
            objPos = new System.Windows.Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            objSizeStart = new System.Windows.Point(this.ActualWidth, this.ActualHeight);
            rec.CaptureMouse();
            e.Handled = true;
        }

        private void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resizing = false;
            var rec = sender as FrameworkElement;
            rec.ReleaseMouseCapture();
        }

        private void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (resizing)
            {
                var mysPosTed = e.GetPosition(this.Parent as FrameworkElement);
                var prvek = sender as FrameworkElement;
                if (prvek.Name[1] != 'C')
                {
                    double width;
                    if (prvek.Name[1] == 'L')
                    {
                        width = mysPosStart.X - mysPosTed.X + objSizeStart.X;
                        if (width < 55)
                            return;
                        Canvas.SetLeft(this, Canvas.GetLeft(this) - (width - Width));
                    }
                    else
                    {
                        width = mysPosTed.X - mysPosStart.X + objSizeStart.X;
                        if (width < 55)
                            return;
                    }

                    Width = Math.Max(width, 0);
                }
                if (prvek.Name[2] != 'C')
                {
                    double height;
                    if (prvek.Name[2] == 'T')
                    {
                        height = mysPosStart.Y - mysPosTed.Y + objSizeStart.Y;
                        if (height < 55)
                            return;
                        Canvas.SetTop(this, Canvas.GetTop(this) - (height - Height));
                    }
                    else
                    {
                        height = mysPosTed.Y - mysPosStart.Y + objSizeStart.Y;
                        if (height < 55)
                            return;
                    }

                    Height = Math.Max(height, 0);
                }
            }
        }

        #endregion

        #region Editing shape

        // Start editing a shape (element)


        public void CaptureElement(FrameworkElement element, MouseButtonEventArgs mouse = null)
        {
            if (CapturedElement != null)
            {
                if (CapturedElement == element)
                    return;
                ReleaseElement();
            }


            for (int i = 0; i < ((Canvas)Parent).Children.Count; i++)
            {
                Label a = ((Canvas)Parent).Children[i] as Label;
                {
                    if (a != null && a.Name == element.Name)
                    {
                        ((Canvas)Parent).Children.RemoveAt(i);
                    }
                }
            }

            if (!Children.Contains(lb))
            {
                lb = new Label();
                lb.Name = element.Name;
                lb.Content = element.Name.Replace("R", String.Empty);
                lb.FontSize = (double)UiManager.appSettings.Property.labelFontSize;
                lb.Foreground = System.Windows.Media.Brushes.Aqua;
                lb.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                Canvas.SetLeft(lb, Canvas.GetLeft(element) - rectSize / 2.0);
                Canvas.SetTop(lb, Canvas.GetTop(element) - rectSize / 2.0 - 100);
                Children.Add(lb);
                //Children.Add(lb);
            }


            
            bool existsName = Children
              .OfType<Rectangle>() // Lọc chỉ lấy các đối tượng có thể có Name
              .Any(child => child.Name == element.Name);

            Visibility = Visibility.Collapsed;
            rCover = new Rectangle()   // almost transparent cover rectangle for catching clicks
            {
                Name = element.Name,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(rectSize / 2.0),
                Fill = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255)),
                RenderTransform = new RotateTransform(0),
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5)
            };
            rCover.MouseLeftButtonDown += RCover_MouseLeftButtonDown;
            rCover.MouseLeftButtonUp += RCover_MouseLeftButtonUp;
            rCover.MouseRightButtonDown += RCover_MouseRightButtonDown;
            rCover.MouseMove += RCover_MouseMove;
            Children.Add(rCover);

            rLT = AddRect("rLT", HorizontalAlignment.Left, VerticalAlignment.Top, Cursors.SizeNWSE);
            rCT = AddRect("rCT", HorizontalAlignment.Center, VerticalAlignment.Top, Cursors.SizeNS);
            rRT = AddRect("rRT", HorizontalAlignment.Right, VerticalAlignment.Top, Cursors.SizeNESW);
            rLC = AddRect("rLC", HorizontalAlignment.Left, VerticalAlignment.Center, Cursors.SizeWE);
            rRC = AddRect("rRC", HorizontalAlignment.Right, VerticalAlignment.Center, Cursors.SizeWE);
            rLB = AddRect("rLB", HorizontalAlignment.Left, VerticalAlignment.Bottom, Cursors.SizeNESW);
            rCB = AddRect("rCB", HorizontalAlignment.Center, VerticalAlignment.Bottom, Cursors.SizeNS);
            rRB = AddRect("rRB", HorizontalAlignment.Right, VerticalAlignment.Bottom, Cursors.SizeNWSE);
            cirArrow = AddCirArrow();
            

            CapturedElement = element;
            Canvas.SetLeft(this, Canvas.GetLeft(element) - rectSize / 2.0);
            Canvas.SetTop(this, Canvas.GetTop(element) - rectSize / 2.0);
            Width = element.Width + rectSize;
            Height = element.Height + rectSize;
            //RenderTransform = rotTrans;
            RenderTransformOrigin = new System.Windows.Point(0.5, 0.5); // Xoay từ trung tâm

            ((Canvas)element.Parent).Children.Remove(element);
            Children.Insert(0, element);
            element.Margin = new Thickness(rectSize / 2.0);
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.VerticalAlignment = VerticalAlignment.Stretch;
            element.Width = double.NaN;
            element.Height = double.NaN;
            element.RenderTransform = new RotateTransform(0);
            element.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);


            Visibility = Visibility.Visible;
            if (mouse != null)
                RCover_MouseLeftButtonDown(rCover, mouse);
        }

        private void RCover_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRegion") as ContextMenu;
            cm.PlacementTarget = sender as ContextMenu;
            cm.IsOpen = true;
        }



        // End editing shape
        public void ReleaseElement()
        {
            if (CapturedElement == null) return;
            FrameworkElement element = CapturedElement;
            Children.Remove(element);
            ((Canvas)Parent).Children.Add(element);
            Children.Remove(lb);
            ((Canvas)Parent).Children.Add(lb);
            Canvas.SetLeft(element, Canvas.GetLeft(this) + rectSize / 2.0);
            Canvas.SetTop(element, Canvas.GetTop(this) + rectSize / 2.0);
            Canvas.SetLeft(lb, Canvas.GetLeft(element) + element.ActualWidth / 2.0);
            Canvas.SetTop(lb, Canvas.GetTop(element) + element.ActualHeight / 2.0);
            lb.RenderTransform = RenderTransform;
            lb.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            element.Width = Width - rectSize;
            element.Height = Height - rectSize;
            element.Margin = new Thickness(0);
            element.RenderTransform = RenderTransform;
            element.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            CapturedElement = null;
            Visibility = Visibility.Collapsed;
        }

        #endregion


        #region Delete Element
        public void DeleteElement(FrameworkElement element)
        {
            ReleaseElement();
            for (int i = 0; i < ((Canvas)Parent).Children.Count; i++)
            {
                Rectangle a = ((Canvas)Parent).Children[i] as Rectangle;
                {
                    if (a != null && a.Name == element.Name)
                    {
                        ((Canvas)Parent).Children.RemoveAt(i);
                    }
                }
            }

        }

        #endregion
    }
}
