using HotkeyManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GetJsonFilePath()
        {
            // Отримуємо базову директорію для даних програми
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Отримуємо назву додатку та файлу з конфігурації
            string appFolder = _configuration["AppSettings:Appfolder"] ?? "HotkeyManager";
            string fileName = _configuration["AppSettings:JsonFileName"] ?? "hotkeys.json";

            // Формуємо повний шлях
            string filePath = Path.Combine(appDataPath, appFolder, fileName);

            // Створюємо директорію, якщо її немає
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return filePath;
        }
    }
}
