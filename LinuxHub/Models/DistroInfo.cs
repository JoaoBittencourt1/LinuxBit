using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxHub.Models
{
    public class DistroInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string DownloadLink { get; set; }
        public string[] CarouselImages { get; set; }
    }
}
