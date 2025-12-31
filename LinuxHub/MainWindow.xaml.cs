using LinuxHub.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace LinuxHub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            RegisterDistroClicks();
        }

        #region Window config

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int darkMode = 1;

            // DWMWA_USE_IMMERSIVE_DARK_MODE = 20 (Windows 11)
            DwmSetWindowAttribute(hwnd, 20, ref darkMode, Marshal.SizeOf(typeof(int)));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );

        #endregion

        #region Navigation

        private void OpenDistro(DistroInfo distro)
        {
            var window = new DistroWindow(
                distro.Name,
                distro.Description,
                distro.ImagePath,
                distro.DownloadLink
            );

            window.Owner = this;
            this.Hide();

            if (distro.CarouselImages != null && distro.CarouselImages.Length > 0)
                window.LoadCarousel(distro.CarouselImages);

            window.Show();
        }

        #endregion

        #region Distro registration

        private void RegisterDistroClicks()
        {
            UbuntuPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Ubuntu());
            MintPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Mint());
            ZorinPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Zorin());
            PopPanel.MouseLeftButtonUp += (_, _) => OpenDistro(PopOS());
            FedoraPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Fedora());
            KubuntuPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Kubuntu());
            XubuntuPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Xubuntu());
            ManjaroPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Manjaro());
            ArchPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Arch());
            EndeavourosPanel.MouseLeftButtonUp += (_, _) => OpenDistro(EndeavourOS());
            KaliPanel.MouseLeftButtonUp += (_, _) => OpenDistro(Kali());
            ChromeosPanel.MouseLeftButtonUp += (_, _) => OpenDistro(ChromeOS());
        }

        #endregion

        #region Distros (dados)

        private DistroInfo Ubuntu() => new()
        {
            Name = "Ubuntu",
            Description = "O Ubuntu é uma das distribuições Linux mais populares e amigáveis...",
            ImagePath = "pack://application:,,,/Assets/Images/ubuntu.png",
            DownloadLink = "https://ubuntu.com/download/desktop",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Ubuntu/ubuntu1.jpg",
                "pack://application:,,,/Assets/Images/Ubuntu/ubuntu2.png"
            }
        };

        private DistroInfo Mint() => new()
        {
            Name = "Linux Mint",
            Description = "O Linux Mint é focado em usuários vindos do Windows...",
            ImagePath = "pack://application:,,,/Assets/Images/mint.png",
            DownloadLink = "https://linuxmint.com/download.php",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Mint/Mint1.png",
                "pack://application:,,,/Assets/Images/Mint/mint2.png"
            }
        };

        private DistroInfo Zorin() => new()
        {
            Name = "Zorin OS",
            Description = "O Zorin OS é moderno, elegante e acessível...",
            ImagePath = "pack://application:,,,/Assets/Images/zorin.png",
            DownloadLink = "https://zorin.com/os/download/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Zorin/zorin1.png",
                "pack://application:,,,/Assets/Images/Zorin/zorin2.jpg",
                "pack://application:,,,/Assets/Images/Zorin/zorin3.jpg"
            }
        };

        private DistroInfo PopOS() => new()
        {
            Name = "Pop!_OS",
            Description = "Distribuição focada em produtividade e desempenho...",
            ImagePath = "pack://application:,,,/Assets/Images/popos.png",
            DownloadLink = "https://system76.com/pop/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/PopOs/pop1.png",
                "pack://application:,,,/Assets/Images/PopOs/pop2.png"
            }
        };

        private DistroInfo Fedora() => new()
        {
            Name = "Fedora",
            Description = "Distribuição moderna e inovadora...",
            ImagePath = "pack://application:,,,/Assets/Images/fedora.png",
            DownloadLink = "https://www.fedoraproject.org/pt-br/workstation/download",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Fedora/fedora1.jpg",
                "pack://application:,,,/Assets/Images/Fedora/fedora2.jpg",
                "pack://application:,,,/Assets/Images/Fedora/fedora3.jpg"
            }
        };

        private DistroInfo Kubuntu() => new()
        {
            Name = "Kubuntu",
            Description = "Ubuntu com KDE Plasma...",
            ImagePath = "pack://application:,,,/Assets/Images/Kubuntu.png",
            DownloadLink = "https://kubuntu.org/archives/getkubuntu.html",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Kubuntu/Kubuntu1.png"
            }
        };

        private DistroInfo Xubuntu() => new()
        {
            Name = "Xubuntu",
            Description = "Distribuição leve baseada no Ubuntu...",
            ImagePath = "pack://application:,,,/Assets/Images/Xubuntu.png",
            DownloadLink = "https://xubuntu.org/download/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Xubuntu/xubuntu.png"
            }
        };

        private DistroInfo Manjaro() => new()
        {
            Name = "Manjaro",
            Description = "Baseado em Arch com facilidade de uso...",
            ImagePath = "pack://application:,,,/Assets/Images/manjaro.png",
            DownloadLink = "https://manjaro.org/products/download/x86",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Manjaro/manjaro1.jpg",
                "pack://application:,,,/Assets/Images/Manjaro/manjaro2.jpg"
            }
        };

        private DistroInfo Arch() => new()
        {
            Name = "Arch Linux",
            Description = "Distribuição minimalista e altamente personalizável...",
            ImagePath = "pack://application:,,,/Assets/Images/arch.png",
            DownloadLink = "https://archlinux.org/download/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Arch/arch1.png",
                "pack://application:,,,/Assets/Images/Arch/arch2.png",
                "pack://application:,,,/Assets/Images/Arch/arch3.png",
                "pack://application:,,,/Assets/Images/Arch/arch4.png"
            }
        };

        private DistroInfo EndeavourOS() => new()
        {
            Name = "EndeavourOS",
            Description = "Arch guiado e acessível...",
            ImagePath = "pack://application:,,,/Assets/Images/endeavouros.png",
            DownloadLink = "https://endeavouros.com/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/End/end1.jpg",
                "pack://application:,,,/Assets/Images/End/end2.png",
                "pack://application:,,,/Assets/Images/End/end3.jpeg"
            }
        };

        private DistroInfo Kali() => new()
        {
            Name = "Kali Linux",
            Description = "Distribuição para segurança ofensiva...",
            ImagePath = "pack://application:,,,/Assets/Images/kali.png",
            DownloadLink = "https://www.kali.org/get-kali/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Kali/kali1.jpg",
                "pack://application:,,,/Assets/Images/Kali/kali2.jpg"
            }
        };

        private DistroInfo ChromeOS() => new()
        {
            Name = "Chrome OS",
            Description = "Sistema leve focado em nuvem...",
            ImagePath = "pack://application:,,,/Assets/Images/chromeos.png",
            DownloadLink = "https://chromeos.google/intl/pt_br/products/chromeos-flex/",
            CarouselImages = new[]
            {
                "pack://application:,,,/Assets/Images/Chrome/Chrome2.png",
                "pack://application:,,,/Assets/Images/Chrome/Chromeos.jpg"
            }
        };

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UserNamePlaceholder.Visibility =
                string.IsNullOrEmpty(UserNameBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }


        #endregion
    }
}
