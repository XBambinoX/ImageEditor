using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public partial class BrushSettingsWindow : Window
    {
        public Color SelectedColor { get; private set; } = Colors.Black;
        public int BrushSize => (int)SizeSlider.Value;

        public BrushSettingsWindow()
        {
            InitializeComponent();
        }

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
                SelectedColor = brush.Color;
                SwatchBrush.Color = SelectedColor;
            }

            ColorPopup.IsOpen = false;
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeLabel != null)
                SizeLabel.Text = ((int)SizeSlider.Value).ToString();
        }
    }
}