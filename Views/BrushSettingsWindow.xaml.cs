using ImageEditor.Services.Math;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public partial class BrushSettingsWindow : Window
    {
        public Color SelectedColor { get; private set; } = Colors.Black;
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

        private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
        {
            ColorPopup.IsOpen = true;
        }

        private void Color_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.Background is SolidColorBrush brush)
            {
                ApplyColor(brush.Color);
            }

            ColorPopup.IsOpen = false;
        }

        private void HexInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryApplyHex();
        }

        private void HexInput_LostFocus(object sender, RoutedEventArgs e)
        {
            TryApplyHex();
        }

        private void TryApplyHex()
        {
            var text = HexInput.Text.Trim();
            if (!text.StartsWith("#"))
                text = "#" + text;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(text);
                ApplyColor(color);
            }
            catch
            {
                HexInput.Text = ColorToHex(SelectedColor);
            }
        }

        private void ApplyColor(Color color)
        {
            SelectedColor = color;
            SwatchBrush.Color = color;
            HexInput.Text = ColorToHex(color);
        }

        private string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

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
    }
}