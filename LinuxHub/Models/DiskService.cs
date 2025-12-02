using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LinuxHub.Models
{
    class DiskService
    {
        public void CreatePartition(int sizeInGb)
        {
            string script = $@"
select volume C
shrink desired={sizeInGb * 1024}
create partition primary size={sizeInGb * 1024}
assign letter=Z
exit";

            File.WriteAllText("diskpart_script.txt", script);

            Process.Start(new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                Arguments = "/s diskpart_script.txt",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            });
        }
    }
}
