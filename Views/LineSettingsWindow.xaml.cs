using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public enum LineMode { Straight, Bezier }

    public partial class LineSettingsWindow : Window
    {
        public Color SelectedColor { get; private set; } = Colors.Black;
        public int LineWidth => (int)WidthSlider.Value;
        public LineMode Mode => BezierMode.IsChecked == true ? LineMode.Bezier : LineMode.Straight;

        public LineSettingsWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
        {
            ColorPopup.IsOpen = true;
        }

        private void Color_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Background is SolidColorBrush brush)
            {
                ApplyColor(brush.Color);
            }
            ColorPopup.IsOpen = false;
        }

        private void WidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WidthLabel != null)
                WidthLabel.Text = ((int)WidthSlider.Value).ToString();
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
    }
}