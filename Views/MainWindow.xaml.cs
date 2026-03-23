using ImageEditor.Models;
using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class MainWindow : Window
    {
        private Point? _lastBrushPoint;
        private bool _isMiddleDragging;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm?.ActiveTool == ToolType.Brush)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, img, vm);
                vm.BeginBrushStroke();
                vm.SaveState();
                vm.BrushStroke(imgPoint, null);
                _lastBrushPoint = imgPoint;
                img?.CaptureMouse();
                return;
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {           
            var vm = DataContext as MainViewModel;

            if (_isMiddleDragging && e.MiddleButton == MouseButtonState.Pressed)
            {
                vm?.DragTo(e.GetPosition(Application.Current.MainWindow));
                return;
            }

            if (vm?.ActiveTool == ToolType.Brush && e.LeftButton == MouseButtonState.Pressed)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, img, vm);
                vm.BrushStroke(imgPoint, _lastBrushPoint);
                _lastBrushPoint = imgPoint;
                e.Handled = true;
                return;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            _lastBrushPoint = null;
            if (!_isMiddleDragging)
                (sender as FrameworkElement)?.ReleaseMouseCapture();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.MouseWheelCommand.Execute(e);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                var vm = DataContext as MainViewModel;
                _isMiddleDragging = true;
                vm?.StartDrag(e.GetPosition(Application.Current.MainWindow));
                (sender as FrameworkElement)?.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released && _isMiddleDragging)
            {
                var vm = DataContext as MainViewModel;
                _isMiddleDragging = false;
                vm?.EndDrag();
                (sender as FrameworkElement)?.ReleaseMouseCapture();
            }
        }

        private Point GetImagePixel(MouseEventArgs e, Image img, MainViewModel vm)
        {
            var pos = e.GetPosition(img);

            var bitmap = (vm.SelectedTab?.Image);
            if (bitmap == null) return pos;

            double scaleX = bitmap.PixelWidth / img.ActualWidth;
            double scaleY = bitmap.PixelHeight / img.ActualHeight;

            return new Point(pos.X * scaleX, pos.Y * scaleY);
        }
    }
}