using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HotkeyManager.Models
{
    public class Hotkey
    {
        public int Id { get; set; }
        public ModifierMask Modifier { get; set; }  
        public KeyCode Key { get; set; }            
        public string ProgramPath { get; set; } 
        
        public bool RunMultipleInstances { get; set; }

        [JsonConstructor]
        public Hotkey(int id, ModifierMask modifier, KeyCode key, string programPath, bool runMultipleInstances)
        {
            Id = id;
            Modifier = modifier;
            Key = key;
            ProgramPath = programPath;
            RunMultipleInstances = runMultipleInstances; 
        }
        public string KeyString => Key.ToString().Substring(2);
        public string ProgramPathString => Path.GetFileName(ProgramPath);
    }
}
