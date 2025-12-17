using System;
using System.IO;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.Utils.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new();

        public FileLogger()
        {
            try
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Locomotiv",
                    "Logs"
                );

                Directory.CreateDirectory(folder);
                _logFilePath = Path.Combine(folder, "locomotiv.log");
            }
            catch
            {
                _logFilePath = string.Empty;
            }
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        public void Warning(string message)
        {
            Write("WARNING", message);
        }

        public void Error(string message, Exception? ex = null)
        {
            var fullMessage = ex == null
                ? message
                : $"{message} | Exception: {ex.Message}";

            Write("ERROR", fullMessage);
        }

        private void Write(string level, string message)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
                return;

            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // Si l'écriture dans le fichier échoue, on ignore silencieusement l'erreur
            }
        }
    }
}
