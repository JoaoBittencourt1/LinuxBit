using System.Windows.Input;
using LinuxHub.Common.Mvvm;

namespace LinuxHub.Features.Catalog.ViewModels
{
    public class ImageViewerViewModel : ObservableObject
    {
        public ImageViewerViewModel(string imagePath)
        {
            ImagePath = imagePath;
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        }

        public string ImagePath { get; }

        public ICommand CloseCommand { get; }

        public event Action? CloseRequested;
    }
}
