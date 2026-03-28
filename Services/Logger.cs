using System;
using System.IO;
using System.Linq;

namespace ImageEditor.Services
{
    public static class Logger
    {
        private static readonly string LogDir = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Log");

        private static readonly string LogPath;
        private static readonly object _lock = new object();
        private const int MaxLogFiles = 10;

        static Logger()
        {
            Directory.CreateDirectory(LogDir);
            CleanOldLogs();

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            LogPath = Path.Combine(LogDir, $"MonoFrame_{timestamp}.log");
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Warning(string message) => Write("WARN", message);
        public static void Error(string message, Exception ex = null) =>
            Write("ERROR", ex != null ? $"{message} | {ex.Message}\n{ex.StackTrace}" : message);

        private static void Write(string level, string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (_lock)
            {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            System.Diagnostics.Debug.WriteLine(line);
        }

        private static void CleanOldLogs()
        {
            var files = Directory.GetFiles(LogDir, "MonoFrame_*.log")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            if (files.Count >= MaxLogFiles)
            {
                foreach (var file in files.Skip(MaxLogFiles - 1))
                {
                    try { File.Delete(file); }
                    catch { }
                }
            }
        }
    }
}