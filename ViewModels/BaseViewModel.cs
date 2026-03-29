using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public abstract class BaseFilterViewModel : BaseViewModel
    {
        public WriteableBitmap _original;
        public CancellationTokenSource _cts;

        public WriteableBitmap ResultImage { get; protected set; }
        public Action<bool> CloseAction;

        private WriteableBitmap _preview;
        public WriteableBitmap PreviewImage
        {
            get => _preview;
            set { _preview = value; OnPropertyChanged(); }
        }

        public void Cleanup()
        {
            PreviewImage = null;
            ResultImage = null;
        }
    }
}
