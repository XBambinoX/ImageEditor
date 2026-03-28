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

        // For selection resizing
        private enum ResizeHandle { None, TL, TC, TR, ML, MR, BL, BC, BR }
        private ResizeHandle _activeHandle = ResizeHandle.None;
        private Point? _resizeDragStart;
        private Int32Rect _resizeOriginalRect;
        private const double HandleSize = 8;
        private const double HandleHitRadius = 10;

        private Color _eyedropperPreviewColor;

        private Point? _textPosition; // canvas coordinates
        private Point? _textImagePosition; // pixel coordinates

        private bool _isDraggingText;
        private Point? _textDragStart;
        private Point _textDragOriginCanvas;
        private Point _textDragOriginImage;

        private InputBindingCollection _savedBindings;

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

                        if (args.PropertyName == nameof(MainViewModel.ActiveTool))
                        {
                            UpdateSelectionOverlay(vm);
                            if (vm.ActiveTool != ToolType.Eyedropper)
                                HideEyedropperPreview();
                            else
                            {
                                var img2 = FindVisualChild<Image>(this, "MainImage");
                                var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");
                                var bitmap = vm.SelectedTab?.Image as WriteableBitmap;

                                if (img2 != null && canvas != null && bitmap != null)
                                {
                                    var imgPoint = GetImagePixelFromPoint(Mouse.GetPosition(img2), img2, vm);
                                    UpdateEyedropperPreview(imgPoint, Mouse.GetPosition(canvas), bitmap);
                                }
                            }
                        }

                        if (args.PropertyName == nameof(MainViewModel.ActiveColor))
                        {
                            UpdateTextOverlayStyle(vm);
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
            var img = FindVisualChild<Image>(this, "MainImage");

            if (vm?.IsFloatingPaste == true)
            {
                var clickPoint = GetImagePixel(e, vm);
                var canvasPoint = GetCanvasPoint(e, img, vm);

                if (vm.Selection.HasValue)
                {
                    var (sc1, sc2) = GetSelectionCanvasPoints(vm, img);
                    var hit = HitTestHandle(canvasPoint, sc1, sc2);

                    if (hit != ResizeHandle.None)
                    {
                        _activeHandle = hit;
                        _resizeDragStart = canvasPoint;
                        _resizeOriginalRect = vm.Selection.Value;
                        (sender as FrameworkElement)?.CaptureMouse();
                        e.Handled = true;
                        return;
                    }

                    if (RectContains(vm.Selection.Value, (int)clickPoint.X, (int)clickPoint.Y))
                    {
                        _floatingDragStart = clickPoint;
                        _floatingDragOriginX = vm.PasteX;
                        _floatingDragOriginY = vm.PasteY;
                        (sender as FrameworkElement)?.CaptureMouse();
                        e.Handled = true;
                        return;
                    }
                }

                vm.CommitFloatingPaste();
                UpdateSelectionOverlay(vm);
            }

            if (vm?.ActiveTool == ToolType.Selection)
            {
                _selectionStart = GetImagePixel(e, vm);
                vm.Selection = null;
                UpdateSelectionOverlay(vm);
                (sender as FrameworkElement)?.CaptureMouse();
                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Line)
            {
                var imgPoint = GetImagePixel(e, vm);
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
                var imgPoint = GetImagePixel(e, vm);
                vm.BeginBrushStroke();
                vm.SaveState();
                vm.BrushStroke(imgPoint, null);
                _lastBrushPoint = imgPoint;
                (sender as FrameworkElement)?.CaptureMouse();
                return;
            }

            if (vm?.ActiveTool == ToolType.Eyedropper)
            {
                var imgPoint = GetImagePixel(e, vm);
                var bitmap = vm.SelectedTab?.Image as WriteableBitmap;

                if (bitmap != null)
                {
                    int px = (int)imgPoint.X;
                    int py = (int)imgPoint.Y;

                    if (px >= 0 && px < bitmap.PixelWidth && py >= 0 && py < bitmap.PixelHeight)
                    {
                        byte[] pixel = new byte[4];
                        bitmap.CopyPixels(new Int32Rect(px, py, 1, 1), pixel, 4, 0);
                        vm.ActiveColor = Color.FromRgb(pixel[2], pixel[1], pixel[0]);
                    }
                }

                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Text)
            {
                var border = FindVisualChild<Border>(this, "TextOverlayBorder");
                var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");

                if (border?.Visibility == Visibility.Visible)
                {
                    var clickOnCanvas = e.GetPosition(canvas);
                    var borderLeft = Canvas.GetLeft(border);
                    var borderTop = Canvas.GetTop(border);
                    var borderRight = borderLeft + border.ActualWidth;
                    var borderBottom = borderTop + border.ActualHeight;

                    bool clickedInsideBorder = clickOnCanvas.X >= borderLeft && clickOnCanvas.X <= borderRight
                                            && clickOnCanvas.Y >= borderTop && clickOnCanvas.Y <= borderBottom;

                    if (clickedInsideBorder)
                    {
                        e.Handled = true;
                        return; // do not commit text if clicking inside the text box
                    }

                    CommitText(vm);
                    return;
                }

                var imgPoint = GetImagePixel(e, vm);
                var canvasPoint = e.GetPosition(canvas);
                ShowTextOverlay(canvasPoint, imgPoint, vm);
                e.Handled = true;
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

            if (_activeHandle != ResizeHandle.None && _resizeDragStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var canvasPoint = GetCanvasPoint(e, sender as Image, vm);
                var delta = new Point(canvasPoint.X - _resizeDragStart.Value.X,
                                      canvasPoint.Y - _resizeDragStart.Value.Y);

                var (newRect, newX, newY) = ComputeResizedRect(_resizeOriginalRect, _activeHandle, delta, vm,
                                                                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
                vm.ResizeFloatingPaste(newRect.Width, newRect.Height, newX, newY);
                UpdateSelectionOverlay(vm);
                e.Handled = true;
                return;
            }

            // Floating paste drag
            if (vm?.IsFloatingPaste == true && _floatingDragStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, vm);

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
                var current = GetImagePixel(e, vm);

                // Recover the image state before drawing the preview
                if (_linePreviewBackup != null && vm.SelectedTab != null)
                {
                    var preview = new WriteableBitmap(_linePreviewBackup);
                    vm.SelectedTab.Image = preview;

                    if (!vm.IsLineBezierMode && e.LeftButton == MouseButtonState.Pressed)
                    {
                        DrawingService.DrawLine(preview, _linePreviewStart.Value, current,
                                                        vm.LineWidth, vm.ActiveColor);
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
                            vm.LineWidth, vm.ActiveColor);
                    }

                    vm.OnPropertyChangedPublic(nameof(vm.CurrentImage));
                }
                return;
            }

            if (vm?.ActiveTool == ToolType.Selection && e.LeftButton == MouseButtonState.Pressed && _selectionStart.HasValue)
            {
                var img = sender as Image;
                var current = GetImagePixel(e, vm);
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
                var imgPoint = GetImagePixel(e, vm);
                vm.BrushStroke(imgPoint, _lastBrushPoint);
                _lastBrushPoint = imgPoint;
                e.Handled = true;
                return;
            }

            if (vm?.ActiveTool == ToolType.Eyedropper)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, vm);
                var bitmap = vm.SelectedTab?.Image as WriteableBitmap;
                var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");

                if (bitmap != null && canvas != null)
                {
                    UpdateEyedropperPreview(imgPoint, GetCanvasPoint(e, img, vm), bitmap);
                }
                return;
            }
            else
            {
                var preview = FindVisualChild<Border>(this, "EyedropperPreview");
                if (preview != null) preview.Visibility = Visibility.Collapsed;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _activeHandle = ResizeHandle.None;
            _resizeDragStart = null;

            _floatingDragStart = null;

            var vm = DataContext as MainViewModel;
            _selectionStart = null;
            _lastBrushPoint = null;
            if (!_isMiddleDragging)
                (sender as FrameworkElement)?.ReleaseMouseCapture();

            if (vm?.ActiveTool == ToolType.Line && !vm.IsLineBezierMode && vm.LineStart.HasValue)
            {
                var img = sender as Image;
                var imgPoint = GetImagePixel(e, vm);
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

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            HideEyedropperPreview();
        }

        private Point GetImagePixel(MouseEventArgs e, MainViewModel vm)
        {
            var img = FindVisualChild<Image>(this, "MainImage");
            if (img == null) return new Point();

            var pos = e.GetPosition(img);

            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null || img.ActualWidth <= 0 || img.ActualHeight <= 0) return pos;

            double scaleX = bitmap.PixelWidth / img.ActualWidth;
            double scaleY = bitmap.PixelHeight / img.ActualHeight;

            return new Point(pos.X * scaleX, pos.Y * scaleY);
        }

        private Point GetImagePixelFromPoint(Point pos, Image img, MainViewModel vm)
        {
            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null || img.ActualWidth <= 0) return pos;

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

                UpdateHandles(vm.IsFloatingPaste && vm.Selection.HasValue, c1, c2);
            }
            else
            {
                rect.Visibility = Visibility.Collapsed;
                if (rectBlack != null) rectBlack.Visibility = Visibility.Collapsed;
                if (rectFill != null) rectFill.Visibility = Visibility.Collapsed;
                UpdateHandles(false, new Point(), new Point());
            }
        }

        public void UpdateTextOverlayStyle(MainViewModel vm)
        {
            var box = FindVisualChild<TextBox>(this, "TextOverlayBox");
            var border = FindVisualChild<Border>(this, "TextOverlayBorder");
            if (box == null || border?.Visibility != Visibility.Visible) return;

            var img = FindVisualChild<Image>(this, "MainImage");
            var bitmap = vm.SelectedTab?.Image;
            if (img == null || bitmap == null || img.ActualWidth <= 0) return;

            double dpiScaleX = bitmap.PixelWidth / img.ActualWidth;

            // cursor and selection state
            int caretIndex = box.CaretIndex;
            int selectionStart = box.SelectionStart;
            int selectionLength = box.SelectionLength;

            box.FontSize = Math.Max(6, vm.TextFontSize / dpiScaleX * vm.Zoom);
            box.FontFamily = new FontFamily(vm.TextFontFamily);
            box.FontWeight = vm.TextBold ? FontWeights.Bold : FontWeights.Normal;
            box.FontStyle = vm.TextItalic ? FontStyles.Italic : FontStyles.Normal;
            box.TextAlignment = vm.TextAlignment;
            box.Foreground = new SolidColorBrush(vm.ActiveColor);
            box.CaretBrush = new SolidColorBrush(vm.ActiveColor);

            // Focus and selection restoration
            box.CaretIndex = Math.Min(caretIndex, box.Text.Length);
            box.Select(Math.Min(selectionStart, box.Text.Length),
                       Math.Min(selectionLength, box.Text.Length - selectionStart));

            // Focus the TextBox after style update to ensure caret visibility
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                new Action(() =>
                {
                    box.Focus();
                    box.CaretIndex = Math.Min(caretIndex, box.Text.Length);
                }));
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

        private void UpdateHandles(bool visible, Point c1, Point c2)
        {
            double cx = (c1.X + c2.X) / 2;
            double cy = (c1.Y + c2.Y) / 2;
            double half = HandleSize / 2;

            var positions = new (string name, double x, double y)[]
            {
                ("Handle_TL", c1.X, c1.Y),
                ("Handle_TC", cx,   c1.Y),
                ("Handle_TR", c2.X, c1.Y),
                ("Handle_ML", c1.X, cy),
                ("Handle_MR", c2.X, cy),
                ("Handle_BL", c1.X, c2.Y),
                ("Handle_BC", cx,   c2.Y),
                ("Handle_BR", c2.X, c2.Y),
            };

            foreach (var (name, x, y) in positions)
            {
                var h = FindVisualChild<Rectangle>(this, name);
                if (h == null) continue;
                h.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                if (visible)
                {
                    Canvas.SetLeft(h, x - half);
                    Canvas.SetTop(h, y - half);
                }
            }
        }

        private ResizeHandle HitTestHandle(Point canvasPoint, Point c1, Point c2)
        {
            double cx = (c1.X + c2.X) / 2;
            double cy = (c1.Y + c2.Y) / 2;
            double r = HandleHitRadius;

            (ResizeHandle h, double x, double y)[] handles =
            {
                (ResizeHandle.TL, c1.X, c1.Y), (ResizeHandle.TC, cx, c1.Y), (ResizeHandle.TR, c2.X, c1.Y),
                (ResizeHandle.ML, c1.X, cy),                                 (ResizeHandle.MR, c2.X, cy),
                (ResizeHandle.BL, c1.X, c2.Y), (ResizeHandle.BC, cx, c2.Y), (ResizeHandle.BR, c2.X, c2.Y),
            };

            foreach (var (handle, x, y) in handles)
                if (Math.Abs(canvasPoint.X - x) <= r && Math.Abs(canvasPoint.Y - y) <= r)
                    return handle;

            return ResizeHandle.None;
        }

        private Point GetCanvasPoint(MouseEventArgs e, Image img, MainViewModel vm)
        {
            var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");
            return canvas != null ? e.GetPosition(canvas) : e.GetPosition(img);
        }

        private (Point c1, Point c2) GetSelectionCanvasPoints(MainViewModel vm, Image img)
        {
            var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");
            if (canvas == null || img == null || img.ActualWidth <= 0)
                return (new Point(), new Point());

            var s = vm.Selection.Value;
            var bitmap = vm.SelectedTab?.Image;

            double dpiScaleX = bitmap.PixelWidth / img.ActualWidth;
            double dpiScaleY = bitmap.PixelHeight / img.ActualHeight;

            var p1 = new Point(s.X / dpiScaleX, s.Y / dpiScaleY);
            var p2 = new Point((s.X + s.Width) / dpiScaleX, (s.Y + s.Height) / dpiScaleY);

            var transform = img.TransformToVisual(canvas);
            return (transform.Transform(p1), transform.Transform(p2));
        }

        private (Int32Rect rect, int newX, int newY) ComputeResizedRect(Int32Rect orig, ResizeHandle handle, Point delta,   MainViewModel vm, bool keepAspect)
        {
            var img = FindVisualChild<Image>(this, "MainImage");
            if (img == null) return (orig, orig.X, orig.Y);

            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null) return (orig, orig.X, orig.Y);

            double dpiScaleX = bitmap.PixelWidth / (img.ActualWidth * vm.Zoom);
            double dpiScaleY = bitmap.PixelHeight / (img.ActualHeight * vm.Zoom);

            int dx = (int)(delta.X * dpiScaleX);
            int dy = (int)(delta.Y * dpiScaleY);

            int x = orig.X, y = orig.Y, w = orig.Width, h = orig.Height;

            switch (handle)
            {
                case ResizeHandle.TL: x += dx; y += dy; w -= dx; h -= dy; break;
                case ResizeHandle.TC: y += dy; h -= dy; break;
                case ResizeHandle.TR: y += dy; w += dx; h -= dy; break;
                case ResizeHandle.ML: x += dx; w -= dx; break;
                case ResizeHandle.MR: w += dx; break;
                case ResizeHandle.BL: x += dx; w -= dx; h += dy; break;
                case ResizeHandle.BC: h += dy; break;
                case ResizeHandle.BR: w += dx; h += dy; break;
            }

            w = Math.Max(4, w);
            h = Math.Max(4, h);

            if (keepAspect)
            {
                double aspect = (double)orig.Width / orig.Height;
                if (w / (double)h > aspect)
                    w = (int)(h * aspect);
                else
                    h = (int)(w / aspect);
            }

            switch (handle)
            {
                case ResizeHandle.TL:
                    x = orig.X + orig.Width - w;
                    y = orig.Y + orig.Height - h;
                    break;
                case ResizeHandle.TC:
                    y = orig.Y + orig.Height - h;
                    break;
                case ResizeHandle.TR:
                    x = orig.X;
                    y = orig.Y + orig.Height - h;
                    break;
                case ResizeHandle.ML:
                    x = orig.X + orig.Width - w;
                    break;
                case ResizeHandle.BL:
                    x = orig.X + orig.Width - w;
                    break;
            }

            return (new Int32Rect(x, y, w, h), x, y);
        }

        private void HideEyedropperPreview()
        {
            var preview = FindVisualChild<Border>(this, "EyedropperPreview");
            if (preview != null) preview.Visibility = Visibility.Collapsed;
        }

        private void UpdateEyedropperPreview(Point imgPoint, Point canvasPoint, WriteableBitmap bitmap)
        {
            int px = (int)imgPoint.X;
            int py = (int)imgPoint.Y;

            if (px < 0 || px >= bitmap.PixelWidth || py < 0 || py >= bitmap.PixelHeight)
            {
                HideEyedropperPreview();
                return;
            }

            byte[] pixel = new byte[4];
            bitmap.CopyPixels(new Int32Rect(px, py, 1, 1), pixel, 4, 0);
            var color = Color.FromRgb(pixel[2], pixel[1], pixel[0]);

            var preview = FindVisualChild<Border>(this, "EyedropperPreview");
            var colorBox = FindVisualChild<Border>(this, "EyedropperColorPreview");
            var hexText = FindVisualChild<TextBlock>(this, "EyedropperHexText");

            if (preview == null) return;

            Canvas.SetLeft(preview, canvasPoint.X + 15);
            Canvas.SetTop(preview, canvasPoint.Y - 45);

            if (colorBox != null) colorBox.Background = new SolidColorBrush(color);
            if (hexText != null) hexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            preview.Visibility = Visibility.Visible;
        }

        private void ShowTextOverlay(Point canvasPoint, Point imagePoint, MainViewModel vm)
        {
            var box = FindVisualChild<TextBox>(this, "TextOverlayBox");
            var border = FindVisualChild<Border>(this, "TextOverlayBorder");
            var img = FindVisualChild<Image>(this, "MainImage");
            var selCanvas = FindVisualChild<Canvas>(this, "SelectionCanvas");

            if (box == null || border == null || img == null || selCanvas == null) return;

            _textPosition = canvasPoint;
            _textImagePosition = imagePoint;

            box.Text = "";

            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null || img.ActualWidth <= 0) return;

            double dpiScaleX = bitmap.PixelWidth / img.ActualWidth;
            double dpiScaleY = bitmap.PixelHeight / img.ActualHeight;

            _textImagePosition = new Point(
                _textImagePosition.Value.X + 3 * dpiScaleX,
                _textImagePosition.Value.Y + 3 * dpiScaleY
            );

            double handleHeightInPixels = 16 * dpiScaleY; // reserve space for one line of text above the click point to avoid covering it
            _textImagePosition = new Point(
                _textImagePosition.Value.X,
                _textImagePosition.Value.Y);

            var p = new Point(imagePoint.X / dpiScaleX, imagePoint.Y / dpiScaleY);
            var transform = img.TransformToVisual(selCanvas);
            var canvasPos = transform.Transform(p);

            Canvas.SetLeft(border, canvasPos.X);
            Canvas.SetTop(border, canvasPos.Y);

            border.Visibility = Visibility.Visible;
            UpdateTextOverlayStyle(vm);

            box.Focus();
            DisableHotkeys();
        }

        private void HideTextOverlay()
        {
            var border = FindVisualChild<Border>(this, "TextOverlayBorder");
            if (border != null) border.Visibility = Visibility.Collapsed;
            _textPosition = null;
            _textImagePosition = null;
            RestoreHotkeys();
        }

        private void CommitText(MainViewModel vm)
        {
            var box = FindVisualChild<TextBox>(this, "TextOverlayBox");
            if (box == null || string.IsNullOrWhiteSpace(box.Text))
            {
                HideTextOverlay();
                return;
            }

            if (_textImagePosition == null) return;

            vm.CommitText(
                box.Text,
                _textImagePosition.Value);

            HideTextOverlay();
        }

        private void TextOverlay_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            if (e.Key == Key.Escape)
            {
                HideTextOverlay();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter &&
                     (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                CommitText(vm);
                e.Handled = true;
            }
        }

        private void TextOverlay_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        public void DisableHotkeys()
        {
            if (_savedBindings != null) return;
            _savedBindings = new InputBindingCollection();
            foreach (InputBinding b in InputBindings)
                _savedBindings.Add(b);
            InputBindings.Clear();
        }

        public void RestoreHotkeys()
        {
            if (_savedBindings == null) return;
            InputBindings.Clear();
            foreach (InputBinding b in _savedBindings)
                InputBindings.Add(b);
            _savedBindings = null;
        }

        #region text dragging methods
        private void TextDragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = FindVisualChild<Border>(this, "TextOverlayBorder");
            var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");

            if (border == null || canvas == null) return;

            _isDraggingText = true;
            _textDragStart = e.GetPosition(canvas);
            _textDragOriginCanvas = new Point(Canvas.GetLeft(border), Canvas.GetTop(border));

            (sender as Border)?.CaptureMouse();
            e.Handled = true;
        }

        private void TextDragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingText || !_textDragStart.HasValue) return;

            var border = FindVisualChild<Border>(this, "TextOverlayBorder");
            var canvas = FindVisualChild<Canvas>(this, "SelectionCanvas");
            var img = FindVisualChild<Image>(this, "MainImage");
            var vm = DataContext as MainViewModel;
            if (border == null || canvas == null || img == null || vm == null) return;

            var current = e.GetPosition(canvas);
            double newLeft = _textDragOriginCanvas.X + (current.X - _textDragStart.Value.X);
            double newTop = _textDragOriginCanvas.Y + (current.Y - _textDragStart.Value.Y);

            Canvas.SetLeft(border, newLeft);
            Canvas.SetTop(border, newTop);

            var bitmap = vm.SelectedTab?.Image;
            if (bitmap == null || img.ActualWidth <= 0) return;

            double dpiScaleX = bitmap.PixelWidth / img.ActualWidth;
            double dpiScaleY = bitmap.PixelHeight / img.ActualHeight;

            var transform = canvas.TransformToVisual(img);
            var imgPoint = transform.Transform(new Point(newLeft, newTop));

            _textImagePosition = new Point(
                imgPoint.X * dpiScaleX,
                imgPoint.Y * dpiScaleY);

            e.Handled = true;
        }

        private void TextDragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingText = false;
            _textDragStart = null;
            (sender as Border)?.ReleaseMouseCapture();
            e.Handled = true;
        }
        #endregion
    }
}