using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LinuxHub.Models
{
    class BootService
    {
        public void AddBootEntry(string description)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c bcdedit /create /d \"{description}\" /application bootsector",
                Verb = "runas",
                UseShellExecute = true
            });
        }
    }
}
