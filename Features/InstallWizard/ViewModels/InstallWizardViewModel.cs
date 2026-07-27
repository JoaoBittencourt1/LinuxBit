using System.Windows.Input;
using LinuxHub.Common.Localization;
using LinuxHub.Common.Models;
using LinuxHub.Common.Mvvm;
using LinuxHub.Features.InstallWizard.Services;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Orquestra as três etapas do wizard (ISO, alvo, conta), exige confirmação
    /// destrutiva explícita e gera o <c>install.conf</c> ao confirmar. Ver
    /// specs/install-wizard/spec.md.
    /// </summary>
    public class InstallWizardViewModel : ObservableObject
    {
        private readonly InstallerConfigBuilder _configBuilder;
        private readonly IInstallerConfigWriter _configWriter;
        private readonly IDiskPartitioningService _diskPartitioning;
        private readonly IBootStagingService _bootStaging;
        private ConfirmationViewModel? _pendingConfirmation;

        public InstallWizardViewModel(
            IsoAcquisitionViewModel iso,
            TargetSelectionViewModel target,
            AccountViewModel account,
            InstallerConfigBuilder configBuilder,
            IInstallerConfigWriter configWriter,
            IDiskPartitioningService diskPartitioning,
            IBootStagingService bootStaging)
        {
            Iso = iso ?? throw new ArgumentNullException(nameof(iso));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Account = account ?? throw new ArgumentNullException(nameof(account));
            _configBuilder = configBuilder ?? throw new ArgumentNullException(nameof(configBuilder));
            _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
            _diskPartitioning = diskPartitioning ?? throw new ArgumentNullException(nameof(diskPartitioning));
            _bootStaging = bootStaging ?? throw new ArgumentNullException(nameof(bootStaging));

            Iso.Notify += (title, message, isError) => Notify?.Invoke(title, message, isError);

            InstallCommand = new RelayCommand(BeginInstall);
        }

        public IsoAcquisitionViewModel Iso { get; }
        public TargetSelectionViewModel Target { get; }
        public AccountViewModel Account { get; }

        public ICommand InstallCommand { get; }

        /// <summary>Não nulo entre o clique em "Instalar" e a confirmação/cancelamento
        /// do usuário — a instalação de fato só ocorre depois de confirmada.</summary>
        public ConfirmationViewModel? PendingConfirmation
        {
            get => _pendingConfirmation;
            private set
            {
                if (!SetProperty(ref _pendingConfirmation, value))
                    return;

                OnPropertyChanged(nameof(IsConfirming));
                OnPropertyChanged(nameof(IsNotConfirming));
            }
        }

        public bool IsConfirming => PendingConfirmation is not null;
        public bool IsNotConfirming => !IsConfirming;

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

        private void BeginInstall()
        {
            var loc = LocalizationManager.Instance;

            try
            {
                if (Iso.DisplayedDistro is not { } distro)
                    throw new InvalidOperationException(loc["Wizard_NoDistroSelected"]);

                if (string.IsNullOrWhiteSpace(Iso.ResolvedIsoPath))
                    throw new InvalidOperationException(loc["Wizard_NoIsoSelected"]);

                bool isReplace = Target.IsReplaceMode;

                string summary = isReplace
                    ? loc.Format(
                        Target.IsReplacingSystemDisk ? "Wizard_ConfirmReplaceSystemDiskSummary" : "Wizard_ConfirmReplaceSummary",
                        Target.SelectedDisk?.ToString() ?? string.Empty)
                    : loc.Format(
                        "Wizard_ConfirmShrinkSummary",
                        Target.SelectedPartition?.ToString() ?? string.Empty,
                        (int)Target.LinuxPartitionSizeGb);

                var confirmation = new ConfirmationViewModel(
                    summary,
                    requiresTypedConfirmation: isReplace,
                    confirmationWord: loc["Wizard_ConfirmReplaceWord"]);

                confirmation.Confirmed += () => ExecuteInstall(distro);
                confirmation.Cancelled += () => PendingConfirmation = null;

                PendingConfirmation = confirmation;
            }
            catch (Exception ex)
            {
                Notify?.Invoke(loc["Wizard_InstallErrorTitle"], ex.Message, true);
            }
        }

        private void ExecuteInstall(DistroInfo distro)
        {
            var loc = LocalizationManager.Instance;

            try
            {
                if (Target.IsDualBootMode && Target.SelectedPartition is { } partition)
                {
                    _diskPartitioning.ShrinkPartition(
                        partition.DiskIndex,
                        partition.PartitionIndex,
                        (int)Target.LinuxPartitionSizeGb);
                }

                var request = new BuildInstallerConfigRequest(
                    Distro: distro,
                    IsoPath: Iso.ResolvedIsoPath!,
                    IsUefi: Target.IsUefi,
                    Mode: Target.Mode,
                    TargetDiskIndex: Target.IsReplaceMode ? Target.SelectedDisk?.Index : Target.SelectedPartition?.DiskIndex,
                    TargetPartitionIndex: Target.IsDualBootMode ? Target.SelectedPartition?.PartitionIndex : null,
                    LinuxPartitionSizeGb: (int)Target.LinuxPartitionSizeGb,
                    Username: Account.Username,
                    Password: Account.Password,
                    Hostname: Account.Hostname);

                var config = _configBuilder.Build(request);
                _configWriter.Save(config);

                int targetDiskIndex = Target.IsReplaceMode
                    ? Target.SelectedDisk!.Index
                    : Target.SelectedPartition!.DiskIndex;

                _bootStaging.InstallStagingBootloader(new BootStagingRequest(
                    DistroName: distro.Name,
                    IsoPath: Iso.ResolvedIsoPath!,
                    IsUefi: Target.IsUefi,
                    TargetDiskIndex: targetDiskIndex));

                Notify?.Invoke(loc["Wizard_InstallSuccessTitle"], loc["Wizard_InstallSuccessMessage"], false);
            }
            catch (Exception ex)
            {
                Notify?.Invoke(loc["Wizard_InstallErrorTitle"], ex.Message, true);
            }
            finally
            {
                PendingConfirmation = null;
            }
        }
    }
}
