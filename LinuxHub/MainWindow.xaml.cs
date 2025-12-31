using LinuxHub.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using System.IO;
using System.Management;



namespace LinuxHub
{
    public partial class MainWindow : Window
    {

        private string selectedIsoPath;


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

            DwmSetWindowAttribute(hwnd, 20, ref darkMode, Marshal.SizeOf(typeof(int)));

            LoadPartitions();

            // Exemplo de uso do UEFI
            if (!IsUefi())
            {
                MessageBox.Show(
                    "Seu sistema NÃO está em modo UEFI.\nInstalação automática pode não funcionar.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
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

        private bool ValidarIso(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!File.Exists(path))
                return false;

            var info = new FileInfo(path);

            return info.Length > 700 * 1024 * 1024; // > 700MB
        }

        private bool IsUefi()
        {
            return Directory.Exists(@"C:\Windows\Boot\EFI");
        }

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UserNamePlaceholder.Visibility =
                string.IsNullOrEmpty(UserNameBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Hidden;
        }


        private void BrowseIso_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Selecionar imagem ISO",
                Filter = "Imagem ISO (*.iso)|*.iso",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            var isoPath = dialog.FileName;

            if (!ValidarIso(isoPath))
            {
                MessageBox.Show(
                    "ISO inválida ou muito pequena.\nSelecione uma ISO Linux válida.",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                selectedIsoPath = null;
                IsoPathTextBox.Text = string.Empty;
                return;
            }

            selectedIsoPath = isoPath;
            IsoPathTextBox.Text = isoPath;
        }

        //private void LoadDisks()
        //{
        //    DiskComboBox.Items.Clear();
        //    using var searcher = new ManagementObjectSearcher(
        //        "SELECT Index, Model, Size FROM Win32_DiskDrive"
        //    );

        //    foreach (ManagementObject disk in searcher.Get())
        //    {
        //        if (disk["Size"] == null)
        //            continue;

        //       var diskInfo = new DiskInfo
        //       {
        //           Index = Convert.ToInt32(disk["Index"]),
        //           Model = disk["Model"]?.ToString() ?? "Desconhecido",
        //           SizeBytes = Convert.ToInt64(disk["Size"])
        //       };

        //       DiskComboBox.Items.Add(diskInfo);
        //   }

        //   if (DiskComboBox.Items.Count > 0)
        //       DiskComboBox.SelectedIndex = 0;
        //}

        private void LoadPartitions()
        {
            DiskComboBox.Items.Clear();

            ManagementObjectSearcher partitionSearcher;

            try
            {
                partitionSearcher = new ManagementObjectSearcher(
                    "SELECT DeviceID, DiskIndex, Index, Size, Type FROM Win32_DiskPartition"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar informações de disco:\n" + ex.Message);
                return;
            }

            foreach (ManagementObject partition in partitionSearcher.Get())
            {
                try
                {
                    int diskIndex = Convert.ToInt32(partition["DiskIndex"]);
                    int partIndex = Convert.ToInt32(partition["Index"]);
                    long size = Convert.ToInt64(partition["Size"]);

                    string deviceId = partition["DeviceID"].ToString();

                    string drive = null;
                    string fs = null;
                    bool isSystem = false;

                    // Buscar volumes associados (nem toda partição tem!)
                    using var logicalSearcher = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID=\"{deviceId}\"}} " +
                        "WHERE AssocClass=Win32_LogicalDiskToPartition"
                    );

                    foreach (ManagementObject logical in logicalSearcher.Get())
                    {
                        drive = logical["DeviceID"]?.ToString();
                        fs = logical["FileSystem"]?.ToString();
                        isSystem = logical["BootVolume"] != null && (bool)logical["BootVolume"];
                    }

                    DiskComboBox.Items.Add(new PartitionInfo
                    {
                        DiskIndex = diskIndex,
                        PartitionIndex = partIndex,
                        DriveLetter = drive,
                        FileSystem = fs ?? "Sem sistema de arquivos",
                        SizeBytes = size,
                        IsSystem = isSystem
                    });
                }
                catch
                {
                    // Ignora partições problemáticas (EFI, MSR, Recovery)
                    continue;
                }
            }

            if (DiskComboBox.Items.Count > 0)
                DiskComboBox.SelectedIndex = 0;
        }





        #endregion
    }
}
