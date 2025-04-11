using System;
using System.Runtime.InteropServices;

namespace HotkeyManager.Platform.Windows
{
    public class NativeMethods
    {
        // Робить вікно активним
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // Керує станом вікна (розгортає, згортає тощо)
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Вказує, що вікно потрібно розгорнути
        public const int SW_RESTORE = 9;

        // Перебирає всі вікна верхнього рівня
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        // Отримує ProcessId для вікна
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Делегат для EnumWindows
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}