using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class BrightnessWindow : Window
    {
        public BrightnessWindow()
        {
            InitializeComponent();
        }

        private void Brightness_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is BrightnessViewModel vm)
            {
                vm.Brightness += e.Delta > 0 ? 1 : -1;
            }
        }

        private void Contrast_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is BrightnessViewModel vm)
            {
                vm.Contrast += e.Delta > 0 ? 1 : -1;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
