using ImageEditor.Commands;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ================= IMAGE =================
        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set
            {
                _image = value;
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

        // ================= COMMANDS =================
        public ICommand OpenImageCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AboutCommand { get; }

        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeRestoreCommand { get; }
        public ICommand CloseCommand { get; }

        // ================= CONSTRUCTOR =================
        public MainViewModel()
        {
            OpenImageCommand = new RelayCommand(_ => OpenImage());
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => Image != null);

            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            UndoCommand = new RelayCommand(_ => Undo(), _ => false);
            RedoCommand = new RelayCommand(_ => Redo(), _ => false);
            AboutCommand = new RelayCommand(_ => About());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow());
            MaximizeRestoreCommand = new RelayCommand(_ => MaximizeRestoreWindow());
            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        // ================= METHODS =================

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

                StatusText = $"Loaded: {dialog.FileName}";
            }
        }

        private void SaveImage()
        {
            MessageBox.Show("Save not implemented yet", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Undo()
        {
            MessageBox.Show("Undo not implemented yet");
        }

        private void Redo()
        {
            MessageBox.Show("Redo not implemented yet");
        }

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
            _dragStart = startPoint; // приватне поле Point _dragStart;
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
