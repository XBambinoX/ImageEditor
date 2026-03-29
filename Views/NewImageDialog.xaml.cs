using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class NewImageDialog : Window
    {
        public bool Confirmed { get; private set; } = false;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }

        public NewImageDialog()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthBox.Text, out int w) || w <= 0 ||
                !int.TryParse(HeightBox.Text, out int h) || h <= 0)
            {
                WidthBox.BorderBrush = System.Windows.Media.Brushes.IndianRed;
                HeightBox.BorderBrush = System.Windows.Media.Brushes.IndianRed;
                return;
            }

            ImageWidth = w;
            ImageHeight = h;
            Confirmed = true;

            long estimatedMB = (long)w * h * 3 / 1024 / 1024;
            if (estimatedMB > 100)
            {
                var result = MessageBox.Show(
                    $"This image will use approximately {estimatedMB} MB of memory.\nContinue?",
                    "Large Image",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return;
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$"); // Handle non-digit input
        }
    }
}