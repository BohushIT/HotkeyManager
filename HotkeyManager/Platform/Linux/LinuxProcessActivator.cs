using HotkeyManager.Platform.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace HotkeyManager.Platform.Linux
{
    public class LinuxProcessActivator : IProcessActivator
    {
        private const int MaxRetries = 3;
        private const int InitialDelayMs = 1000;
        private const int RetryDelayMs = 1500;

        public bool ActivateWindow(Process? process)
        {
            if (process == null || process.HasExited)
            {
                File.AppendAllText("hotkeymanager_log.txt", $"Failed: Process is null or has exited (PID: {process?.Id ?? -1}).\n");
                return false;
            }

            // Спроба X11, якщо MainWindowHandle доступний
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                try
                {
                    IntPtr display = NativeMethods.XOpenDisplay(null);
                    if (display == IntPtr.Zero)
                    {
                        File.AppendAllText("hotkeymanager_log.txt", $"Failed: Unable to open X11 display for {process.ProcessName} (PID: {process.Id}).\n");
                        return false;
                    }

                    try
                    {
                        IntPtr window = process.MainWindowHandle;
                        File.AppendAllText("hotkeymanager_log.txt", $"Starting X11 activation for {process.ProcessName} (PID: {process.Id}, Window Handle: {window}).\n");

                        int mapResult = NativeMethods.XMapWindow(display, window);
                        File.AppendAllText("hotkeymanager_log.txt", $"XMapWindow executed (Result: {mapResult}, 0 = success).\n");

                        int raiseResult = NativeMethods.XRaiseWindow(display, window);
                        File.AppendAllText("hotkeymanager_log.txt", $"XRaiseWindow executed (Result: {raiseResult}, 0 = success).\n");

                        int focusResult = NativeMethods.XSetInputFocus(display, window, NativeMethods.RevertToParent, NativeMethods.CurrentTime);
                        File.AppendAllText("hotkeymanager_log.txt", $"XSetInputFocus executed (Result: {focusResult}, 0 = success).\n");

                        int flushResult = NativeMethods.XFlush(display);
                        File.AppendAllText("hotkeymanager_log.txt", $"XFlush executed (Result: {flushResult}, 0 = success).\n");

                        File.AppendAllText("hotkeymanager_log.txt", $"Success: X11 activation completed for {process.ProcessName} (PID: {process.Id}).\n");
                        return true;
                    }
                    finally
                    {
                        int closeResult = NativeMethods.XCloseDisplay(display);
                        File.AppendAllText("hotkeymanager_log.txt", $"XCloseDisplay executed (Result: {closeResult}, 0 = success).\n");
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("hotkeymanager_log.txt", $"Error during X11 activation for {process.ProcessName} (PID: {process.Id}): {ex.Message}.\n");
                    return false;
                }
            }
            else
            {
                File.AppendAllText("hotkeymanager_log.txt", $"Failed: No valid MainWindowHandle for {process.ProcessName} (PID: {process.Id}, Handle: {process.MainWindowHandle}). Falling back to xdotool.\n");
            }

            Thread.Sleep(InitialDelayMs);

            // Універсальна спроба активації через xdotool за PID
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    File.AppendAllText("hotkeymanager_log.txt", $"Trying xdotool with PID for {process.ProcessName} (PID: {process.Id}, Attempt: {attempt}).\n");

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "xdotool",
                        Arguments = $"search --onlyvisible --pid {process.Id} windowraise windowactivate",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process xdotool = Process.Start(startInfo))
                    {
                        xdotool.WaitForExit();
                        string output = xdotool.StandardOutput.ReadToEnd();
                        string error = xdotool.StandardError.ReadToEnd();

                        if (xdotool.ExitCode == 0)
                        {
                            File.AppendAllText("hotkeymanager_log.txt", $"Success: xdotool activated window for {process.ProcessName} (PID: {process.Id}, Attempt: {attempt}, Output: {output.Trim()}).\n");
                            return true;
                        }
                        else
                        {
                            File.AppendAllText("hotkeymanager_log.txt", $"Failed: xdotool PID search error for {process.ProcessName} (PID: {process.Id}, Attempt: {attempt}): {error}.\n");
                        }

                        if (attempt < MaxRetries)
                        {
                            File.AppendAllText("hotkeymanager_log.txt", $"Retrying xdotool for {process.ProcessName} (PID: {process.Id}).\n");
                            Thread.Sleep(RetryDelayMs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("hotkeymanager_log.txt", $"Error during xdotool PID activation for {process.ProcessName} (PID: {process.Id}, Attempt: {attempt}): {ex.Message}.\n");
                    if (attempt < MaxRetries)
                    {
                        Thread.Sleep(RetryDelayMs);
                        continue;
                    }
                }
            }

            File.AppendAllText("hotkeymanager_log.txt", $"Failed: All attempts to activate window for {process.ProcessName} (PID: {process.Id}) exhausted.\n");
            return false;
        }

        public int GetWindowProcessId(Process initialProcess, string path)
        {
            if (initialProcess == null)
            {
                File.AppendAllText("hotkeymanager_log.txt", $"GetWindowProcessId: Failed: initialProcess is null for path {path}.\n");
                throw new ArgumentNullException(nameof(initialProcess));
            }

            try
            {
                int processId = initialProcess.Id;
                File.AppendAllText("hotkeymanager_log.txt", $"GetWindowProcessId: Success: Returning ProcessId={processId} for {initialProcess.ProcessName} (Path: {path}).\n");
                return processId;
            }
            catch (InvalidOperationException ex)
            {
                File.AppendAllText("hotkeymanager_log.txt", $"GetWindowProcessId: Failed: Process {initialProcess.ProcessName} (Path: {path}) is inaccessible: {ex.Message}.\n");
                throw;
            }
        }
    }
}