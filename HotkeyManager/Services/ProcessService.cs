using HotkeyManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void StartProcess(string programPath)
        {
            //if (IsProcessRunning(programPath))
            //{
            //    return; // Процес уже запущений
            //}

            var process = Process.Start(programPath);
            if (process != null)
            {
                _runningProcesses[programPath] = process;
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
