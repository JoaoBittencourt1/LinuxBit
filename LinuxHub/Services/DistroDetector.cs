using LinuxHub.Models;
using System;
using System.Collections.Generic;
using System.Text;
using LinuxHub.Models;
using System.Collections.Generic;
using System.IO;

namespace LinuxHub.Services
{
    public class DistroDetector
    {
        private readonly Dictionary<string, DistroInfo> _distros =
            new()
    {
        // Ubuntu base
        {
            "ubuntu",
            new DistroInfo
            {
                Name = "Ubuntu",
                ImagePath = "Assets/Images/ubuntu.png",
            }
        },
        {
            "kubuntu",
            new DistroInfo
            {
                Name = "Kubuntu",
                ImagePath = "Assets/Images/kubuntu.png",
            }
        },
        {
            "xubuntu",
            new DistroInfo
            {
                Name = "Xubuntu",
                ImagePath = "Assets/Images/xubuntu.png",
            }
        },

        // Debian based
        {
            "mint",
            new DistroInfo
            {
                Name = "Linux Mint",
                ImagePath = "Assets/Images/mint.png",
            }
        },
        {
            "zorin",
            new DistroInfo
            {
                Name = "Zorin OS",
                ImagePath = "Assets/Images/zorin.png",
            }
        },
        {
            "pop",
            new DistroInfo
            {
                Name = "Pop!_OS",
                ImagePath = "Assets/Images/popos.png",
            }
        },

        // Fedora / Red Hat
        {
            "fedora",
            new DistroInfo
            {
                Name = "Fedora",
                ImagePath = "Assets/Images/fedora.png",
            }
        },

        // Arch based
        {
            "arch",
            new DistroInfo
            {
                Name = "Arch Linux",
                ImagePath = "Assets/Images/arch.png",
            }
        },
        {
            "manjaro",
            new DistroInfo
            {
                Name = "Manjaro",
                ImagePath = "Assets/Images/manjaro.png",
            }
        },
        {
            "endeavour",
            new DistroInfo
            {
                Name = "EndeavourOS",
                ImagePath = "Assets/Images/endeavouros.png",
            }
        },

        {
            "kali",
            new DistroInfo
            {
                Name = "Kali Linux",
                ImagePath = "Assets/Images/kali.png",
            }
        },

        {
            "chrome",
            new DistroInfo
            {
                Name = "Chrome OS Flex",
                ImagePath = "Assets/Images/chromeos.png",
            }
        }
    };

        public DistroInfo Detect(string isoPath)
        {
            var fileName = Path.GetFileName(isoPath).ToLower();

            foreach (var distro in _distros)
            {
                if (fileName.Contains(distro.Key))
                    return distro.Value;
            }

            return new DistroInfo
            {
                Name = "Distribuição desconhecida",
                Description = "Não foi possível identificar a distro",
                ImagePath = "Assets/Images/linux.png"
            };
        }
    }
}
