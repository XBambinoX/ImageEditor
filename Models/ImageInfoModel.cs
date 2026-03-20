namespace ImageEditor.Models
{
    public class ImageInfoModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Extension { get; set; }
        public string FileSize { get; set; }
        public string Dimensions { get; set; }
        public string DpiX { get; set; }
        public string DpiY { get; set; }
        public string Format { get; set; }
        public string BitDepth { get; set; }
        public string MemorySize { get; set; }
        public string Created { get; set; }
        public string Modified { get; set; }
    }
}