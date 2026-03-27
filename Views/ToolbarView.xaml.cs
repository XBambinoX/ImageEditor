using ImageEditor.Models;
using ImageEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                        if (args.PropertyName == nameof(MainViewModel.ActiveColor))
                            ActiveSwatchBrush.Color = vm.ActiveColor;
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

            EyedropperButton.Style = vm.ActiveTool == ToolType.Eyedropper
                ? (Style)FindResource("ActiveToolButtonStyle")
                : (Style)FindResource("ToolButtonStyle");
        }

        private void ActiveColor_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.ToggleColorPickerCommand.Execute(null);
        }

        private void EyedropperButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectEyedropperCommand.Execute(null);
                UpdateActiveButton(vm);
            }
        }
    }
}