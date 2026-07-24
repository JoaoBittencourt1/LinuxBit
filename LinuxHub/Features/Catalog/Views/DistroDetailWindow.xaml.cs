using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LinuxHub.Common.Helpers;
using LinuxHub.Features.Catalog.ViewModels;

namespace LinuxHub.Features.Catalog.Views
{
    /// <summary>
    /// Renderização do carrossel (imagem vs. vídeo) e liberação de mídia dependem
    /// diretamente de APIs de WPF (MediaElement, BitmapImage) — por isso ficam aqui
    /// no code-behind, não na ViewModel. A navegação/índice do carrossel é da VM.
    /// </summary>
    public partial class DistroDetailWindow : Window
    {
        private readonly List<BitmapImage> _loadedImages = new();
        private MediaElement? _currentVideo;

        public DistroDetailWindow(DistroDetailViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.OpenImageRequested += path =>
            {
                var win = new ImageViewerWindow(path) { Owner = this };
                win.ShowDialog();
            };
            viewModel.BackRequested += () =>
            {
                FreeCurrentMedia();
                Owner?.Show();
                Owner?.Activate();
                Close();
            };
            viewModel.DownloadLinkOpenFailed += () =>
                MessageBox.Show("Não foi possível abrir o link.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            UpdateCarousel(viewModel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => WindowChromeHelper.EnableDarkMode(this);

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DistroDetailViewModel.CurrentCarouselItem))
                UpdateCarousel((DistroDetailViewModel)sender!);
        }

        private void DistroDownload_Click(object sender, MouseButtonEventArgs e) =>
            ((DistroDetailViewModel)DataContext).OpenDownloadLinkCommand.Execute(null);

        private void UpdateCarousel(DistroDetailViewModel viewModel)
        {
            FreeCurrentMedia();

            var item = viewModel.CurrentCarouselItem;
            if (item is null)
                return;

            if (item.EndsWith(".mp4"))
            {
                var video = new MediaElement
                {
                    Source = new Uri(item, UriKind.RelativeOrAbsolute),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop,
                    Stretch = Stretch.Uniform
                };

                CarouselContent.Content = video;
                video.Play();
                _currentVideo = video;
                return;
            }

            var bmp = LoadBitmapSafe(item);
            _loadedImages.Add(bmp);

            var img = new Image
            {
                Source = bmp,
                Stretch = Stretch.Uniform,
                Cursor = Cursors.Hand
            };

            img.MouseLeftButtonUp += (_, _) => viewModel.OpenImageCommand.Execute(item);
            CarouselContent.Content = img;
        }

        private static BitmapImage LoadBitmapSafe(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private void FreeCurrentMedia()
        {
            if (_currentVideo != null)
            {
                try
                {
                    _currentVideo.Stop();
                    _currentVideo.Source = null;
                }
                catch (Exception)
                {
                    // WPF pode lançar se o MediaElement já foi descartado pelo unloading da
                    // janela; parar/limpar é best-effort de liberação de recurso, não uma
                    // falha que o usuário precise ver.
                }
                _currentVideo = null;
            }

            foreach (var img in _loadedImages)
            {
                try
                {
                    img.StreamSource?.Dispose();
                }
                catch (Exception)
                {
                    // idem acima — best-effort de liberação de recurso.
                }
            }

            _loadedImages.Clear();
            CarouselContent.Content = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            FreeCurrentMedia();
            base.OnClosed(e);
        }
    }
}
