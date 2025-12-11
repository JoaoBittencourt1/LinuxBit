using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LinuxHub
{
    public partial class DistroWindow : Window
    {
        private string downloadLink;
        private List<object> carouselItems = new();
        private int carouselIndex = 0;

        private List<BitmapImage> loadedImages = new(); // imagens grandes carregadas dinamicamente
        private MediaElement? currentVideo = null;       // vídeo atual do carrossel

        // Ativa dark mode
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, 20, ref darkMode, Marshal.SizeOf(typeof(int)));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public DistroWindow(string name, string description, string imagePath, string link)
        {
            InitializeComponent();
            Loaded += WindowLoaded;

            // Informações da distro
            DistroName.Text = name;
            DistroDescription.Text = description;

            // Carga segura da imagem principal
            DistroImage.Source = LoadBitmapSafe(imagePath);

            downloadLink = link;
            DistroDownload.Text = "Clique aqui para Baixar!";
        }

        // criação de BitmapImage com opções que evitam arquivo bloqueado
        private BitmapImage LoadBitmapSafe(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private void DistroDownload_Click(object sender, MouseButtonEventArgs e)
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
                MessageBox.Show("Não foi possível abrir o link.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===========================
        //       CARROSSEL
        // ===========================

        public void LoadCarousel(params object[] items)
        {
            carouselItems = items.ToList();
            carouselIndex = 0;
            UpdateCarousel();
        }

        private void UpdateCarousel()
        {
            if (carouselItems.Count == 0)
                return;

            FreeCurrentMedia();

            var item = carouselItems[carouselIndex];

            // --- VÍDEO ---
            if (item is string path && path.EndsWith(".mp4"))
            {
                var video = new MediaElement
                {
                    Source = new Uri(path, UriKind.RelativeOrAbsolute),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop,
                    Stretch = Stretch.Uniform
                };

                CarouselContent.Content = video;
                video.Play();
                currentVideo = video;
                return;
            }

            // --- IMAGEM ---
            if (item is string imagePath)
            {
                var bmp = LoadBitmapSafe(imagePath);
                loadedImages.Add(bmp);

                var img = new Image
                {
                    Source = bmp,
                    Stretch = Stretch.Uniform,
                    Cursor = Cursors.Hand
                };

                img.MouseLeftButtonUp += (s, e) => OpenImageFull(imagePath);

                CarouselContent.Content = img;
            }
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

        private void OpenImageFull(string path)
        {
            var win = new ImageViewerWindow(path);
            win.Owner = this;
            win.ShowDialog();
        }

        // ===========================
        //   LIBERAÇÃO DE MEMÓRIA
        // ===========================

        private void FreeCurrentMedia()
        {
            // parar vídeo
            if (currentVideo != null)
            {
                try
                {
                    currentVideo.Stop();
                    currentVideo.Source = null;
                }
                catch { }
                currentVideo = null;
            }

            // limpar imagem atual
            foreach (var img in loadedImages)
            {
                try
                {
                    if (img.StreamSource != null)
                        img.StreamSource.Dispose();
                }
                catch { }
            }

            loadedImages.Clear();
            CarouselContent.Content = null;
        }

        // chamado ao clicar em voltar
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            FreeCurrentMedia();

            if (Owner != null)
            {
                Owner.Show();
                Owner.Activate();
            }

            Close();
        }

        // chamado quando fechar pela borda da janela
        protected override void OnClosed(EventArgs e)
        {
            FreeCurrentMedia();
            base.OnClosed(e);
        }
    }
}
