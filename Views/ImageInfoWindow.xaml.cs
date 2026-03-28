using ImageEditor.Models;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class ImageInfoWindow : Window
    {
        public ImageInfoWindow(ImageInfoModel info)
        {
            InitializeComponent();
            DataContext = info;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}