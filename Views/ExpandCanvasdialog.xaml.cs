using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class ExpandCanvasdialog : Window
    {
        public bool Confirmed { get; private set; } = false;

        public ExpandCanvasdialog(int pasteW, int pasteH, int canvasW, int canvasH)
        {
            InitializeComponent();
            PasteSizeRun.Text = $"{pasteW}×{pasteH}";
            CanvasSizeRun.Text = $"{canvasW}×{canvasH}";
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();
    }
}
