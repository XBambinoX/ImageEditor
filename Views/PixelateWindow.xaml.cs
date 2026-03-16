using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class PixelateWindow : Window
    {
        public PixelateWindow()
        {
            InitializeComponent();
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is PixelateViewModel vm)
            {
                vm.BlockSize += e.Delta > 0 ? 1 : -1;
                e.Handled = true;
            }
        }
    }
}
