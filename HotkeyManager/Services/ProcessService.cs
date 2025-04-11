using HotkeyManager.Platform.Interfaces;
using HotkeyManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HotkeyManager.Services
{
    public class ProcessService : IProcessService
    {
        private readonly Dictionary<string, int> _runningProcessId = new Dictionary<string, int>();
        private readonly IProcessActivator _activator;

        public ProcessService(IProcessActivator activator)
        {
            _activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        public bool TryActivateWindow(string programPath)
        {
            if (_runningProcessId.TryGetValue(programPath, out int processId))
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    bool result = _activator.ActivateWindow(process);
                    process.Dispose();
                    return result;
                }
                catch (ArgumentException)
                {
                    _runningProcessId.Remove(programPath);
                }
            }
            return false;
        }

        public bool IsProcessRunning(string programPath)
        {
            if (_runningProcessId.TryGetValue(programPath, out int processId))
            {
                try
                {
                    Process.GetProcessById(processId);
                    return true;
                }
                catch (ArgumentException)
                {
                    _runningProcessId.Remove(programPath);
                }
            }
            return false;
        }

        public void StartProcess(string path)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    int processId = process.Id;


                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        processId = _activator.GetWindowProcessId(process, path);
                        _runningProcessId[path] = processId;
                    }
                    else
                    {
                        processId = _activator.GetWindowProcessId(process, path);
                        _runningProcessId[path] = processId;

                    }

                    process.Dispose();
                }
                else
                {
                    Debug.WriteLine($"StartProcess: Не вдалося запустити процес для {path}: Process is null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartProcess: Помилка при запуску {path}: {ex.Message}");
                throw new InvalidOperationException($"Не вдалося відкрити {path}: {ex.Message}", ex);
            }
        }

        public void ShutdownAll()
        {
            _runningProcessId.Clear();
        }
    }
}