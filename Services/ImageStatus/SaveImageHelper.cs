using System.Windows.Media.Imaging;

namespace ImageEditor.Services.ImageStatus
{
    internal static class SaveImageHelper
    {
        public static void SaveToFile(string path, BitmapSource Image)
        {
            BitmapEncoder encoder;

            string extension = System.IO.Path.GetExtension(path).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;

                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }

            encoder.Frames.Add(BitmapFrame.Create(Image));

            using (var stream = System.IO.File.Create(path))
            {
                encoder.Save(stream);
            }
        }
    }
}
