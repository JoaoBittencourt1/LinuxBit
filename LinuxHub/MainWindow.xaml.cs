using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LinuxHub
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UbuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Ubuntu";
                string description = "O Ubuntu é uma distribuição Linux baseada no Debian, conhecida por sua estabilidade e facilidade de uso.";
                string imagePath = "pack://application:,,,/Assets/Images/ubuntu.png";
                string downloadLink = "https://ubuntu.com/download/desktop"; // pq karalhos minha net tinha que ficar ruim justo quando testo a porra de um site

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            MintPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Mint";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/mint.png";
                string downloadLink = "https://linuxmint.com/download.php"; 

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            ZorinPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Zorin";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/zorin.png";
                string downloadLink = "https://zorin.com/os/download/"; 

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            PopPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "PopOS";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/popos.png";
                string downloadLink = "https://system76.com/pop/?srsltid=AfmBOop3UGv4zcy_41dAXa9YAaxUgtsWEs5I928XYaMBB475zcDMxBBj";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            FedoraPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Fedora";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/fedora.png";
                string downloadLink = "https://www.fedoraproject.org/pt-br/workstation/download";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            KubuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Kubuntu";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/Kubuntu.png";
                string downloadLink = "https://kubuntu.org/archives/getkubuntu.html";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            XubuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Xubuntu";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/Xubuntu.png";
                string downloadLink = "https://xubuntu.org/download/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            ManjaroPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Manjaro";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/manjaro.png";
                string downloadLink = "https://manjaro.org/products/download/x86";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            ArchPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Arch Linux";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/arch.png";
                string downloadLink = "https://archlinux.org/download/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            EndeavourosPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "EndeavourOS";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/endeavouros.png";
                string downloadLink = "https://endeavouros.com/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            KaliPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Kali Linux";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/kali.png";
                string downloadLink = "https://www.kali.org/get-kali/#kali-installer-images";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            ChromeosPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Chrome OS";
                string description = "teste";
                string imagePath = "pack://application:,,,/Assets/Images/chromeos.png";
                string downloadLink = "https://chromeos.google/intl/pt_br/products/chromeos-flex/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

        }
    }
}