using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxHub.Models
{
    class Distro
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Pros { get; set; }
        public string Cons { get; set; }
        public string LogoPath { get; set; }
        public string IsoDownloadUrl { get; set; }
        public string InstallerType { get; set; }
    }
}
