using ImageEditor.Models;
using ImageEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageEditor.Views
{
    public partial class MainWindow : Window
    {
        private Point? _lastBrushPoint;
        private bool _isMiddleDragging;

        private Point? _selectionStart;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(MainViewModel.Zoom) ||
                            args.PropertyName == nameof(MainViewModel.ImageOffsetX) ||
                            args.PropertyName == nameof(MainViewModel.ImageOffsetY) ||
                            args.PropertyName == nameof(MainViewModel.Selection))
                        {
                            UpdateSelectionOverlay(vm);
                        }
                    };
                }
            };
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

            if (vm?.ActiveTool == ToolType.Selection)
            {
                var img = sender as Image;
                _selectionStart = GetImagePixel(e, img, vm);
                vm.Selection = null;
                UpdateSelectionOverlay(vm);
                (sender as FrameworkElement)?.CaptureMouse();
                e.Handled = true;
                return;
            }

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

            if (vm?.ActiveTool == ToolType.Selection && e.LeftButton == MouseButtonState.Pressed && _selectionStart.HasValue)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, img, vm);

                int x = (int)Math.Min(_selectionStart.Value.X, current.X);
                int y = (int)Math.Min(_selectionStart.Value.Y, current.Y);
                int w = (int)Math.Abs(current.X - _selectionStart.Value.X);
                int h = (int)Math.Abs(current.Y - _selectionStart.Value.Y);

                if (w > 0 &&h > 0)
                {
                    vm.Selection = new Int32Rect(x, y, w, h);
                    UpdateSelectionOverlay(vm);
                }
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
            _selectionStart = null;
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

            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null || img.ActualWidth <= 0 || img.ActualHeight <= 0) return pos;

            double scaleX = bitmap.PixelWidth / img.ActualWidth;
            double scaleY = bitmap.PixelHeight / img.ActualHeight;

            return new Point(pos.X * scaleX, pos.Y * scaleY);
        }

        private void UpdateSelectionOverlay(MainViewModel vm)
        {
            var rect = FindSelectionRect();
            var img = FindVisualChild<Image>(this, "MainImage");
            var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");

            if (rect == null || img == null || canvas == null) return;

            if (vm.Selection.HasValue && img.ActualWidth > 0)
            {
                var s = vm.Selection.Value;
                var bitmap = vm.SelectedTab?.Image;
                if (bitmap == null) return;

                double dpiScaleX = bitmap.PixelWidth / img.ActualWidth;
                double dpiScaleY = bitmap.PixelHeight / img.ActualHeight;

                var topLeft = new Point(s.X / dpiScaleX, s.Y / dpiScaleY);
                var botRight = new Point((s.X + s.Width) / dpiScaleX, (s.Y + s.Height) / dpiScaleY);

                var transform = img.TransformToVisual(canvas);
                var canvasTopLeft = transform.Transform(topLeft);
                var canvasBotRight = transform.Transform(botRight);

                Canvas.SetLeft(rect, canvasTopLeft.X);
                Canvas.SetTop(rect, canvasTopLeft.Y);
                rect.Width = canvasBotRight.X - canvasTopLeft.X;
                rect.Height = canvasBotRight.Y - canvasTopLeft.Y;
                rect.Visibility = Visibility.Visible;
            }
            else
            {
                rect.Visibility = Visibility.Collapsed;
            }
        }

        private Rectangle FindSelectionRect()
        {
            return FindVisualChild<Rectangle>(this, "SelectionRect");
        }

        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name) return fe;
                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}