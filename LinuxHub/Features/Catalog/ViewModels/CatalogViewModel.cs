using System.Windows.Input;
using LinuxHub.Common.Data;
using LinuxHub.Common.Models;
using LinuxHub.Common.Mvvm;

namespace LinuxHub.Features.Catalog.ViewModels
{
    public class CatalogViewModel : ObservableObject
    {
        public CatalogViewModel()
        {
            Distros = DistroCatalog.All;
            OpenDistroCommand = new RelayCommand(param => OpenDistroRequested?.Invoke((DistroInfo)param!));
        }

        public IReadOnlyList<DistroInfo> Distros { get; }

        public ICommand OpenDistroCommand { get; }

        /// <summary>Pedido de abrir o detalhe de uma distro — a View decide como.</summary>
        public event Action<DistroInfo>? OpenDistroRequested;
    }
}
