using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Platform.Interfaces
{
    public interface IProcessActivator
    {
        bool ActivateWindow(Process? process);
        int GetWindowProcessId(Process initialProcess, string path);
    }
}
