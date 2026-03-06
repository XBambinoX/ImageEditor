using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class SharpenWindow : Window
    {
        public SharpenWindow()
        {
            InitializeComponent();
        }

        private void Strength_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is SharpenViewModel vm)
            {
                vm.Strength += e.Delta > 0 ? 1 : -1;
            }
        }

        private void Radius_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is SharpenViewModel vm)
            {
                vm.Radius += e.Delta > 0 ? 1 : -1;
            }
        }
    }
}
