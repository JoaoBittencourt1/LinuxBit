using System.Globalization;
using System.Reflection;
using System.Windows;
using LinuxHub.Common.Localization;
using LinuxHub.Features.Catalog.ViewModels;
using LinuxHub.Features.Catalog.Views;
using LinuxHub.Features.InstallWizard.ViewModels;
using LinuxHub.Features.InstallWizard.Views;
using Wpf.Ui.Controls;

namespace LinuxHub.Shell
{
    /// <summary>
    /// Shell da aplicação: hospeda as views de feature e troca o idioma. Sem lógica
    /// de negócio. Navegação é troca direta de ContentControl.Content via Click de
    /// botão — não usa o gerenciamento de conteúdo interno do NavigationView (que se
    /// mostrou pouco confiável em teste manual: SelectionChanged nem sempre refletia
    /// o item clicado). Button.Click é um evento simples e determinístico.
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private readonly CatalogView _catalogView;
        private readonly InstallWizardView _installWizardView;

        public MainWindow(CatalogViewModel catalogViewModel, InstallWizardViewModel installWizardViewModel)
        {
            ArgumentNullException.ThrowIfNull(catalogViewModel);
            ArgumentNullException.ThrowIfNull(installWizardViewModel);

            InitializeComponent();

            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = version is null ? string.Empty : $"v{version.Major}.{version.Minor}.{version.Build}";

            _catalogView = new CatalogView { DataContext = catalogViewModel };
            _installWizardView = new InstallWizardView { DataContext = installWizardViewModel };

            ShowCatalog();
        }

        private void CatalogNavButton_Click(object sender, RoutedEventArgs e) => ShowCatalog();

        private void InstallNavButton_Click(object sender, RoutedEventArgs e) => ShowInstallWizard();

        private void ShowCatalog()
        {
            ContentHost.Content = _catalogView;
            CatalogNavButton.Appearance = ControlAppearance.Secondary;
            InstallNavButton.Appearance = ControlAppearance.Transparent;
        }

        private void ShowInstallWizard()
        {
            ContentHost.Content = _installWizardView;
            InstallNavButton.Appearance = ControlAppearance.Secondary;
            CatalogNavButton.Appearance = ControlAppearance.Transparent;
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageMenu.PlacementTarget = LanguageButton;
            LanguageMenu.IsOpen = true;
        }

        private void PortugueseMenuItem_Click(object sender, RoutedEventArgs e) =>
            LocalizationManager.Instance.SetLanguage(new CultureInfo("pt-BR"));

        private void EnglishMenuItem_Click(object sender, RoutedEventArgs e) =>
            LocalizationManager.Instance.SetLanguage(new CultureInfo("en-US"));
    }
}
