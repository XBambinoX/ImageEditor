using System.Windows;
using System.Windows.Controls;
using ImageEditor.ViewModels;

namespace ImageEditor.Views
{
    public partial class ToolbarView : UserControl
    {
        public ToolbarView()
        {
            InitializeComponent();
        }

        private void BrushButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectBrushCommand.Execute(null);
        }
    }
}