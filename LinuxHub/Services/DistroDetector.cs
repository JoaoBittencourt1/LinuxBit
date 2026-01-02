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
        private readonly Dictionary<string, DistroInfo> _distros = new()
{
    // Ubuntu base
    {
        "ubuntu",
        new DistroInfo
        {
            Id = "ubuntu",
            Name = "Ubuntu",
            Family = "Debian",
            Version = "24.04.3",
            ImagePath = "Assets/Images/ubuntu.png",
        }
    },
    {
        "kubuntu",
        new DistroInfo
        {
            Id = "kubuntu",
            Name = "Kubuntu",
            Family = "Ubuntu",
            Version = "24.04",
            ImagePath = "Assets/Images/kubuntu.png",
        }
    },
    {
        "xubuntu",
        new DistroInfo
        {
            Id = "xubuntu",
            Name = "Xubuntu",
            Family = "Ubuntu",
            Version = "24.04",
            ImagePath = "Assets/Images/xubuntu.png",
        }
    },

    // Debian based
    {
        "mint",
        new DistroInfo
        {
            Id = "mint",
            Name = "Linux Mint",
            Family = "Debian",
            Version = "21",
            ImagePath = "Assets/Images/mint.png",
        }
    },
    {
        "zorin",
        new DistroInfo
        {
            Id = "zorin",
            Name = "Zorin OS",
            Family = "Ubuntu",
            Version = "16",
            ImagePath = "Assets/Images/zorin.png",
        }
    },
    {
        "pop",
        new DistroInfo
        {
            Id = "pop",
            Name = "Pop!_OS",
            Family = "Ubuntu",
            Version = "24.04",
            ImagePath = "Assets/Images/popos.png",
        }
    },

    // Fedora / Red Hat
    {
        "fedora",
        new DistroInfo
        {
            Id = "fedora",
            Name = "Fedora",
            Family = "Red Hat",
            Version = "40",
            ImagePath = "Assets/Images/fedora.png",
        }
    },

    // Arch based
    {
        "arch",
        new DistroInfo
        {
            Id = "arch",
            Name = "Arch Linux",
            Family = "Arch",
            Version = "2025.12.01",
            ImagePath = "Assets/Images/arch.png",
        }
    },
    {
        "manjaro",
        new DistroInfo
        {
            Id = "manjaro",
            Name = "Manjaro",
            Family = "Arch",
            Version = "25.0",
            ImagePath = "Assets/Images/manjaro.png",
        }
    },
    {
        "endeavour",
        new DistroInfo
        {
            Id = "endeavour",
            Name = "EndeavourOS",
            Family = "Arch",
            Version = "25.0",
            ImagePath = "Assets/Images/endeavouros.png",
        }
    },

    {
        "kali",
        new DistroInfo
        {
            Id = "kali",
            Name = "Kali Linux",
            Family = "Debian",
            Version = "2025.1",
            ImagePath = "Assets/Images/kali.png",
        }
    },

    {
        "chrome",
        new DistroInfo
        {
            Id = "chrome",
            Name = "Chrome OS Flex",
            Family = "ChromiumOS",
            Version = "114",
            ImagePath = "Assets/Images/chromeos.png",
        }
    }
};


        public DistroInfo Detect(string isoPath)
        {
            if (string.IsNullOrWhiteSpace(isoPath) || !File.Exists(isoPath))
            {
                return new DistroInfo
                {
                    Name = "Distribuição desconhecida",
                    Description = "Não foi possível identificar a distro",
                    ImagePath = "Assets/Images/linux.png"
                };
            }

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
