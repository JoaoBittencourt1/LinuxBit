using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LinuxHub.Common.Models;
using LinuxHub.Features.Catalog.ViewModels;

namespace LinuxHub.Features.Catalog.Views
{
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
        }

        private void DistroTile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CatalogViewModel viewModel && ((FrameworkElement)sender).DataContext is DistroInfo distro)
                viewModel.OpenDistroCommand.Execute(distro);
        }

        private void FullscreenOverlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CatalogViewModel viewModel)
                viewModel.CloseFullscreenCommand.Execute(null);
        }
    }
}
