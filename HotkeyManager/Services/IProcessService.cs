namespace HotkeyManager.Services
{
    public interface IProcessService
    {
        bool IsProcessRunning(string programPath);
        void StartProcess(string programPath);
        void ShutdownAll();
    }
}