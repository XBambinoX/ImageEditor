using ImageEditor.Models;
using ImageEditor.Services;
using ImageEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageEditor.Views
{
    public partial class MainWindow : Window
    {
        private Point? _lastBrushPoint;
        private bool _isMiddleDragging;

        private Point? _selectionStart;

        private Point? _floatingDragStart;
        private int _floatingDragOriginX;
        private int _floatingDragOriginY;

        private Point? _linePreviewStart;
        private WriteableBitmap _linePreviewBackup; // For storing the image state before drawing the line preview

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
            var img = sender as Image;

            if (vm?.IsFloatingPaste == true)
            {
                var clickPoint = GetImagePixel(e, img, vm);

                if (vm.Selection.HasValue && RectContains(vm.Selection.Value, (int)clickPoint.X, (int)clickPoint.Y))
                {
                    _floatingDragStart = clickPoint;
                    _floatingDragOriginX = vm.PasteX;
                    _floatingDragOriginY = vm.PasteY;
                    (sender as FrameworkElement)?.CaptureMouse();
                    e.Handled = true;
                    return;
                }
                else
                {
                    vm.CommitFloatingPaste();
                    UpdateSelectionOverlay(vm);
                }
            }

            if (vm?.ActiveTool == ToolType.Selection)
            {
                _selectionStart = GetImagePixel(e, img, vm);
                vm.Selection = null;
                UpdateSelectionOverlay(vm);
                (sender as FrameworkElement)?.CaptureMouse();
                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Line)
            {
                var imgPoint = GetImagePixel(e, img, vm);
                vm.BeginLineSettings();

                if (!vm.IsLineBezierMode)
                {
                    vm.LineStart = imgPoint;
                    _linePreviewStart = imgPoint;
                    _linePreviewBackup = new WriteableBitmap(vm.SelectedTab.Image);
                    vm.SaveState();
                    (sender as FrameworkElement)?.CaptureMouse();
                }
                else
                {
                    if (!vm.LineStart.HasValue)
                    {
                        // First click — set the start point
                        vm.LineStart = imgPoint;
                        _linePreviewStart = imgPoint;
                        _linePreviewBackup = new WriteableBitmap(vm.SelectedTab.Image);
                        vm.SaveState();
                        (sender as FrameworkElement)?.CaptureMouse();
                    }
                    else if (!vm.IsBezierSecondPhase)
                    {
                        vm.LineEnd = imgPoint;
                        var s = vm.LineStart.Value;
                        var en = imgPoint;
                        vm.BezierControl1 = new Point(s.X + (en.X - s.X) / 3, s.Y + (en.Y - s.Y) / 3);
                        vm.BezierControl2 = new Point(s.X + 2 * (en.X - s.X) / 3, s.Y + 2 * (en.Y - s.Y) / 3);
                        vm.IsBezierSecondPhase = true;
                    }
                    else
                    {
                        // Third click — commit the line with the specified control points
                        vm.CommitLine(vm.LineStart.Value, vm.LineEnd.Value,
                                      vm.BezierControl1, vm.BezierControl2);
                        _linePreviewBackup = null;
                    }
                }

                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Brush)
            {
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

            // Floating paste drag
            if (vm?.IsFloatingPaste == true && _floatingDragStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, img, vm);

                int dx = (int)(current.X - _floatingDragStart.Value.X);
                int dy = (int)(current.Y - _floatingDragStart.Value.Y);

                vm.MoveFloatingPaste(_floatingDragOriginX + dx, _floatingDragOriginY + dy);
                UpdateSelectionOverlay(vm);
                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Line && _linePreviewStart.HasValue)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, img, vm);

                // Recover the image state before drawing the preview
                if (_linePreviewBackup != null && vm.SelectedTab != null)
                {
                    var preview = new WriteableBitmap(_linePreviewBackup);
                    vm.SelectedTab.Image = preview;

                    if (!vm.IsLineBezierMode && e.LeftButton == MouseButtonState.Pressed)
                    {
                        DrawingService.DrawLine(preview, _linePreviewStart.Value, current,
                                                        vm.LineWidth, vm.LineColor);
                    }
                    else if (vm.IsBezierSecondPhase && vm.BezierControl1.HasValue && vm.BezierControl2.HasValue)
                    {
                        var cp1 = vm.BezierControl1.Value;
                        var cp2 = vm.BezierControl2.Value;
                        double d1 = Math.Sqrt(Math.Pow(current.X - cp1.X, 2) + Math.Pow(current.Y - cp1.Y, 2));
                        double d2 = Math.Sqrt(Math.Pow(current.X - cp2.X, 2) + Math.Pow(current.Y - cp2.Y, 2));

                        if (d1 < d2)
                            vm.BezierControl1 = current;
                        else
                            vm.BezierControl2 = current;

                        DrawingService.DrawBezier(preview,
                            vm.LineStart.Value, vm.BezierControl1.Value,
                            vm.BezierControl2.Value, vm.LineEnd.Value,
                            vm.LineWidth, vm.LineColor);
                    }

                    vm.OnPropertyChangedPublic(nameof(vm.CurrentImage));
                }
                return;
            }

            if (vm?.ActiveTool == ToolType.Selection && e.LeftButton == MouseButtonState.Pressed && _selectionStart.HasValue)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, img, vm);
                var bitmap = vm.SelectedTab?.Image;

                int bmpW = bitmap?.PixelWidth ?? int.MaxValue;
                int bmpH = bitmap?.PixelHeight ?? int.MaxValue;

                double startX = Math.Max(0, Math.Min(_selectionStart.Value.X, bmpW));
                double startY = Math.Max(0, Math.Min(_selectionStart.Value.Y, bmpH));
                double endX = Math.Max(0, Math.Min(current.X, bmpW));
                double endY = Math.Max(0, Math.Min(current.Y, bmpH));

                int x = (int)Math.Min(startX, endX);
                int y = (int)Math.Min(startY, endY);
                int w = (int)Math.Abs(endX - startX);
                int h = (int)Math.Abs(endY - startY);

                if (w > 0 && h > 0)
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
            _floatingDragStart = null;

            var vm = DataContext as MainViewModel;
            _selectionStart = null;
            _lastBrushPoint = null;
            if (!_isMiddleDragging)
                (sender as FrameworkElement)?.ReleaseMouseCapture();

            if (vm?.ActiveTool == ToolType.Line && !vm.IsLineBezierMode && vm.LineStart.HasValue)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, img, vm);
                vm.CommitLine(vm.LineStart.Value, imgPoint);
                _linePreviewBackup = null;
                _linePreviewStart = null;
                (sender as FrameworkElement)?.ReleaseMouseCapture();
                return;
            }
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
            var rect = FindVisualChild<Rectangle>(this, "SelectionRect");
            var rectBlack = FindVisualChild<Rectangle>(this, "SelectionRectBlack");
            var rectFill = FindVisualChild<Rectangle>(this, "SelectionFill");
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

                var p1 = new Point(s.X / dpiScaleX, s.Y / dpiScaleY);
                var p2 = new Point((s.X + s.Width) / dpiScaleX, (s.Y + s.Height) / dpiScaleY);

                var transform = img.TransformToVisual(canvas);
                var c1 = transform.Transform(p1);
                var c2 = transform.Transform(p2);

                void ApplyRect(Rectangle r)
                {
                    Canvas.SetLeft(r, c1.X);
                    Canvas.SetTop(r, c1.Y);
                    r.Width = c2.X - c1.X;
                    r.Height = c2.Y - c1.Y;
                    r.Visibility = Visibility.Visible;
                }

                ApplyRect(rect);
                if (rectBlack != null) ApplyRect(rectBlack);
                if (rectFill != null) ApplyRect(rectFill);
            }
            else
            {
                rect.Visibility = Visibility.Collapsed;
                if (rectBlack != null) rectBlack.Visibility = Visibility.Collapsed;
                if (rectFill != null) rectFill.Visibility = Visibility.Collapsed;
            }
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

        private bool RectContains(Int32Rect r, int x, int y)
                      => x >= r.X && x <= r.X + r.Width && y >= r.Y && y <= r.Y + r.Height;
    }
}