using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public partial class ColorPickerWindow : Window
    {
        public Color SelectedColor { get; private set; }
        public event Action<Color> ColorChanged;

        public ColorPickerWindow(Color initialColor)
        {
            InitializeComponent();
            ApplyColor(initialColor);
        }   

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ColorSwatch_Click(object sender, MouseButtonEventArgs e)
            => ColorPopup.IsOpen = true;

        private void Color_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Background is SolidColorBrush brush)
                ApplyColor(brush.Color);
            ColorPopup.IsOpen = false;
        }

        private void HexInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TryApplyHex();
        }

        private void HexInput_LostFocus(object sender, RoutedEventArgs e) => TryApplyHex();

        private void TryApplyHex()
        {
            var text = HexInput.Text.Trim();
            if (!text.StartsWith("#")) text = "#" + text;
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
            ColorChanged?.Invoke(color);
        }

        private string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}