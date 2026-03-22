using System.Windows;

namespace ImageEditor.Views
{
    public enum UnsavedChangesResult { Yes, No, Cancel }

    public partial class UnsavedChangesDialog : Window
    {
        public UnsavedChangesResult Result { get; private set; } = UnsavedChangesResult.Cancel;

        public UnsavedChangesDialog(string fileName)
        {
            InitializeComponent();
            FileNameRun.Text = fileName;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = UnsavedChangesResult.Yes;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = UnsavedChangesResult.No;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = UnsavedChangesResult.Cancel;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}