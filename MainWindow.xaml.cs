using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point clickPosition;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Toolbar Methods
        public void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Open Image";
            dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ImageDisplay.Source = bitmap;

                StatusText.Text = $"Loaded: {dialog.FileName}";
            }
        }

        public void SaveImage_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Undo_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Redo_Click(object sender, RoutedEventArgs e)
        {

        }

        public void About_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Titlebar Methods
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestore();
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestore();
        }

        private void MaximizeRestore()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Image Container Methods
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ImageDisplay.Source == null)
                return;

            isDragging = true;
            clickPosition = e.GetPosition(this);

            ImageDisplay.CaptureMouse();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            Point currentPosition = e.GetPosition(this);

            double offsetX = currentPosition.X - clickPosition.X;
            double offsetY = currentPosition.Y - clickPosition.Y;

            ImageTranslate.X += offsetX;
            ImageTranslate.Y += offsetY;

            clickPosition = currentPosition;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ImageDisplay.ReleaseMouseCapture();
        }

        #endregion

    }
}