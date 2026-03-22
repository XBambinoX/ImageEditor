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

        // ================= CONSTRUCTOR =================
        public MainViewModel()
        {
            #region White Tab Initialization
            var wb = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Bgra32, null);

            int stride = 800 * 4;
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
                SaveState();
                SelectedTab.Image = InvertHelper.ApplyInvert(SelectedTab.Image);
                SelectedTab.IsModified = true;
                OnPropertyChanged(nameof(CurrentImage));
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
        }

        // ================= TABS =================
        private void OpenImage()
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

                var tab = new ImageTab
                {
                    Image = new WriteableBitmap(bitmap),
                    Title = Path.GetFileName(file),
                    FilePath = file
                };

                Tabs.Add(tab);
                SelectedTab = tab;
            }

            ResetView();
        }

        private void CloseTab(ImageTab tab)
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
        }

        private void SaveImage()
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
        }

        private void NewImage()
        {
            var dialog = new NewImageDialog { Owner = Application.Current.MainWindow };
            dialog.ShowDialog();

            if (!dialog.Confirmed) return;

            var wb = new WriteableBitmap(dialog.ImageWidth, dialog.ImageHeight, 96, 96, PixelFormats.Bgra32, null);
            int stride = dialog.ImageWidth *4;
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
        }

        // ================= TOOLS =================
        private void RotateImage(int angle, bool clockwise)
        {
            SaveState();
            int finalAngle = clockwise ? angle : -angle;
            var transform = new RotateTransform(finalAngle);
            SelectedTab.Image = new WriteableBitmap(new TransformedBitmap(SelectedTab.Image, transform));
            SelectedTab.IsModified = true;
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void FlipImage(bool horizontal)
        {
            SaveState();
            var transform = new ScaleTransform(horizontal ? -1 : 1, horizontal ? 1 : -1);
            SelectedTab.Image = new WriteableBitmap(new TransformedBitmap(SelectedTab.Image, transform));
            SelectedTab.IsModified = true;
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void OpenFilterWindow<TWindow>(Func<WriteableBitmap, dynamic> viewModelFactory)
            where TWindow : Window, new()
        {
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
        }

        // ================= UNDO / REDO =================
        private void SaveState()
        {
            if (SelectedTab?.Image == null) return;
            SelectedTab.UndoStack.Push(new WriteableBitmap(SelectedTab.Image));
            SelectedTab.RedoStack.Clear();
        }

        private void Undo()
        {
            if (SelectedTab?.UndoStack.Count == 0) return;
            SelectedTab.RedoStack.Push(new WriteableBitmap(SelectedTab.Image));
            SelectedTab.Image = SelectedTab.UndoStack.Pop();
            SelectedTab.IsModified = SelectedTab.UndoStack.Count > 0;
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void Redo()
        {
            if (SelectedTab?.RedoStack.Count == 0) return;
            SelectedTab.UndoStack.Push(new WriteableBitmap(SelectedTab.Image));
            SelectedTab.Image = SelectedTab.RedoStack.Pop();
            SelectedTab.IsModified = true;
            OnPropertyChanged(nameof(CurrentImage));
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
    }
}