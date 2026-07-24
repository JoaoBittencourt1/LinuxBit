using System.Diagnostics;
using System.Windows.Input;
using LinuxHub.Common.Models;
using LinuxHub.Common.Mvvm;

namespace LinuxHub.Features.Catalog.ViewModels
{
    public class DistroDetailViewModel : ObservableObject
    {
        private int _carouselIndex;

        public DistroDetailViewModel(DistroInfo distro)
        {
            ArgumentNullException.ThrowIfNull(distro);

            Name = distro.Name;
            Description = distro.Description;
            ImagePath = distro.ImagePath;
            DownloadLink = distro.DownloadLink;
            CarouselItems = distro.CarouselImages;

            NextCommand = new RelayCommand(
                () => CarouselIndex = (CarouselIndex + 1) % CarouselItems.Count,
                () => CarouselItems.Count > 1);

            PrevCommand = new RelayCommand(
                () => CarouselIndex = (CarouselIndex - 1 + CarouselItems.Count) % CarouselItems.Count,
                () => CarouselItems.Count > 1);

            OpenImageCommand = new RelayCommand(path => OpenImageRequested?.Invoke((string)path!));
            OpenDownloadLinkCommand = new RelayCommand(OpenDownloadLink);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke());
        }

        public string Name { get; }
        public string Description { get; }
        public string ImagePath { get; }
        public string DownloadLink { get; }
        public IReadOnlyList<string> CarouselItems { get; }

        public int CarouselIndex
        {
            get => _carouselIndex;
            private set
            {
                if (SetProperty(ref _carouselIndex, value))
                    OnPropertyChanged(nameof(CurrentCarouselItem));
            }
        }

        public string? CurrentCarouselItem =>
            CarouselItems.Count > 0 ? CarouselItems[CarouselIndex] : null;

        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand OpenImageCommand { get; }
        public ICommand OpenDownloadLinkCommand { get; }
        public ICommand BackCommand { get; }

        /// <summary>Pedido de abrir uma imagem do carrossel em tela cheia — a View decide como.</summary>
        public event Action<string>? OpenImageRequested;

        /// <summary>Pedido de voltar à janela principal — a View decide como.</summary>
        public event Action? BackRequested;

        /// <summary>Falha ao abrir o link de download no navegador — a View decide como reportar.</summary>
        public event Action? DownloadLinkOpenFailed;

        private void OpenDownloadLink()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = DownloadLink,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                DownloadLinkOpenFailed?.Invoke();
            }
        }
    }
}
