using ImageEditor.Models;
using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
            {
                vm.SelectBrushCommand.Execute(null);
                UpdateActiveButton(vm);
            }
        }

        private void SelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectSelectionToolCommand.Execute(null);
                UpdateActiveButton(vm);
            }
        }

        private void UpdateActiveButton(MainViewModel vm)
        {
            BrushButton.Style = vm.ActiveTool == ToolType.Brush
                ? (Style)FindResource("ActiveToolButtonStyle")
                : (Style)FindResource("ToolButtonStyle");

            SelectionButton.Style = vm.ActiveTool == ToolType.Selection
                ? (Style)FindResource("ActiveToolButtonStyle")
                : (Style)FindResource("ToolButtonStyle");
        }
    }
}