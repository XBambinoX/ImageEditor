using ImageEditor.Services.Math;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public partial class BrushSettingsWindow : Window
    {
        public double BrushHardness => HardnessSlider.Value / 100.0;
        public int BrushSize => (int)SizeSlider.Value;
        private int MaxSize = 100;

        public BrushSettingsWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeLabel != null)
                SizeLabel.Text = ((int)SizeSlider.Value).ToString();
        }

        private void SizeSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SizeSlider.Value = Tools.Clamp(SizeSlider.Value + (e.Delta > 0 ? 1 : -1), 1, MaxSize);
            e.Handled = true;
        }

        private void SizeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryApplySize();
        }

        private void SizeInput_LostFocus(object sender, RoutedEventArgs e)
        {
            TryApplySize();
        }

        private void TryApplySize()
        {
            if (int.TryParse(SizeLabel.Text, out int val))
                SizeSlider.Value = Tools.Clamp(val, 1, MaxSize);
            else
                SizeLabel.Text = ((int)SizeSlider.Value).ToString();
        }

        #region Hardness
        private void HardnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HardnessLabel != null)
                HardnessLabel.Text = ((int)HardnessSlider.Value).ToString();
        }

        private void HardnessSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            HardnessSlider.Value = Tools.Clamp(HardnessSlider.Value + (e.Delta > 0 ? 1 : -1), 0, 100);
            e.Handled = true;
        }

        private void HardnessInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryApplyHardness();
        }

        private void HardnessInput_LostFocus(object sender, RoutedEventArgs e)
        {
            TryApplyHardness();
        }

        private void TryApplyHardness()
        {
            if (int.TryParse(HardnessLabel.Text, out int val))
                HardnessSlider.Value = Tools.Clamp(val, 0, 100);
            else
                HardnessLabel.Text = ((int)HardnessSlider.Value).ToString();
        }
        #endregion
    }
}