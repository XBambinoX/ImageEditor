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
                SelectedColor = brush.Color;
                SwatchBrush.Color = SelectedColor;
            }
            ColorPopup.IsOpen = false;
        }

        private void WidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WidthLabel != null)
                WidthLabel.Text = ((int)WidthSlider.Value).ToString();
        }
    }
}