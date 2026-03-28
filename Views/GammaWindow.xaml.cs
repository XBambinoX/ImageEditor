using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class GammaWindow : Window
    {
        public GammaWindow()
        {
            InitializeComponent();
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is GammaViewModel vm)
            {
                vm.Gamma += e.Delta > 0 ? 0.05 : -0.05;
                e.Handled = true;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
