using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class BlurWindow : Window
    {
        public BlurWindow()
        {
            InitializeComponent();
        }

        private void Radius_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is BlurViewModel vm)
            {
                vm.Radius += e.Delta > 0 ? 1 : -1;
            }
        }
    }
}
