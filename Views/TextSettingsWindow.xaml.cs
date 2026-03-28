using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageEditor.Views
{
    public partial class TextSettingsWindow : Window
    {
        public string SelectedFont => FontComboBox.SelectedItem as string ?? "Arial";
        public double FontSize => SizeSlider.Value;
        public bool IsBold => BoldButton.IsChecked == true;
        public bool IsItalic => ItalicButton.IsChecked == true;
        public TextAlignment Alignment =>
            AlignCenter.IsChecked == true ? TextAlignment.Center :
            AlignRight.IsChecked == true ? TextAlignment.Right :
            TextAlignment.Left;

        public event System.Action SettingsChanged;

        public TextSettingsWindow()
        {
            InitializeComponent();

            // Load system fonts and set default selection
            var fonts = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(f => f)
                .ToList();

            FontComboBox.ItemsSource = fonts;
            FontComboBox.SelectedItem = "Arial";
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void FontComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            => SettingsChanged?.Invoke();

        private void Style_Changed(object sender, RoutedEventArgs e)
            => SettingsChanged?.Invoke();

        private void Alignment_Changed(object sender, RoutedEventArgs e)
            => SettingsChanged?.Invoke();

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeLabel != null)
                SizeLabel.Text = ((int)SizeSlider.Value).ToString();
            SettingsChanged?.Invoke();
        }

        private void SizeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TryApplySize();
        }

        private void SizeInput_LostFocus(object sender, RoutedEventArgs e) => TryApplySize();

        private void TryApplySize()
        {
            if (int.TryParse(SizeLabel.Text, out int val))
                SizeSlider.Value = System.Math.Max(6, System.Math.Min(200, val));
            else
                SizeLabel.Text = ((int)SizeSlider.Value).ToString();
        }
    }
}