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
        private Point? _dragStart;

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
                vm.SaveState();
                vm.BrushStroke(imgPoint, null);
                _lastBrushPoint = imgPoint;
                img?.CaptureMouse();
                e.Handled = true;
                return;
            }

            _dragStart = e.GetPosition(sender as FrameworkElement);
            (sender as FrameworkElement)?.CaptureMouse();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm?.ActiveTool == ToolType.Brush && e.LeftButton == MouseButtonState.Pressed)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, img, vm);
                vm.BrushStroke(imgPoint, _lastBrushPoint);
                _lastBrushPoint = imgPoint;
                e.Handled = true;
                return;
            }

            if (_dragStart.HasValue)
                vm?.DragTo(e.GetPosition(sender as FrameworkElement));
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            _lastBrushPoint = null;
            _dragStart = null;
            (sender as FrameworkElement)?.ReleaseMouseCapture();
            vm?.EndDrag();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.MouseWheelCommand.Execute(e);
        }

        private Point GetImagePixel(MouseEventArgs e, Image img, MainViewModel vm)
        {
            var pos = e.GetPosition(img);
            return new Point(pos.X , pos.Y);
        }
    }
}