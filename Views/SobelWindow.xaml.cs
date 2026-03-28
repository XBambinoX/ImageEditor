using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class SobelWindow : Window
    {
        public SobelWindow()
        {
            InitializeComponent();
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is SobelViewModel vm)
            {
                vm.Threshold += e.Delta > 0 ? 1 : -1;
                e.Handled = true;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
