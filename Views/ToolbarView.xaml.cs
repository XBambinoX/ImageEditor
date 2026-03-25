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
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is MainViewModel vm)
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(MainViewModel.ActiveTool))
                            UpdateActiveButton(vm);
                    };
            };
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

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectLineToolCommand.Execute(null);
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

            LineButton.Style = vm.ActiveTool == ToolType.Line
                ? (Style)FindResource("ActiveToolButtonStyle")
                : (Style)FindResource("ToolButtonStyle");
        }
    }
}