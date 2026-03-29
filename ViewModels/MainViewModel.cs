using ImageEditor.Commands;
using ImageEditor.Models;
using ImageEditor.Services;
using ImageEditor.Services.ImageProcessing;
using ImageEditor.Services.ImageStatus;
using ImageEditor.Services.Math;
using ImageEditor.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ================= TABS =================
        private ObservableCollection<ImageTab> _tabs = new ObservableCollection<ImageTab>();
        public ObservableCollection<ImageTab> Tabs
        {
            get => _tabs;
            set { _tabs = value; OnPropertyChanged(); }
        }

        private ImageTab _selectedTab;
        public ImageTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasImage));
                StatusText = _selectedTab != null
                    ? $"Loaded: {_selectedTab.FilePath ?? _selectedTab.Title}"
                    : "No image loaded";
            }
        }

        public BitmapSource CurrentImage => SelectedTab?.Image;
        public bool HasImage => SelectedTab?.Image != null;

        // ================= STATUS =================
        private string _statusText = "No image loaded";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Tools.Clamp(value, 0.1, 5.0);
                OnPropertyChanged();
            }
        }

        private ToolType _activeTool = ToolType.None;
        public ToolType ActiveTool
        {
            get => _activeTool;
            set { _activeTool = value; OnPropertyChanged(); }
        }

        private int _brushSize = 10;
        public int BrushSize
        {
            get => _brushSize;
            set { _brushSize = value; OnPropertyChanged(); }
        }

        private double _brushHardness = 1.0;
        public double BrushHardness
        {
            get => _brushHardness;
            set { _brushHardness = value; OnPropertyChanged(); }
        }

        private LineSettingsWindow _lineSettingsWindow;

        private int _lineWidth = 2;
        public int LineWidth
        {
            get => _lineWidth;
            set { _lineWidth = value; OnPropertyChanged(); }
        }

        public Point? LineStart { get; set; }
        public Point? LineEnd { get; set; }
        public Point? BezierControl1 { get; set; }
        public Point? BezierControl2 { get; set; }
        public bool IsBezierSecondPhase { get; set; }

        private WriteableBitmap _clipboard;

        private Int32Rect? _selection;
        public Int32Rect? Selection
        {
            get => _selection;
            set { _selection = value; OnPropertyChanged(); }
        }

        private ColorPickerWindow _colorPickerWindow;


        // Floating settings window
        private BrushSettingsWindow _brushSettingsWindow;

        private double _imageOffsetX;
        public double ImageOffsetX
        {
            get => _imageOffsetX;
            set { _imageOffsetX = value; OnPropertyChanged(); }
        }

        private double _imageOffsetY;
        public double ImageOffsetY
        {
            get => _imageOffsetY;
            set { _imageOffsetY = value; OnPropertyChanged(); }
        }

        // floating paste
        private WriteableBitmap _pasteFloating;
        private WriteableBitmap _pasteBackground;
        private WriteableBitmap _pasteFloatingOriginal;

        private int _pasteX;
        private int _pasteY;
        public int PasteX => _pasteX;
        public int PasteY => _pasteY;

        public bool IsFloatingPaste => _pasteFloating != null;


        private Color _activeColor = Colors.Black;
        public Color ActiveColor
        {
            get => _activeColor;
            set { _activeColor = value; OnPropertyChanged(); }
        }

        //Text tool properties
        private string _textFontFamily = "Arial";
        public string TextFontFamily
        {
            get => _textFontFamily;
            set { _textFontFamily = value; OnPropertyChanged(); }
        }

        private double _textFontSize = 16;
        public double TextFontSize
        {
            get => _textFontSize;
            set { _textFontSize = value; OnPropertyChanged(); }
        }

        private bool _textBold;
        public bool TextBold
        {
            get => _textBold;
            set { _textBold = value; OnPropertyChanged(); }
        }

        private bool _textItalic;
        public bool TextItalic
        {
            get => _textItalic;
            set { _textItalic = value; OnPropertyChanged(); }
        }

        private TextAlignment _textAlignment = TextAlignment.Left;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set { _textAlignment = value; OnPropertyChanged(); }
        }

        private TextSettingsWindow _textSettingsWindow;
        public event Action TextOverlayShouldHide;

        // ================= COMMANDS =================
        public ICommand OpenImageCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand SaveAsImageCommand { get; }
        public ICommand CloseImageCommand { get; }
        public ICommand CloseTabCommand { get; }
        public ICommand ImageInfoCommand { get; }
        public ICommand NewImageCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AboutCommand { get; }

        public ICommand Rotate90Clockwise { get; }
        public ICommand Rotate90CounterClockwise { get; }
        public ICommand Rotate180 { get; }
        public ICommand FlipHorizontal { get; }
        public ICommand FlipVertical { get; }
        public ICommand GaussianBlurCommand { get; }
        public ICommand SharpenCommand { get; }
        public ICommand BrightnessCommand { get; }
        public ICommand GrayscaleCommand { get; }
        public ICommand SobelCommand { get; }
        public ICommand InvertCommand { get; }
        public ICommand PixelateCommand { get; }
        public ICommand GammaCommand { get; }

        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeRestoreCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand MouseWheelCommand { get; }

        // ================ TOOLBAR =================
        public ICommand SelectBrushCommand { get; }
        public ICommand SelectSelectionToolCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectLineToolCommand { get; }
        public ICommand ToggleColorPickerCommand { get; }
        public ICommand SelectEyedropperCommand { get; }
        public ICommand SelectTextToolCommand { get; }


        // ================= CONSTRUCTOR =================
        public MainViewModel()
        {
            #region White Tab Initialization
            var wb = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Bgr24, null);
            int stride = 800 * 3;
            byte[] pixels = Enumerable.Repeat((byte)255, 600 * stride).ToArray();

            wb.WritePixels(new Int32Rect(0, 0, 800, 600), pixels, stride, 0);

            var whiteTab = new ImageTab
            {
                Image = wb,
                Title = "White",
                FilePath = "No image loaded"
            };

            Tabs.Add(whiteTab);
            SelectedTab = whiteTab;
            wb = null;
            #endregion

            OpenImageCommand = new RelayCommand(_ => OpenImage());
            CloseImageCommand = new RelayCommand(_ => CloseTab(SelectedTab), _ => HasImage);
            CloseTabCommand = new RelayCommand(tab => CloseTab(tab as ImageTab));
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => HasImage);
            SaveAsImageCommand = new RelayCommand(_ => SaveImageAs(), _ => HasImage);

            NewImageCommand = new RelayCommand(_ => NewImage());

            ImageInfoCommand = new RelayCommand(_ =>
            {
                var info = ImageInfoService.GetInfo(SelectedTab.Image, SelectedTab.FilePath);
                var window = new ImageInfoWindow(info) { Owner = Application.Current.MainWindow };
                window.ShowDialog();
            }, _ => HasImage);

            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
            UndoCommand = new RelayCommand(_ => Undo(), _ => SelectedTab?.UndoStack.Count > 0);
            RedoCommand = new RelayCommand(_ => Redo(), _ => SelectedTab?.RedoStack.Count > 0);
            AboutCommand = new RelayCommand(_ => About());

            Rotate90Clockwise = new RelayCommand(_ => RotateImage(90, true), _ => HasImage);
            Rotate90CounterClockwise = new RelayCommand(_ => RotateImage(90, false), _ => HasImage);
            Rotate180 = new RelayCommand(_ => RotateImage(180, true), _ => HasImage);
            FlipHorizontal = new RelayCommand(_ => FlipImage(true), _ => HasImage);
            FlipVertical = new RelayCommand(_ => FlipImage(false), _ => HasImage);

            GaussianBlurCommand = new RelayCommand(_ => OpenFilterWindow<BlurWindow>(img => new BlurViewModel(img)), _ => HasImage);
            SharpenCommand = new RelayCommand(_ => OpenFilterWindow<SharpenWindow>(img => new SharpenViewModel(img)), _ => HasImage);
            BrightnessCommand = new RelayCommand(_ => OpenFilterWindow<BrightnessWindow>(img => new BrightnessViewModel(img)), _ => HasImage);
            GrayscaleCommand = new RelayCommand(_ => OpenFilterWindow<GrayscaleWindow>(img => new GrayscaleViewModel(img)), _ => HasImage);
            SobelCommand = new RelayCommand(_ => OpenFilterWindow<SobelWindow>(img => new SobelViewModel(img)), _ => HasImage);
            PixelateCommand = new RelayCommand(_ => OpenFilterWindow<PixelateWindow>(img => new PixelateViewModel(img)), _ => HasImage);
            GammaCommand = new RelayCommand(_ => OpenFilterWindow<GammaWindow>(img => new GammaViewModel(img)), _ => HasImage);

            InvertCommand = new RelayCommand(_ =>
            {
                try
                {
                    SaveState();
                    SelectedTab.Image = InvertHelper.ApplyInvert(SelectedTab.Image);
                    SelectedTab.IsModified = true;
                    OnPropertyChanged(nameof(CurrentImage));

                    Logger.Info("Invert filter applied");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to apply invert: {ex.Message}");
                    MessageBox.Show("Failed to apply invert. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, _ => HasImage);

            MinimizeCommand = new RelayCommand(_ => Application.Current.MainWindow.WindowState = WindowState.Minimized);
            MaximizeRestoreCommand = new RelayCommand(_ =>
            {
                var w = Application.Current.MainWindow;
                w.WindowState = w.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            });
            CloseCommand = new RelayCommand(_ => Application.Current.MainWindow.Close());

            MouseWheelCommand = new RelayCommand(parameter =>
            {
                var args = parameter as MouseWheelEventArgs;
                if (args == null)
                    return;

                var element = args.Source as FrameworkElement;
                if (element == null)
                    return;

                var mousePos = args.GetPosition(element);
                double factor = args.Delta > 0 ? 1.1 : 0.9;
                double oldZoom = Zoom;
                double newZoom = Tools.Clamp(oldZoom * factor, 0.1, 5.0);

                ImageOffsetX = mousePos.X - (mousePos.X - ImageOffsetX) * (newZoom / oldZoom);
                ImageOffsetY = mousePos.Y - (mousePos.Y - ImageOffsetY) * (newZoom / oldZoom);

                Zoom = newZoom;
            });


            SelectBrushCommand = new RelayCommand(_ => ToggleBrushTool());
            SelectSelectionToolCommand = new RelayCommand(_ => ToggleSelectionTool());
            ClearSelectionCommand = new RelayCommand(_ => Selection = null);

            CopyCommand = new RelayCommand(_ => CopySelection(), _ => Selection.HasValue && HasImage);
            CutCommand = new RelayCommand(_ => CutSelection(), _ => Selection.HasValue && HasImage);
            PasteCommand = new RelayCommand(_ => PasteClipboard());

            SelectAllCommand = new RelayCommand(_ =>
            {
                if (SelectedTab?.Image == null) return;
                ActiveTool = ToolType.Selection;
                Selection = new Int32Rect(0, 0, SelectedTab.Image.PixelWidth, SelectedTab.Image.PixelHeight);
            }, _ => HasImage);

            SelectLineToolCommand = new RelayCommand(_ => ToggleLineTool());
            ToggleColorPickerCommand = new RelayCommand(_ => ToggleColorPicker());

            SelectEyedropperCommand = new RelayCommand(_ => ToggleEyedropperTool());
            SelectTextToolCommand = new RelayCommand(_ => ToggleTextTool());

            Logger.Info("MonoFrame initialized successfully");
        }

        // ================= TABS =================
        private void OpenImage()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Open Image",
                    Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Multiselect = true
                };

                if (dialog.ShowDialog() != true) return;

                foreach (var file in dialog.FileNames)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgr24, null, 0);
                    var tab = new ImageTab
                    {
                        Image = new WriteableBitmap(converted),
                        Title = Path.GetFileName(file),
                        FilePath = file
                    };

                    Tabs.Add(tab);
                    SelectedTab = tab;

                    Logger.Info($"Image opened: {file}");
                }

                ResetView();
            }
            catch(Exception ex)
            {
                Logger.Error($"Failed to open image: {ex.Message}");
                MessageBox.Show("Failed to open image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseTab(ImageTab tab)
        {
            try
            {
                if (tab == null) return;

                if (tab.IsModified)
                {
                    var dialog = new UnsavedChangesDialog(tab.Title)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    dialog.ShowDialog();

                    if (dialog.Result == UnsavedChangesResult.Cancel) return;
                    if (dialog.Result == UnsavedChangesResult.Yes) SaveImage();
                }

                tab.UndoStack.Clear();
                tab.RedoStack.Clear();
                tab.Image = null;

                int index = Tabs.IndexOf(tab);
                Tabs.Remove(tab);

                if (Tabs.Count > 0)
                    SelectedTab = Tabs[Tools.Clamp(index, 0, Tabs.Count - 1)];
                else
                {
                    SelectedTab = null;
                    StatusText = "No image loaded";
                    ResetView();
                }

                GC.Collect(2, GCCollectionMode.Forced, blocking: true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, blocking: true);

                Logger.Info($"Tab closed: {tab.Title}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to close tab: {ex.Message}");
                MessageBox.Show("Failed to close tab. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImage()
        {
            try
            {
                if (SelectedTab == null) return;

                if (!string.IsNullOrEmpty(SelectedTab.FilePath))
                {
                    SaveImageHelper.SaveToFile(SelectedTab.FilePath, SelectedTab.Image);
                    SelectedTab.IsModified = false;
                    StatusText = $"Saved: {SelectedTab.FilePath}";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Title = "Save Image",
                    Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                    DefaultExt = ".png"
                };

                if (dialog.ShowDialog() != true) return;

                SelectedTab.FilePath = dialog.FileName;
                SelectedTab.Title = Path.GetFileName(dialog.FileName);
                SelectedTab.IsModified = false;

                SaveImageHelper.SaveToFile(SelectedTab.FilePath, SelectedTab.Image);
                StatusText = $"Saved: {SelectedTab.FilePath}";

                Logger.Info($"Image saved: {SelectedTab.FilePath}");
            }
            catch(Exception ex)
            {
                Logger.Error($"Failed to save image: {ex.Message}");
                MessageBox.Show("Failed to save image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImageAs()
        {
            if (SelectedTab == null) return;

            var dialog = new SaveFileDialog
            {
                Title = "Save Image As",
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                DefaultExt = ".png",
                FileName = SelectedTab.Title
            };

            if (dialog.ShowDialog() != true) return;

            SelectedTab.FilePath = dialog.FileName;
            SelectedTab.Title = Path.GetFileName(dialog.FileName);
            SelectedTab.IsModified = false;

            SaveImageHelper.SaveToFile(SelectedTab.FilePath, SelectedTab.Image);
            StatusText = $"Saved: {SelectedTab.FilePath}";

            Logger.Info($"Image saved as: {SelectedTab.FilePath}");
        }

        private void NewImage()
        {
            try
            {
                var dialog = new NewImageDialog { Owner = Application.Current.MainWindow };
                dialog.ShowDialog();

                if (!dialog.Confirmed) return;

                var wb = new WriteableBitmap(dialog.ImageWidth, dialog.ImageHeight, 96, 96, PixelFormats.Bgr24, null);
                int stride = dialog.ImageWidth * 3;
                byte[] pixels = Enumerable.Repeat((byte)255, dialog.ImageHeight * stride).ToArray();
                wb.WritePixels(new Int32Rect(0, 0, dialog.ImageWidth, dialog.ImageHeight), pixels, stride, 0);

                var tab = new ImageTab
                {
                    Image = wb,
                    Title = $"New {dialog.ImageWidth}×{dialog.ImageHeight}",
                    FilePath = null
                };

                Tabs.Add(tab);
                SelectedTab = tab;
                ResetView();

                Logger.Info($"New image created: {dialog.ImageWidth}×{dialog.ImageHeight}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create new image: {ex.Message}");
                MessageBox.Show("Failed to create new image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= TOOLS =================
        private void RotateImage(int angle, bool clockwise)
        {
            try
            {
                SaveState();
                int finalAngle = clockwise ? angle : -angle;
                var transform = new RotateTransform(finalAngle);
                SelectedTab.Image = new WriteableBitmap(new TransformedBitmap(SelectedTab.Image, transform));
                SelectedTab.IsModified = true;
                OnPropertyChanged(nameof(CurrentImage));

                Logger.Info($"Image rotated: {finalAngle} degrees");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to rotate image: {ex.Message}");
                MessageBox.Show("Failed to rotate image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FlipImage(bool horizontal)
        {
            try
            {
                SaveState();
                var transform = new ScaleTransform(horizontal ? -1 : 1, horizontal ? 1 : -1);
                SelectedTab.Image = new WriteableBitmap(new TransformedBitmap(SelectedTab.Image, transform));
                SelectedTab.IsModified = true;
                OnPropertyChanged(nameof(CurrentImage));

                Logger.Info($"Image flipped: {(horizontal ? "horizontal" : "vertical")}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to flip image: {ex.Message}");
                MessageBox.Show("Failed to flip image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFilterWindow<TWindow>(Func<WriteableBitmap, dynamic> viewModelFactory)
            where TWindow : Window, new()
        {
            try {
                if (SelectedTab == null) return;

                var writeable = SelectedTab.Image as WriteableBitmap
                                ?? new WriteableBitmap(SelectedTab.Image);
                var vm = viewModelFactory(writeable);

                var window = new TWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };

                vm.CloseAction = new Action<bool>(result =>
                {
                    if (result)
                    {
                        SaveState();
                        SelectedTab.Image = vm.ResultImage;
                        SelectedTab.IsModified = true;
                        OnPropertyChanged(nameof(CurrentImage));
                    }
                    window.Close();
                });

                window.ShowDialog();

                Logger.Info($"Filter applied: {typeof(TWindow).Name.Replace("Window", "")}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to apply filter: {ex.Message}");
                MessageBox.Show("Failed to apply filter. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BrushStroke(Point imagePoint, Point? previousPoint)
        {
            if (ActiveTool != ToolType.Brush) return;
            if (SelectedTab?.Image == null) return;

            WriteableBitmap wb;
            if (SelectedTab.Image is WriteableBitmap existing && !existing.IsFrozen)
                wb = existing;
            else
            {
                wb = new WriteableBitmap(SelectedTab.Image);
                SelectedTab.Image = wb;
                OnPropertyChanged(nameof(CurrentImage));
            }

            int radius = Math.Max(1, BrushSize / 2);

            if (previousPoint.HasValue)
                DrawingService.DrawLine(wb, previousPoint.Value, imagePoint, radius, ActiveColor, BrushHardness);
            else
                DrawingService.DrawCircle(wb, (int)imagePoint.X, (int)imagePoint.Y, radius, ActiveColor, BrushHardness);

            ExpandDirtyRegion(imagePoint);
            if (previousPoint.HasValue)
                ExpandDirtyRegion(previousPoint.Value);

            SelectedTab.IsModified = true;
        }

        public void BeginBrushStroke()
        {
            if (_brushSettingsWindow != null)
            {
                BrushSize = _brushSettingsWindow.BrushSize;
                BrushHardness = _brushSettingsWindow.BrushHardness;
            }
        }

        private bool _strokeInProgress;
        private Int32Rect _strokeDirtyRegion;
        private WriteableBitmap _strokeBefore;

        public void BeginStrokeSnapshot(Point startPoint)
        {
            if (SelectedTab?.Image == null) return;

            _strokeInProgress = true;
            _strokeBefore = new WriteableBitmap(SelectedTab.Image);

            _strokeDirtyRegion = Int32Rect.Empty;
            ExpandDirtyRegion(startPoint);
        }

        public void CommitStrokeSnapshot()
        {
            if (!_strokeInProgress || SelectedTab == null) return;

            _strokeInProgress = false;

            if (_strokeDirtyRegion.Width <= 0 || _strokeDirtyRegion.Height <= 0)
                return;

            var snapshot = new ImageSnapshot(_strokeBefore, _strokeDirtyRegion);

            SelectedTab.UndoStack.Push(snapshot);
            SelectedTab.RedoStack.Clear();

            _strokeBefore = null;
            _strokeDirtyRegion = Int32Rect.Empty;
        }

        private void ExpandDirtyRegion(Point p)
        {
            int r = BrushSize;

            var rect = new Int32Rect(
                (int)p.X - r,
                (int)p.Y - r,
                r * 2,
                r * 2);

            if (_strokeDirtyRegion.IsEmpty)
            {
                _strokeDirtyRegion = rect;
            }
            else
            {
                int x1 = Math.Min(_strokeDirtyRegion.X, rect.X);
                int y1 = Math.Min(_strokeDirtyRegion.Y, rect.Y);

                int x2 = Math.Max(_strokeDirtyRegion.X + _strokeDirtyRegion.Width, rect.X + rect.Width);
                int y2 = Math.Max(_strokeDirtyRegion.Y + _strokeDirtyRegion.Height, rect.Y + rect.Height);

                _strokeDirtyRegion = new Int32Rect(x1, y1, x2 - x1, y2 - y1);
            }
        }

        #region toggle tools methods
        private void ToggleBrushTool()
        {
            if (ActiveTool == ToolType.Brush)
            {
                ActiveTool = ToolType.None;
                _brushSettingsWindow?.Close();
                _brushSettingsWindow = null;
                return;
            }

            CloseAllToolWindows(ToolType.Brush);
            ActiveTool = ToolType.Brush;

            _brushSettingsWindow = new BrushSettingsWindow
            {
                Owner = Application.Current.MainWindow
            };

            _brushSettingsWindow.Closed += (s, e) =>
            {
                if (ActiveTool == ToolType.Brush)
                {
                    ActiveTool = ToolType.None;
                }
                _brushSettingsWindow = null;
            };

            var main = Application.Current.MainWindow;
            _brushSettingsWindow.Left = main.Left + 50;
            _brushSettingsWindow.Top = main.Top + 80;

            _brushSettingsWindow.Show();
        }

        private void ToggleSelectionTool()
        {
            if (ActiveTool == ToolType.Selection)
            {
                ActiveTool = ToolType.None;
                return;
            }

            CloseAllToolWindows();
            ActiveTool = ToolType.Selection;
            Selection = null;
        }

        private void ToggleLineTool()
        {
            if (ActiveTool == ToolType.Line)
            {
                ActiveTool = ToolType.None;
                _lineSettingsWindow?.Close();
                _lineSettingsWindow = null;
                return;
            }

            CloseAllToolWindows(ToolType.Line);
            ActiveTool = ToolType.Line;
            LineStart = null;
            LineEnd = null;
            IsBezierSecondPhase = false;

            _lineSettingsWindow = new LineSettingsWindow
            {
                Owner = Application.Current.MainWindow
            };
            _lineSettingsWindow.Closed += (s, e) =>
            {
                if (ActiveTool == ToolType.Line)
                    ActiveTool = ToolType.None;
                _lineSettingsWindow = null;
            };

            var main = Application.Current.MainWindow;
            _lineSettingsWindow.Left = main.Left + 50;
            _lineSettingsWindow.Top = main.Top + 80;
            _lineSettingsWindow.Show();
        }

        private void ToggleEyedropperTool()
        {
            if (ActiveTool == ToolType.Eyedropper)
            {
                ActiveTool = ToolType.None;
                return;
            }

            CloseAllToolWindows();
            ActiveTool = ToolType.Eyedropper;
        }

        private void ToggleColorPicker()
        {
            if (_colorPickerWindow != null)
            {
                _colorPickerWindow.Close();
                _colorPickerWindow = null;
                return;
            }

            _colorPickerWindow = new ColorPickerWindow(ActiveColor)
            {
                Owner = Application.Current.MainWindow
            };

            _colorPickerWindow.ColorChanged += color => ActiveColor = color;
            _colorPickerWindow.Closed += (s, e) => _colorPickerWindow = null;

            var main = Application.Current.MainWindow;

            _colorPickerWindow.Left = main.Left + 50;
            _colorPickerWindow.Top = main.Top + main.ActualHeight - _colorPickerWindow.Height - 40;
            _colorPickerWindow.Show();
        }

        private void CloseAllToolWindows(ToolType except = ToolType.None)
        {
            if (except != ToolType.Brush)
            {
                _brushSettingsWindow?.Close();
                _brushSettingsWindow = null;
            }

            if (except != ToolType.Line)
            {
                _lineSettingsWindow?.Close();
                _lineSettingsWindow = null;
            }

            if (except != ToolType.Text)
            {
                _textSettingsWindow?.Close();
                _textSettingsWindow = null;
            }
        }

        private void ToggleTextTool()
        {
            if (ActiveTool == ToolType.Text)
            {
                ActiveTool = ToolType.None;
                _textSettingsWindow?.Close();
                _textSettingsWindow = null;
                return;
            }

            CloseAllToolWindows();
            ActiveTool = ToolType.Text;
            TextFontFamily = "Arial";
            TextFontSize = 16;
            TextBold = false;
            TextItalic = false;
            TextAlignment = TextAlignment.Left;


            _textSettingsWindow = new TextSettingsWindow
            {
                Owner = Application.Current.MainWindow
            };

            _textSettingsWindow.SettingsChanged += () =>
            {
                TextFontFamily = _textSettingsWindow.SelectedFont;
                TextFontSize = _textSettingsWindow.FontSize;
                TextBold = _textSettingsWindow.IsBold;
                TextItalic = _textSettingsWindow.IsItalic;
                TextAlignment = _textSettingsWindow.Alignment;

                // update text overlay if open
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.UpdateTextOverlayStyle(this);
            };

            _textSettingsWindow.Closed += (s, e) =>
            {
                if (ActiveTool == ToolType.Text)
                    ActiveTool = ToolType.None;
                _textSettingsWindow = null;
            };

            var main = Application.Current.MainWindow;
            _textSettingsWindow.Left = main.Left + 50;
            _textSettingsWindow.Top = main.Top + 80;
            _textSettingsWindow.Show();
        }
        #endregion

        private void CopySelection()
        {
            if (!Selection.HasValue || SelectedTab?.Image == null) return;
            var wb = SelectedTab.Image as WriteableBitmap ?? new WriteableBitmap(SelectedTab.Image);
            _clipboard = SelectionService.Copy(wb, Selection.Value);

            if (_clipboard != null)
            {
                Clipboard.SetImage(_clipboard);
            }
        }

        private void CutSelection()
        {
            if (!Selection.HasValue || SelectedTab?.Image == null) return;
            SaveState();

            var wb = new WriteableBitmap(SelectedTab.Image);
            SelectedTab.Image = wb;

            _clipboard = SelectionService.Cut(wb, Selection.Value);

            if (_clipboard != null)
                Clipboard.SetImage(_clipboard);

            SelectedTab.IsModified = true;
            Selection = null;
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void PasteClipboard()
        {
            try
            {
                if (SelectedTab?.Image == null) return;

                WriteableBitmap source = _clipboard;

                if (source == null && Clipboard.ContainsImage())
                {
                    var img = Clipboard.GetImage();
                    if (img != null)
                        source = new WriteableBitmap(img);
                }

                if (source == null) return;

                if (IsFloatingPaste) CommitFloatingPaste();

                // If the pasted image is larger than the current canvas, we need to expand it first
                var current = SelectedTab.Image;
                if (source.PixelWidth > current.PixelWidth || source.PixelHeight > current.PixelHeight)
                {
                    int newW = Math.Max(source.PixelWidth, current.PixelWidth);
                    int newH = Math.Max(source.PixelHeight, current.PixelHeight);

                    var dialog = new ExpandCanvasdialog(
                        source.PixelWidth, source.PixelHeight,
                        current.PixelWidth, current.PixelHeight)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    dialog.ShowDialog();

                    if (dialog.Confirmed)
                    {
                        SaveState();

                        var expanded = new WriteableBitmap(newW, newH, 96, 96, PixelFormats.Bgr24, null);
                        int stride = newW * 3;
                        byte[] white = Enumerable.Repeat((byte)255, newH * stride).ToArray();
                        expanded.WritePixels(new Int32Rect(0, 0, newW, newH), white, stride, 0);
                        SelectionService.Paste(expanded, new WriteableBitmap(current), 0, 0);

                        SelectedTab.Image = expanded;
                        OnPropertyChanged(nameof(CurrentImage));
                    }

                    SaveState();

                    var wbitmap = new WriteableBitmap(SelectedTab.Image);

                    _pasteFloating = source;
                    _pasteFloatingOriginal = source;
                    _pasteX = 0;
                    _pasteY = 0;

                    _pasteBackground = CaptureBackground(wbitmap, 0, 0, source.PixelWidth, source.PixelHeight);

                    SelectionService.Paste(wbitmap, source, _pasteX, _pasteY);
                    SelectedTab.IsModified = true;
                    Selection = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
                    OnPropertyChanged(nameof(CurrentImage));
                    OnPropertyChanged(nameof(IsFloatingPaste));
                }

                SaveState();

                var wb = new WriteableBitmap(SelectedTab.Image);
                SelectedTab.Image = wb;

                _pasteFloating = source;
                _pasteFloatingOriginal = source;
                _pasteX = 0;
                _pasteY = 0;

                _pasteBackground = CaptureBackground(wb, 0, 0, source.PixelWidth, source.PixelHeight);

                SelectionService.Paste(wb, source, _pasteX, _pasteY);
                SelectedTab.IsModified = true;
                Selection = new Int32Rect(_pasteX, _pasteY, source.PixelWidth, source.PixelHeight);
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(IsFloatingPaste));

                Logger.Info("Image pasted from clipboard");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to paste image: {ex.Message}");
                MessageBox.Show("Failed to paste image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MoveFloatingPaste(int newX, int newY)
        {
            if (!IsFloatingPaste || SelectedTab?.Image == null) return;

            var wb = SelectedTab.Image as WriteableBitmap;
            if (wb == null || wb.IsFrozen) return;

            // Restore the original background before moving
            SelectionService.Paste(wb, _pasteBackground, _pasteX, _pasteY);

            _pasteX = newX;
            _pasteY = newY;

            _pasteBackground = CaptureBackground(wb, _pasteX, _pasteY,
                                                _pasteFloating.PixelWidth, _pasteFloating.PixelHeight);

            SelectionService.Paste(wb, _pasteFloating, _pasteX, _pasteY);

            Selection = new Int32Rect(_pasteX, _pasteY, _pasteFloating.PixelWidth, _pasteFloating.PixelHeight);
            OnPropertyChanged(nameof(CurrentImage));
        }

        private WriteableBitmap CaptureBackground(WriteableBitmap wb, int x, int y, int w, int h)
        {
            var bg = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr24, null);
            int stride = w * 3;
            byte[] white = Enumerable.Repeat((byte)255, h * stride).ToArray();
            bg.WritePixels(new Int32Rect(0, 0, w, h), white, stride, 0);

            int clampedX = Math.Max(0, x);
            int clampedY = Math.Max(0, y);
            int clampedW = Math.Min(w, wb.PixelWidth - clampedX);
            int clampedH = Math.Min(h, wb.PixelHeight - clampedY);

            if (clampedW <= 0 || clampedH <= 0) return bg;

            int bpp = 3;
            int srcStride = clampedW * bpp;
            byte[] pixels = new byte[clampedH * srcStride];
            wb.CopyPixels(new Int32Rect(clampedX, clampedY, clampedW, clampedH), pixels, srcStride, 0);

            int dstX = clampedX - x;
            int dstY = clampedY - y;

            int dstStride = w * bpp;
            byte[] bgPixels = new byte[h * dstStride];
            bg.CopyPixels(bgPixels, dstStride, 0);

            for (int row = 0; row < clampedH; row++)
            {
                int src = row * srcStride;
                int dst = (dstY + row) * dstStride + dstX * bpp;
                int copyLen = Math.Min(srcStride, bgPixels.Length - dst);
                if (copyLen <= 0) break;
                Array.Copy(pixels, src, bgPixels, dst, copyLen);
            }

            bg.WritePixels(new Int32Rect(0, 0, w, h), bgPixels, dstStride, 0);
            return bg;
        }

        public void CommitFloatingPaste()
        {
            _pasteFloating = null;
            _pasteBackground = null;
            Selection = null;
            OnPropertyChanged(nameof(IsFloatingPaste));
        }

        public void CancelFloatingPaste()
        {
            if (!IsFloatingPaste) return;

            // Recover the original background
            var wb = SelectedTab?.Image as WriteableBitmap;
            if (wb != null)
            {
                SelectionService.Paste(wb, _pasteBackground, _pasteX, _pasteY);
                OnPropertyChanged(nameof(CurrentImage));
            }

            _pasteFloating = null;
            _pasteBackground = null;
            Selection = null;
            OnPropertyChanged(nameof(IsFloatingPaste));
        }

        public void BeginLineSettings()
        {
            if (_lineSettingsWindow != null)
            {
                LineWidth = _lineSettingsWindow.LineWidth;
            }
        }

        public bool IsLineBezierMode =>
            _lineSettingsWindow?.Mode == LineMode.Bezier;

        public void CommitLine(Point from, Point to, Point? cp1 = null, Point? cp2 = null)
        {
            try
            {
                if (SelectedTab?.Image == null) return;

                var wb = SelectedTab.Image as WriteableBitmap;
                if (wb == null || wb.IsFrozen)
                {
                    wb = new WriteableBitmap(SelectedTab.Image);
                    SelectedTab.Image = wb;
                    OnPropertyChanged(nameof(CurrentImage));
                }

                if (cp1.HasValue && cp2.HasValue)
                    DrawingService.DrawBezier(wb, from, cp1.Value, cp2.Value, to, LineWidth, ActiveColor);
                else
                    DrawingService.DrawLine(wb, from, to, LineWidth, ActiveColor);

                SelectedTab.IsModified = true;

                LineStart = null;
                LineEnd = null;
                BezierControl1 = null;
                BezierControl2 = null;
                IsBezierSecondPhase = false;

                Logger.Info($"Line drawn: from {from} to {to} with width {LineWidth}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to draw line: {ex.Message}");
                MessageBox.Show("Failed to draw line. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CommitText(string text, Point imagePosition)
        {
            try
            {
                if (SelectedTab?.Image == null) return;
                SaveState();

                var wb = SelectedTab.Image as WriteableBitmap
                         ?? new WriteableBitmap(SelectedTab.Image);

                var visual = new DrawingVisual();
                using (var ctx = visual.RenderOpen())
                {
                    // Existing image rendering
                    ctx.DrawImage(wb, new Rect(0, 0, wb.PixelWidth, wb.PixelHeight));

                    // Text rendering
                    var formattedText = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(
                            new FontFamily(TextFontFamily),
                            TextItalic ? FontStyles.Italic : FontStyles.Normal,
                            TextBold ? FontWeights.Bold : FontWeights.Normal,
                            FontStretches.Normal),
                        TextFontSize,
                        new SolidColorBrush(ActiveColor),
                        120);

                    formattedText.TextAlignment = TextAlignment;
                    ctx.DrawText(formattedText, imagePosition);
                }

                var rtb = new RenderTargetBitmap(
                    wb.PixelWidth, wb.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(visual);

                var converted = new FormatConvertedBitmap(rtb, PixelFormats.Bgr24, null, 0);
                SelectedTab.Image = new WriteableBitmap(converted);
                SelectedTab.IsModified = true;
                OnPropertyChanged(nameof(CurrentImage));

                Logger.Info($"Text drawn: \"{text}\" at {imagePosition} with font {TextFontFamily}, size {TextFontSize}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add text: {ex.Message}");
                MessageBox.Show("Failed to add text. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ResizeFloatingPaste(int newW, int newH, int newX, int newY)
        {
            if (!IsFloatingPaste || SelectedTab?.Image == null) return;

            newW = Math.Max(4, newW);
            newH = Math.Max(4, newH);

            var wb = SelectedTab.Image as WriteableBitmap;
            if (wb == null || wb.IsFrozen) return;

            // Restore the background before resizing
            SelectionService.Paste(wb, _pasteBackground, _pasteX, _pasteY);

            // Make sure the new size doesn't exceed canvas bounds
            _pasteFloating = SelectionService.Resize(_pasteFloatingOriginal, newW, newH);

            _pasteX = newX;
            _pasteY = newY;

            _pasteBackground = CaptureBackground(wb, _pasteX, _pasteY, newW, newH);

            SelectionService.Paste(wb, _pasteFloating, _pasteX, _pasteY);

            Selection = new Int32Rect(_pasteX, _pasteY, newW, newH);
            OnPropertyChanged(nameof(CurrentImage));
        }

        // ================= UNDO / REDO =================
        public void SaveState()
        {
            if (SelectedTab?.Image == null) return;
            var region = new Int32Rect(0, 0, SelectedTab.Image.PixelWidth, SelectedTab.Image.PixelHeight);
            SaveState(region);
        }

        public void SaveState(Int32Rect region) // partial save state for selection-based edits
        {
            if (SelectedTab?.Image == null) return;

            var snapshot = new ImageSnapshot(SelectedTab.Image, region);
            SelectedTab.UndoStack.Push(snapshot);
            SelectedTab.RedoStack.Clear();

            if (SelectedTab.UndoStack.ShouldCollect())
            {
                GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
                GC.WaitForPendingFinalizers();
            }
        }

        private void Undo()
        {
            try
            {
                if (SelectedTab?.UndoStack.Count == 0) return;

                var wb = SelectedTab.Image as WriteableBitmap ?? new WriteableBitmap(SelectedTab.Image);
                SelectedTab.Image = wb;

                var undo = SelectedTab.UndoStack.Pop();

                var redoSnapshot = ImageSnapshot.CreateDiff(wb, undo.Region, undo.Pixels);
                if (redoSnapshot != null)
                    SelectedTab.RedoStack.Push(redoSnapshot);

                undo.Restore(wb);

                SelectedTab.IsModified = SelectedTab.UndoStack.Count > 0;
                OnPropertyChanged(nameof(CurrentImage));

                Logger.Info("Undo performed");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to undo: {ex.Message}");
                MessageBox.Show("Failed to undo. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Redo()
        {
            try
            {
                if (SelectedTab?.RedoStack.Count == 0) return;

                var wb = SelectedTab.Image as WriteableBitmap ?? new WriteableBitmap(SelectedTab.Image);
                SelectedTab.Image = wb;

                var redo = SelectedTab.RedoStack.Pop();

                var undoSnapshot = ImageSnapshot.CreateDiff(wb, redo.Region, redo.Pixels);
                if (undoSnapshot != null)
                    SelectedTab.UndoStack.Push(undoSnapshot);

                redo.Restore(wb);

                SelectedTab.IsModified = true;
                OnPropertyChanged(nameof(CurrentImage));

                Logger.Info("Redo performed");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to redo: {ex.Message}");
                MessageBox.Show("Failed to redo. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= HELPERS =================
        private void ResetView()
        {
            Zoom = 1.0;
            ImageOffsetX = 0;
            ImageOffsetY = 0;
        }

        private void About()
        {
            MessageBox.Show(
                "MonoFrame\nSimple WPF Image Editor\n\nMVVM Architecture",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ================= DRAG =================
        private Point? _dragStart;

        public void StartDrag(Point p) => _dragStart = p;
        public void EndDrag() => _dragStart = null;

        public void DragTo(Point current)
        {
            if (_dragStart == null) return;
            ImageOffsetX += current.X - _dragStart.Value.X;
            ImageOffsetY += current.Y - _dragStart.Value.Y;
            _dragStart = current;
        }

        // ================= WINDOW =================
        private void MinimizeWindow() => Application.Current.MainWindow.WindowState = WindowState.Minimized;
        private void CloseWindow() => Application.Current.MainWindow.Close();
        private void MaximizeRestoreWindow()
        {
            var w = Application.Current.MainWindow;
            w.WindowState = w.WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;
        }

        public void OnPropertyChangedPublic(string name) => OnPropertyChanged(name);
    }
}