using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HotkeyManager.Services
{
    public class AutoStartService
    {
        private const string AppName = "HotkeyManager";
        private readonly string _appPath;

        public AutoStartService()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("Автозавантаження через реєстр підтримується лише на Windows.");
            }
            _appPath = Process.GetCurrentProcess().MainModule.FileName;
            Debug.WriteLine(_appPath);
        }

        public bool IsAutoStartEnabled()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    return key != null && key.GetValue(AppName) != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetAutoStart(bool enable)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null)
                        throw new InvalidOperationException("Не вдалося відкрити розділ реєстру Run");

                    if (enable && key.GetValue(AppName) == null)
                    {
                        key.SetValue(AppName, $"\"{_appPath}\"");
                    }
                    else if (!enable && key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Помилка зміни автозавантаження: {ex.Message}", ex);
            }
        }
    }
}