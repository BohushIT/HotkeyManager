using HotkeyManager.Platform.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;

namespace HotkeyManager.Platform.Windows
{
    public class WindowsProcessActivator : IProcessActivator
    {
        public bool ActivateWindow(Process? process)
        {
            if (process == null || process.HasExited)
            {
                return false;
            }

            IntPtr foundHWnd = IntPtr.Zero;

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                NativeMethods.GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                if (windowProcessId == process.Id)
                {
                    foundHWnd = hWnd;
                    return false; // Зупинити перебір
                }
                return true;
            }, IntPtr.Zero);

            if (foundHWnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(foundHWnd, NativeMethods.SW_RESTORE);
                return NativeMethods.SetForegroundWindow(foundHWnd);
            }

            return false;
        }

        public int GetWindowProcessId(Process initialProcess, string path)
        {
            if (initialProcess == null)
            {
                throw new ArgumentNullException(nameof(initialProcess));
            }

            string executableName;
            try
            {
                executableName = initialProcess.ProcessName;
            }
            catch (InvalidOperationException)
            {
                executableName = null;
            }

            Process windowProcess = null;
            bool isInitialProcessActive = true;
            int maxWindowWaitMs = 5000; // Максимальний час очікування 5 секунд
            int checkIntervalMs = 500; // Перевіряти кожні 0.5 секунди
            int waitedTimeMs = 0;

            while (waitedTimeMs < maxWindowWaitMs)
            {
                try
                {
                    if (isInitialProcessActive && !initialProcess.HasExited && initialProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        windowProcess = initialProcess;
                        break;
                    }
                    else if (isInitialProcessActive)
                    {
                        Debug.WriteLine(
                            $"GetWindowProcessId: Новий процес ще не має вікна, HasExited={initialProcess.HasExited}, MainWindowHandle={initialProcess.MainWindowHandle}");
                    }
                }
                catch (InvalidOperationException)
                {
                    isInitialProcessActive = false;
                    Debug.WriteLine("GetWindowProcessId: Новий процес завершився або недоступний");
                }

                if (!string.IsNullOrEmpty(executableName))
                {
                    var processes = Process.GetProcessesByName(executableName);
                    windowProcess = processes.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

                    if (windowProcess != null)
                    {
                        foreach (var p in processes.Where(p => p != windowProcess))
                        {
                            p.Dispose();
                        }
                        break;
                    }
                    else
                    {
                        foreach (var p in processes)
                        {
                            p.Dispose();
                        }
                    }
                }

                System.Threading.Thread.Sleep(checkIntervalMs);
                waitedTimeMs += checkIntervalMs;
            }

            int resultProcessId;
            if (windowProcess != null)
            {
                resultProcessId = windowProcess.Id;
                if (windowProcess != initialProcess)
                {
                    windowProcess.Dispose();
                }
            }
            else
            {
                resultProcessId = initialProcess.Id;
            }

            return resultProcessId;
        }
    }
}