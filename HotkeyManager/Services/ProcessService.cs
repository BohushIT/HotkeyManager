using HotkeyManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Services
{
    public class ProcessService : IProcessService
    {
        private readonly Dictionary<string, Process> _runningProcesses = new Dictionary<string, Process>();

        public bool IsProcessRunning(string programPath)
        {
            if (_runningProcesses.ContainsKey(programPath) && !_runningProcesses[programPath].HasExited)
            {
                return true;
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
                    _runningProcesses[path] = process;

                }
            }
            catch (Exception ex)
            {               
                File.AppendAllText("hotkeymanager_log.txt", $"Помилка при запуску {path}: {ex.Message}\n");
                throw new InvalidOperationException($"Не вдалося відкрити {path}: {ex.Message}", ex);
            }
        }

        public void ShutdownAll()
        {
            foreach (var process in _runningProcesses.Values)
            {
                if (!process.HasExited)
                {
                    process.Dispose();
                }
            }
            _runningProcesses.Clear();
        }
    }
}
