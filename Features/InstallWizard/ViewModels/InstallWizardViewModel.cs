using System.Windows.Input;
using LinuxHub.Common.Localization;
using LinuxHub.Common.Mvvm;
using LinuxHub.Features.InstallWizard.Services;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Orquestra as três etapas do wizard (ISO, alvo, conta) e gera o
    /// <c>install.conf</c> ao confirmar. Ver specs/install-wizard/spec.md.
    /// </summary>
    public class InstallWizardViewModel : ObservableObject
    {
        private readonly InstallerConfigBuilder _configBuilder;
        private readonly IInstallerConfigWriter _configWriter;

        public InstallWizardViewModel(
            IsoAcquisitionViewModel iso,
            TargetSelectionViewModel target,
            AccountViewModel account,
            InstallerConfigBuilder configBuilder,
            IInstallerConfigWriter configWriter)
        {
            Iso = iso ?? throw new ArgumentNullException(nameof(iso));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Account = account ?? throw new ArgumentNullException(nameof(account));
            _configBuilder = configBuilder ?? throw new ArgumentNullException(nameof(configBuilder));
            _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));

            Iso.Notify += (title, message, isError) => Notify?.Invoke(title, message, isError);

            InstallCommand = new RelayCommand(Install);
        }

        public IsoAcquisitionViewModel Iso { get; }
        public TargetSelectionViewModel Target { get; }
        public AccountViewModel Account { get; }

        public ICommand InstallCommand { get; }

        public event Action<string, string, bool>? Notify;

        /// <summary>
        /// Chamado pela View depois de se inscrever em <see cref="Notify"/> — avisos que
        /// dependem de estado calculado no construtor (ex.: UEFI) não podem ser disparados
        /// do próprio construtor, porque ainda não há ninguém ouvindo o evento nesse ponto.
        /// </summary>
        public void RaiseStartupWarnings()
        {
            if (!Target.IsUefi)
            {
                var loc = LocalizationManager.Instance;
                Notify?.Invoke(loc["Wizard_UefiWarningTitle"], loc["Wizard_UefiWarningMessage"], false);
            }
        }

        private void Install()
        {
            var loc = LocalizationManager.Instance;

            try
            {
                if (Iso.DisplayedDistro is not { } distro)
                    throw new InvalidOperationException(loc["Wizard_NoDistroSelected"]);

                if (string.IsNullOrWhiteSpace(Iso.ResolvedIsoPath))
                    throw new InvalidOperationException(loc["Wizard_NoIsoSelected"]);

                var request = new BuildInstallerConfigRequest(
                    Distro: distro,
                    IsoPath: Iso.ResolvedIsoPath,
                    IsUefi: Target.IsUefi,
                    Mode: Target.Mode,
                    TargetDiskIndex: Target.SelectedDisk?.Index,
                    TargetPartitionIndex: Target.IsDualBootMode ? Target.SelectedPartition?.PartitionIndex : null,
                    LinuxPartitionSizeGb: (int)Target.LinuxPartitionSizeGb,
                    Username: Account.Username,
                    Password: Account.Password,
                    Hostname: Account.Hostname);

                var config = _configBuilder.Build(request);
                _configWriter.Save(config);

                Notify?.Invoke(loc["Wizard_InstallSuccessTitle"], loc["Wizard_InstallSuccessMessage"], false);
            }
            catch (Exception ex)
            {
                Notify?.Invoke(loc["Wizard_InstallErrorTitle"], ex.Message, true);
            }
        }
    }
}
