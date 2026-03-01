using ImageEditor.Commands;
using ImageEditor.ViewModels;
using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;

public class BlurViewModel : BaseViewModel
{
    private WriteableBitmap _original;
    private WriteableBitmap _preview;

    private int _radius = 10;

    public WriteableBitmap PreviewImage
    {
        get => _preview;
        set { _preview = value; OnPropertyChanged(); }
    }

    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            OnPropertyChanged();
        }
    }

    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }

    public Action<bool> CloseAction;

    public BlurViewModel(WriteableBitmap source)
    {
        _original = source;
        PreviewImage = source;

        ApplyCommand = new RelayCommand(_ => CloseAction?.Invoke(true));
        CancelCommand = new RelayCommand(_ => CloseAction?.Invoke(false));
    }
}