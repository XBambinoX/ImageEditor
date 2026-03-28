using ImageEditor.ViewModels;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace ImageEditor.Models
{
    public class ImageTab : BaseViewModel
    {
        private WriteableBitmap _image;
        public WriteableBitmap Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(); }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }

        public string DisplayTitle => IsModified ? $"{Title} *" : Title;
        public string FilePath { get; set; }

        public Stack<WriteableBitmap> UndoStack { get; } = new Stack<WriteableBitmap>();
        public Stack<WriteableBitmap> RedoStack { get; } = new Stack<WriteableBitmap>();
    }
}