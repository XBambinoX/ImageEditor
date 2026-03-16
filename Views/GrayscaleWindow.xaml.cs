using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class GrayscaleWindow : Window
    {
        public GrayscaleWindow()
        {
            InitializeComponent();
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is GrayscaleViewModel vm)
            {
                vm.Intensity += e.Delta > 0 ? 1 : -1;
                e.Handled = true;
            }
        }
    }
}
