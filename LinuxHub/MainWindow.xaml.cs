using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace LinuxHub
{
    
    public partial class MainWindow : Window
    {

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int darkMode = 1;

            // DWMWA_USE_IMMERSIVE_DARK_MODE = 20 no Win 11  
            DwmSetWindowAttribute(hwnd, 20, ref darkMode, Marshal.SizeOf(typeof(int)));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        public MainWindow()
        {
            // esse codigo logo logo vai ficar um saco de dar suporte, tenho que dar um jeito de arrumar isso
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            UbuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Ubuntu";
                string description = "O Ubuntu é uma das distribuições Linux mais populares e amigáveis, sendo altamente recomendada para iniciantes. " +
                         "Sua interface gráfica moderna lembra um pouco o MacOS, oferecendo um ambiente intuitivo e agradável. " +
                         "O sistema conta com recursos de acessibilidade, facilitando o uso por pessoas com dificuldades visuais ou auditivas. " +
                         "Além disso, possui uma comunidade ativa e vibrante, que contribui com suporte, tutoriais e atualizações frequentes, " +
                         "garantindo estabilidade e segurança contínua para usuários de todos os níveis.";
                string imagePath = "pack://application:,,,/Assets/Images/ubuntu.png";
                string downloadLink = "https://ubuntu.com/download/desktop"; // pq karalhos minha net tinha que ficar ruim justo quando testo a porra de um site

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                

                window.LoadCarousel(
                    "pack://application:,,,/Assets/Images/Ubuntu/ubuntu1.jpg", // pq NAO ABRE QUANDO EU ABRO NO /UBUNTU/UBUNTU1.JPG MAS ABRE SE EU ABRIR AS IMAGENS DAS DISTROS 
                    "pack://application:,,,/Assets/Images/Ubuntu/ubuntu2.png"
                );

            

                window.Show();
            };

            MintPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Mint";
                string description = "O Linux Mint é uma distribuição Linux focada em oferecer uma experiência familiar para usuários vindos do Windows. " +
                         "Sua interface é limpa, direta e altamente personalizável, com menus e áreas de trabalho que lembram bastante o ambiente do Windows, " +
                         "facilitando a adaptação para quem está migrando. " +
                         "O Mint vem pré-carregado com uma variedade de softwares essenciais e ferramentas de fácil instalação, " +
                         "permitindo que o usuário comece a trabalhar ou navegar rapidamente sem complicações. " +
                         "Além disso, possui uma comunidade ativa e suporte contínuo, garantindo estabilidade, segurança e atualizações regulares."; 
                string imagePath = "pack://application:,,,/Assets/Images/mint.png";
                string downloadLink = "https://linuxmint.com/download.php"; 

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);

                window.LoadCarousel(
                    "pack://application:,,,/Assets/Images/Mint/Mint1.png",  
                    "pack://application:,,,/Assets/Images/Mint/mint2.png"
                );

                window.Show();
            };
            ZorinPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Zorin";
                string description = "O Zorin OS é uma distribuição Linux projetada para ser extremamente acessível e elegante, " +
                         "ideal para quem está migrando do Windows ou do Mac. Sua interface moderna e altamente personalizável permite " +
                         "que os usuários adaptem o visual e a experiência do sistema conforme suas preferências, tornando a transição suave e confortável. " +
                         "Além disso, o Zorin OS oferece compatibilidade com uma grande variedade de softwares, incluindo aplicativos Windows via Wine, " +
                         "e vem equipado com ferramentas de produtividade, multimídia e internet prontas para uso. " +
                         "Com atualizações regulares e foco em segurança, ele garante uma experiência estável e confiável, tanto para iniciantes quanto para usuários avançados.";
                string imagePath = "pack://application:,,,/Assets/Images/zorin.png";
                string downloadLink = "https://zorin.com/os/download/"; 

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            PopPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "PopOS";
                string description = "O Pop!_OS é uma distribuição Linux desenvolvida pela System76, projetada para oferecer alta produtividade " +
                         "e desempenho otimizado, especialmente para desenvolvedores, engenheiros e profissionais criativos. " +
                         "Sua interface é moderna, limpa e eficiente, permitindo uma navegação rápida e organizada entre janelas e aplicativos. " +
                         "O Pop!_OS se destaca pelo suporte nativo a hardware moderno, incluindo drivers de GPU para jogos e aplicações gráficas, " +
                         "e vem com ferramentas integradas para gerenciamento de janelas, workspaces e atalhos, aumentando a eficiência do usuário. " +
                         "Além disso, conta com atualizações frequentes, forte foco em segurança e uma comunidade ativa pronta para ajudar.";
                string imagePath = "pack://application:,,,/Assets/Images/popos.png";
                string downloadLink = "https://system76.com/pop/?srsltid=AfmBOop3UGv4zcy_41dAXa9YAaxUgtsWEs5I928XYaMBB475zcDMxBBj";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            FedoraPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Fedora";
                string description = "O Fedora é uma distribuição Linux moderna e inovadora, focada em fornecer as últimas tecnologias de software com estabilidade e segurança. " +
"É ideal para desenvolvedores, entusiastas de tecnologia e usuários avançados que desejam experimentar novas funcionalidades sem abrir mão da confiabilidade. " +
"Com uma interface elegante e amigável, o Fedora oferece suporte nativo a diversos ambientes gráficos e ferramentas de desenvolvimento, " +
"além de atualizações frequentes e uma comunidade ativa pronta para suporte e colaboração.";
                string imagePath = "pack://application:,,,/Assets/Images/fedora.png";
                string downloadLink = "https://www.fedoraproject.org/pt-br/workstation/download";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            KubuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Kubuntu";
                string description = "O Kubuntu é uma variante do Ubuntu que utiliza o ambiente gráfico KDE Plasma, conhecido por sua beleza, flexibilidade e personalização. " +
