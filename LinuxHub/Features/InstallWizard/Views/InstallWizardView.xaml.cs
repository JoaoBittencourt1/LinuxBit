using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LinuxHub.Features.InstallWizard.ViewModels;
using Microsoft.Win32;

namespace LinuxHub.Features.InstallWizard.Views
{
    /// <summary>
    /// PasswordBox não suporta data binding (por design, por segurança) — por isso o
    /// code-behind sincroniza manualmente com AccountViewModel.Password/ConfirmPassword,
    /// que são a fonte de verdade real.
    /// </summary>
    public partial class InstallWizardView : UserControl
    {
        public InstallWizardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is InstallWizardViewModel oldVm)
            {
                oldVm.Notify -= OnNotify;
                oldVm.Account.PropertyChanged -= OnAccountPropertyChanged;
            }

            if (e.NewValue is InstallWizardViewModel newVm)
            {
                newVm.Notify += OnNotify;
                newVm.Account.PropertyChanged += OnAccountPropertyChanged;
                newVm.RaiseStartupWarnings();
            }
        }

        private void OnNotify(string title, string message, bool isError) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);

        private void OnAccountPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var account = (AccountViewModel)sender!;

            if (e.PropertyName == nameof(AccountViewModel.Password) && PasswordBox.Password != account.Password)
                PasswordBox.Password = account.Password;

            if (e.PropertyName == nameof(AccountViewModel.ConfirmPassword) && ConfirmPasswordBox.Password != account.ConfirmPassword)
                ConfirmPasswordBox.Password = account.ConfirmPassword;
        }

        private void BrowseIso_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (InstallWizardViewModel)DataContext;

            var dialog = new OpenFileDialog
            {
                Title = "Selecionar imagem ISO",
                Filter = "Imagem ISO (*.iso)|*.iso",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
                viewModel.Iso.SelectManualIso(dialog.FileName);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            ((InstallWizardViewModel)DataContext).Account.Password = PasswordBox.Password;

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            ((InstallWizardViewModel)DataContext).Account.ConfirmPassword = ConfirmPasswordBox.Password;
    }
}
