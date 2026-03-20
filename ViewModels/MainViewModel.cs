using ImageEditor.Commands;
using ImageEditor.Models;
using ImageEditor.Services;
using ImageEditor.Services.ImageProcessing;
using ImageEditor.Services.ImageStatus;
using ImageEditor.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ================= IMAGE =================
        private BitmapSource _image;
        public BitmapSource Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        private readonly Stack<BitmapSource> _undoStack = new Stack<BitmapSource>();
        private readonly Stack<BitmapSource> _redoStack = new Stack<BitmapSource>();

        private string _currentFilePath;
        public string CurrentFilePath
        {
            get => _currentFilePath;
            set
            {
                _currentFilePath = value;
                OnPropertyChanged();
            }
        }
        // ================= STATUS =================
        private string _statusText = "No image loaded";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        // ================= IMAGE TRANSFORM =================
        private double _imageOffsetX;
        public double ImageOffsetX
        {
            get => _imageOffsetX;
            set
            {
                _imageOffsetX = value;
                OnPropertyChanged();
            }
        }

        private double _imageOffsetY;
        public double ImageOffsetY
        {
            get => _imageOffsetY;
            set
            {
                _imageOffsetY = value;
                OnPropertyChanged();
            }
        }

        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value < 0.1) value = 0.1;
                if (value > 5) value = 5;

                _zoom = value;
                OnPropertyChanged(nameof(Zoom));
            }
        }

        private bool _isSidebarVisible = true;
        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set { _isSidebarVisible = value; OnPropertyChanged(); }
        }

        // ================= COMMANDS =================
        public ICommand OpenImageCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand CloseImageCommand { get; }

        public ICommand ImageInfoCommand { get; }

        public ICommand ExitCommand { get; }

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AboutCommand { get; }

        //tools
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
            // Create a blank white image
            var wb = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Bgra32, null);

            int stride = 800 * 4;
            byte[] pixels = new byte[stride * 600];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 255;

            wb.WritePixels(new Int32Rect(0, 0, 800, 600), pixels, stride, 0);
            _image = wb;
            wb = null;

            // File commands
            OpenImageCommand = new RelayCommand(_ => OpenImage());
            CloseImageCommand = new RelayCommand(_ => CloseImage(), _ => Image != null);
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => Image != null);

            ImageInfoCommand = new RelayCommand(_ =>
            {
                var info = ImageInfoService.GetInfo(Image, CurrentFilePath);
                var window = new ImageInfoWindow(info);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }, _ => Image != null);

            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
            AboutCommand = new RelayCommand(_ => About());

            Rotate90Clockwise = new RelayCommand(_ => RotateImage(90,true), _ => Image != null);
            Rotate90CounterClockwise = new RelayCommand(_ => RotateImage(90, false), _ => Image != null);
            Rotate180 = new RelayCommand(_ => RotateImage(180, true), _ => Image != null);

            FlipHorizontal = new RelayCommand(_ => FlipImage(true), _ => Image != null);
            FlipVertical = new RelayCommand(_ => FlipImage(false), _ => Image != null);

            GaussianBlurCommand = new RelayCommand(_ => OpenFilterWindow<BlurWindow>(img => new BlurViewModel(img)), _ => Image != null);
            SharpenCommand = new RelayCommand(_ => OpenFilterWindow<SharpenWindow>(img => new SharpenViewModel(img)), _ => Image != null);
            BrightnessCommand = new RelayCommand(_ => OpenFilterWindow<BrightnessWindow>(img => new BrightnessViewModel(img)), _ => Image != null);
            GrayscaleCommand = new RelayCommand(_ => OpenFilterWindow<GrayscaleWindow>(img => new GrayscaleViewModel(img)), _ => Image != null);
            SobelCommand = new RelayCommand(_ => OpenFilterWindow<SobelWindow>(img => new SobelViewModel(img)), _ => Image != null);

            InvertCommand = new RelayCommand(_ => {
                    SaveState();
                    Image = InvertHelper.ApplyInvert(Image);
                }, _ => Image != null);

            PixelateCommand = new RelayCommand(_ => OpenFilterWindow<PixelateWindow>(img => new PixelateViewModel(img)), _ => Image != null);
            GammaCommand = new RelayCommand(_ => OpenFilterWindow<GammaWindow>(img => new GammaViewModel(img)), _ => Image != null);


            MinimizeCommand = new RelayCommand(_ => MinimizeWindow());
            MaximizeRestoreCommand = new RelayCommand(_ => MaximizeRestoreWindow());
            CloseCommand = new RelayCommand(_ => CloseWindow());

            MouseWheelCommand = new RelayCommand(parameter =>
            {
                var args = parameter as MouseWheelEventArgs;
                if (args == null) return;

                var element = args.Source as FrameworkElement;
                if (element == null) return;

                var mousePos = args.GetPosition(element);
                double zoomFactor = args.Delta > 0 ? 1.1 : 0.9;
                double oldZoom = Zoom;
                double newZoom = oldZoom * zoomFactor;

                if (newZoom < 0.1 || newZoom > 5) return;

                ImageOffsetX = mousePos.X - (mousePos.X - ImageOffsetX) * (newZoom / oldZoom);
                ImageOffsetY = mousePos.Y - (mousePos.Y - ImageOffsetY) * (newZoom / oldZoom);

                Zoom = newZoom;
            });
        }

        // ================= METHODS =================

        #region TOOLS
        private void RotateImage(int angle, bool clockwise)
        {
            if (Image == null)
            {
                MessageBox.Show("No image loaded", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveState();

            if (angle != 90 && angle != 180 && angle != 270)
                throw new ArgumentException("Angle must be 90, 180 or 270");

            int finalAngle = clockwise ? angle : -angle;

            var transform = new RotateTransform(finalAngle);
            Image = new TransformedBitmap(Image, transform);
        }

        private void FlipImage(bool horizontal)
        {
            if (Image == null)
            {
                MessageBox.Show("No image loaded", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveState();

            var transform = new ScaleTransform(horizontal ? -1 : 1, horizontal ? 1 : -1);
            Image = new TransformedBitmap(Image, transform);
        }

        private void OpenFilterWindow<TWindow>(
            Func<WriteableBitmap, dynamic> viewModelFactory)
            where TWindow : Window, new()
        {
            if (Image == null)
            {
                MessageBox.Show("No image loaded", "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var writeable = Image as WriteableBitmap ?? new WriteableBitmap(Image);
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
                    Image = vm.ResultImage;
                }

                window.Close();
            });

            window.ShowDialog();
        }

        #endregion

        #region FILE
        private void OpenImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image = bitmap;

                ImageOffsetX = 0;
                ImageOffsetY = 0;

                CurrentFilePath = dialog.FileName;
                StatusText = $"Loaded: {CurrentFilePath}";
            }
        }

        private void CloseImage()
        {
            if (Image == null)
                return;

            Image = null;

            _undoStack.Clear();
            _redoStack.Clear();

            Zoom = 1.0;
            _imageOffsetX = 0;
            _imageOffsetY = 0;
            StatusText = "No image loaded";
        }

        private void SaveImage()
        {
            if (Image == null)
            {
                MessageBox.Show("No image loaded", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                SaveImageHelper.SaveToFile(_currentFilePath,_image);
                StatusText = $"Saved: {_currentFilePath}";
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save Image",
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                DefaultExt = ".png"
            };

            if (dialog.ShowDialog() == true)
            {
                _currentFilePath = dialog.FileName;
                SaveImageHelper.SaveToFile(_currentFilePath, _image);
                StatusText = $"Saved: {_currentFilePath}";
            }
        }
        #endregion

        private void SaveState()
        {
            if (Image != null)
            {
                _undoStack.Push(CloneBitmap(Image));
                _redoStack.Clear();
            }
        }

        private BitmapSource CloneBitmap(BitmapSource source)
        {
            return new WriteableBitmap(source);
        }

        #region EDIT
        private void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(CloneBitmap(Image));
                Image = _undoStack.Pop();
            }
        }

        private void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(CloneBitmap(Image));
                Image = _redoStack.Pop();
            }
        }
        #endregion

        private void About()
        {
            MessageBox.Show(
                "MonoFrame\nSimple WPF Image Editor\n\nMVVM Architecture",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }


        public void StartDrag(Point startPoint)
        {
            _dragStart = startPoint;
        }

        public void DragTo(Point currentPoint)
        {
            if (_dragStart == null) return;

            double deltaX = currentPoint.X - _dragStart.Value.X;
            double deltaY = currentPoint.Y - _dragStart.Value.Y;

            ImageOffsetX += deltaX;
            ImageOffsetY += deltaY;

            _dragStart = currentPoint;
        }

        public void EndDrag()
        {
            _dragStart = null;
        }

        private Point? _dragStart = null;

        // ================= WINDOW CONTROL =================

        private void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow()
        {
            var window = Application.Current.MainWindow;

            window.WindowState =
                window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseWindow()
        {
            Application.Current.MainWindow.Close();
        }
    }
}