"Ele combina a estabilidade e confiabilidade do Ubuntu com uma interface moderna, elegante e altamente configurável, permitindo que os usuários moldem o sistema conforme suas necessidades. " +
"Kubuntu é ideal tanto para iniciantes que desejam uma experiência visual agradável quanto para usuários avançados que valorizam personalização e eficiência.";
                string imagePath = "pack://application:,,,/Assets/Images/Kubuntu.png";
                string downloadLink = "https://kubuntu.org/archives/getkubuntu.html";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            XubuntuPanel.MouseLeftButtonUp += (s, e) =>
            {
                string name = "Xubuntu";
                string description = "O Xubuntu é uma distribuição Linux leve baseada no Ubuntu, usando o ambiente gráfico XFCE, perfeito para computadores com hardware mais modesto ou antigos. " +
"Ele oferece uma interface simples, rápida e estável, mantendo a experiência familiar do Ubuntu. " +
"Xubuntu é ideal para usuários que querem um sistema eficiente, responsivo e econômico em recursos, sem abrir mão da confiabilidade e da comunidade ativa do Ubuntu.";
                string imagePath = "pack://application:,,,/Assets/Images/Xubuntu.png";
                string downloadLink = "https://xubuntu.org/download/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };
            ManjaroPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Manjaro";
                string description = "O Manjaro é uma distribuição Linux baseada no Arch, mas com foco em facilidade de uso e instalação. " +
"Ele combina a potência e flexibilidade do Arch com uma experiência pronta para uso, incluindo drivers, codecs e softwares essenciais pré-instalados. " +
"Manjaro oferece atualizações contínuas (rolling release), uma interface amigável e suporte a múltiplos ambientes gráficos, sendo ideal para usuários que desejam controle avançado sem complicações.";
                string imagePath = "pack://application:,,,/Assets/Images/manjaro.png";
                string downloadLink = "https://manjaro.org/products/download/x86";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            ArchPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Arch Linux";
                string description = "O Arch Linux é uma distribuição Linux conhecida por sua filosofia de simplicidade e pelo alto grau de personalização que oferece. " +
"Ele permite que os usuários construam seu sistema do zero, escolhendo exatamente quais pacotes e configurações desejam, tornando-o extremamente flexível e adaptável a qualquer necessidade. " +
"No entanto, essa liberdade vem acompanhada de desafios: o Arch não é recomendado para iniciantes, pois exige conhecimento avançado de Linux para instalação e manutenção. " +
"Apesar disso, usuários experientes valorizam o Arch pela possibilidade de criar um sistema enxuto, eficiente e completamente sob seu controle, com acesso às últimas atualizações de software através do modelo rolling release.";
                string imagePath = "pack://application:,,,/Assets/Images/arch.png";
                string downloadLink = "https://archlinux.org/download/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            EndeavourosPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "EndeavourOS";
                string description = "O EndeavourOS é uma distribuição Linux baseada no Arch que mantém a filosofia de simplicidade e personalização, mas com facilidade de instalação e configuração inicial. " +
"Ele fornece uma experiência Arch pura, mas guiada, permitindo que usuários escolham seu ambiente gráfico e aplicativos preferidos. " +
"EndeavourOS é ideal para entusiastas que querem aprender e controlar seu sistema, com o suporte de uma comunidade acolhedora e ativa.";
                string imagePath = "pack://application:,,,/Assets/Images/endeavouros.png";
                string downloadLink = "https://endeavouros.com/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            KaliPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Kali Linux";
                string description = "O Kali Linux é uma distribuição especializada em segurança e testes de penetração, projetada para profissionais de segurança cibernética e hackers éticos. " +
"Ela vem pré-carregada com centenas de ferramentas para análise de vulnerabilidades, testes de redes, engenharia reversa e investigação forense digital. " +
"Kali é poderoso, altamente configurável e voltado para usuários avançados que precisam de um ambiente seguro e completo para análise e testes de segurança.";
                string imagePath = "pack://application:,,,/Assets/Images/kali.png";
                string downloadLink = "https://www.kali.org/get-kali/#kali-installer-images";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            ChromeosPanel.MouseLeftButtonUp += (s, e) => 
            {
                string name = "Chrome OS";
                string description = "O ChromeOS é uma distribuição baseada em Linux desenvolvida pelo Google, voltada para simplicidade, rapidez e uso na nuvem. " +
"Ele oferece uma experiência centrada em navegador e aplicativos web, com segurança integrada, atualizações automáticas e inicialização rápida. " +
"ChromeOS é ideal para usuários que dependem de internet e serviços na nuvem, proporcionando um sistema leve, eficiente e de fácil manutenção.";
                string imagePath = "pack://application:,,,/Assets/Images/chromeos.png";
                string downloadLink = "https://chromeos.google/intl/pt_br/products/chromeos-flex/";

                DistroWindow window = new DistroWindow(name, description, imagePath, downloadLink);
                window.Show();
            };

            // eu escrevi algumas dessas descrições mas a preguiça bateu e botei o gpt pra ralar. ISSO PRA DAR SUPORTE VAI SER UM INFERNO!!!! preguiça hoje trabalho amanha

        }
    }
}