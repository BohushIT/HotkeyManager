namespace HotkeyManager.Services.Interfaces
{
    public interface IProcessService
    {
        bool TryActivateWindow(string programPath);
        bool IsProcessRunning(string programPath);
        void StartProcess(string programPath);
        void ShutdownAll();
    }
}