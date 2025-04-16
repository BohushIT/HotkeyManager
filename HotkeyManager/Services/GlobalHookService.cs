using SharpHook;
using SharpHook.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Services
{
    public class GlobalHookService : IDisposable
    {
        private static readonly GlobalHookService _instance = new GlobalHookService();

        private readonly TaskPoolGlobalHook _hook;


        private GlobalHookService()
        {
            _hook = new TaskPoolGlobalHook(
                            globalHookType: GlobalHookType.Keyboard,                           
                            runAsyncOnBackgroundThread: true 
                        );
            _hook.KeyPressed += Hook_KeyPressed; 
            _hook.KeyReleased += Hook_KeyReleased;

            _hook.RunAsync(); 
      
        }
        

        public static GlobalHookService Instance => _instance;
        public event EventHandler<KeyboardHookEventArgs>? KeyPressed;
        public event EventHandler<KeyboardHookEventArgs>? KeyReleased;

        private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            KeyPressed?.Invoke(this, e); 
        }


        private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            KeyReleased?.Invoke(this, e);
        }

 
        public void Dispose()
        {
            _hook.KeyPressed -= Hook_KeyPressed;
            _hook.KeyReleased -= Hook_KeyReleased;
            _hook.Dispose();
            Debug.WriteLine("Все очистилось");
        }
    }
}
