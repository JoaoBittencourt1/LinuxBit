using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace LinuxHub // nome meio merda mas fazer oq 
{
    public partial class DistroWindow : Window
    {

        private string downloadLink;

        private List<object> carouselItems = new();
        private int carouselIndex = 0;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int darkMode = 1;

            // DWMWA_USE_IMMERSIVE_DARK_MODE = 20 no Win 11  
            DwmSetWindowAttribute(hwnd, 20, ref darkMode, Marshal.SizeOf(typeof(int)));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        public DistroWindow(string name, string description, string imagePath, string link)
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            DistroName.Text = name;
            DistroDescription.Text = description;
            DistroImage.Source = new BitmapImage(new System.Uri(imagePath, System.UriKind.RelativeOrAbsolute)); // isso aqui ainda vai quebrar e eu não vou saber como

            downloadLink = link; // so link mudar em algum momento o problema não é meu
            DistroDownload.Text = "Clieque aqui para Baixar!";
        }

        private void DistroDownload_Click(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(downloadLink))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadLink,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Não foi possível abrir o link.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); // que isso nunca apareca
                }
            }
        }

        private void UpdateCarousel()
        {
            if (carouselItems.Count == 0)
                return;

            var item = carouselItems[carouselIndex];

            if (item is string path && path.EndsWith(".mp4"))
            {
                CarouselContent.Content = new MediaElement
                {
                    Source = new Uri(path, UriKind.RelativeOrAbsolute),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop,
                    Stretch = Stretch.Uniform
                };
            }
            else if (item is string imagePath)
            {
                CarouselContent.Content = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                    Stretch = Stretch.Uniform
                };
            }
        }


        public void LoadCarousel(params object[] items)
        {
            carouselItems = items.ToList();
            carouselIndex = 0;
            UpdateCarousel();
        }

        private void CarouselNext_Click(object sender, RoutedEventArgs e)
        {
            carouselIndex = (carouselIndex + 1) % carouselItems.Count;
            UpdateCarousel();
        }

        private void CarouselPrev_Click(object sender, RoutedEventArgs e)
        {
            carouselIndex = (carouselIndex - 1 + carouselItems.Count) % carouselItems.Count;
            UpdateCarousel();
        }


    }
}
