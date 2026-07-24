using System.Windows;
using LinuxHub.Common.Helpers;
using LinuxHub.Common.Models;
using LinuxHub.Features.Catalog.ViewModels;
using LinuxHub.Features.Catalog.Views;
using LinuxHub.Features.InstallWizard.ViewModels;

namespace LinuxHub.Shell
{
    /// <summary>
    /// Shell da aplicação: hospeda as views de feature nas abas e cuida só de
    /// navegação entre janelas (chrome). Sem lógica de negócio.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CatalogViewModel _catalogViewModel;
        private readonly InstallWizardViewModel _installWizardViewModel;

        public MainWindow(CatalogViewModel catalogViewModel, InstallWizardViewModel installWizardViewModel)
        {
            _catalogViewModel = catalogViewModel ?? throw new ArgumentNullException(nameof(catalogViewModel));
            _installWizardViewModel = installWizardViewModel ?? throw new ArgumentNullException(nameof(installWizardViewModel));

            InitializeComponent();

            Loaded += (_, _) => WindowChromeHelper.EnableDarkMode(this);

            CatalogViewHost.DataContext = _catalogViewModel;
            _catalogViewModel.OpenDistroRequested += OnOpenDistroRequested;

            InstallWizardViewHost.DataContext = _installWizardViewModel;
        }

        private void OnOpenDistroRequested(DistroInfo distro)
        {
            var window = new DistroDetailWindow(new DistroDetailViewModel(distro))
            {
                Owner = this
            };

            Hide();
            window.Show();
        }
    }
}
