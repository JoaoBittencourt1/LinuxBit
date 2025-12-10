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

        }
    }
}