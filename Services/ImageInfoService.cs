using ImageEditor.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageEditor.Services
{
    public static class ImageInfoService
    {
        public static ImageInfoModel GetInfo(BitmapSource bitmap, string filePath)
        {
            var info = new ImageInfoModel();
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                info.FileName = fi.Name;
                info.FilePath = fi.FullName;
                info.Extension = fi.Extension.ToUpper().TrimStart('.');
                info.FileSize = FormatBytes(fi.Length);
                info.Created = fi.CreationTime.ToString("dd.MM.yyyy  HH:mm");
                info.Modified = fi.LastWriteTime.ToString("dd.MM.yyyy  HH:mm");
            }
            else
            {
                info.FileName = "Unsaved file";
                info.FilePath = "—";
                info.Extension = "—";
                info.FileSize = "—";
                info.Created = "—";
                info.Modified = "—";
            }

            if (bitmap != null)
            {
                int w = bitmap.PixelWidth;
                int h = bitmap.PixelHeight;
                int bpp = bitmap.Format.BitsPerPixel;

                info.Dimensions = $"{w} × {h} px";
                info.DpiX = $"{bitmap.DpiX:F0}";
                info.DpiY = $"{bitmap.DpiY:F0}";
                info.Format = bitmap.Format.ToString();
                info.BitDepth = $"{bpp} bit";
                info.MemorySize = FormatBytes((long)w * h * (bpp / 8));
            }

            return info;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }
    }
}