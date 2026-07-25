using System.IO;
using System.Windows.Input;
using LinuxHub.Common.Data;
using LinuxHub.Common.Localization;
using LinuxHub.Common.Models;
using LinuxHub.Common.Mvvm;
using LinuxHub.Features.InstallWizard.Services;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Fonte da ISO: download automático (com progresso/cancelamento) ou seleção manual
    /// com validação e detecção de distro. Ver specs/install-wizard/spec.md.
    /// </summary>
    public class IsoAcquisitionViewModel : ObservableObject
    {
        private const long MinimumIsoSizeBytes = 700L * 1024 * 1024;

        private readonly IIsoDownloadService _downloadService;
        private readonly IDistroDetectionService _detectionService;
        private readonly AsyncRelayCommand _downloadIsoCommand;
        private readonly RelayCommand _cancelDownloadCommand;
        private CancellationTokenSource? _downloadCts;

        private bool _isManualSelect;
        private DistroInfo? _selectedDistro;
        private string? _manualIsoPath;
        private DistroInfo? _detectedDistro;
        private bool _isDownloading;
        private double _downloadPercent;
        private bool _isDownloadIndeterminate;
        private string _downloadStatusText = string.Empty;

        public IsoAcquisitionViewModel(IIsoDownloadService downloadService, IDistroDetectionService detectionService)
        {
            _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            _detectionService = detectionService ?? throw new ArgumentNullException(nameof(detectionService));

            Distros = DistroCatalog.All;
            SelectedDistro = Distros.Count > 0 ? Distros[0] : null;

            _downloadIsoCommand = new AsyncRelayCommand(DownloadAsync, () => !IsManualSelect && SelectedDistro is not null && !IsDownloading);
            _cancelDownloadCommand = new RelayCommand(() => _downloadCts?.Cancel(), () => IsDownloading);
        }

        public IReadOnlyList<DistroInfo> Distros { get; }

        public bool IsManualSelect
        {
            get => _isManualSelect;
            set
            {
                if (SetProperty(ref _isManualSelect, value))
                {
                    OnPropertyChanged(nameof(IsManualIsoVisible));
                    OnPropertyChanged(nameof(IsDistroSelectionVisible));
                    OnPropertyChanged(nameof(IsDistroDisplayVisible));
                    OnPropertyChanged(nameof(IsDownloadButtonVisible));
                }
            }
        }

        public bool IsManualIsoVisible => IsManualSelect;
        public bool IsDistroSelectionVisible => !IsManualSelect;
        public bool IsDistroDisplayVisible => IsManualSelect;
        public bool IsDownloadButtonVisible => !IsManualSelect && !IsDownloading;

        public DistroInfo? SelectedDistro
        {
            get => _selectedDistro;
            set => SetProperty(ref _selectedDistro, value);
        }

        public string? ManualIsoPath
        {
            get => _manualIsoPath;
            private set => SetProperty(ref _manualIsoPath, value);
        }

        /// <summary>Distro exibida (selecionada no auto-download ou detectada na seleção manual).</summary>
        public DistroInfo? DisplayedDistro => IsManualSelect ? _detectedDistro : SelectedDistro;

        public bool IsDownloading
        {
            get => _isDownloading;
            private set
            {
                if (SetProperty(ref _isDownloading, value))
                {
                    OnPropertyChanged(nameof(IsDownloadButtonVisible));
                    _downloadIsoCommand.RaiseCanExecuteChanged();
                    _cancelDownloadCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public double DownloadPercent
        {
            get => _downloadPercent;
            private set => SetProperty(ref _downloadPercent, value);
        }

        /// <summary>True quando o servidor não informou o tamanho total da ISO —
        /// a barra de progresso não tem como mostrar percentual real nesse caso.</summary>
        public bool IsDownloadIndeterminate
        {
            get => _isDownloadIndeterminate;
            private set => SetProperty(ref _isDownloadIndeterminate, value);
        }

        public string DownloadStatusText
        {
            get => _downloadStatusText;
            private set => SetProperty(ref _downloadStatusText, value);
        }

        /// <summary>Caminho final da ISO pronta para uso (baixada ou selecionada manualmente).</summary>
        public string? ResolvedIsoPath { get; private set; }

        public ICommand DownloadIsoCommand => _downloadIsoCommand;
        public ICommand CancelDownloadCommand => _cancelDownloadCommand;

        public event Action<string, string, bool>? Notify;

        /// <summary>Chamado pela View após o usuário escolher um arquivo no diálogo de seleção.</summary>
        public void SelectManualIso(string path)
        {
            if (!IsValidIso(path))
            {
                var loc = LocalizationManager.Instance;
                Notify?.Invoke(loc["Wizard_IsoInvalidTitle"], loc["Wizard_IsoInvalidMessage"], true);
                ManualIsoPath = null;
                ResolvedIsoPath = null;
                _detectedDistro = null;
                OnPropertyChanged(nameof(DisplayedDistro));
                return;
            }

            ManualIsoPath = path;
            ResolvedIsoPath = path;
            _detectedDistro = _detectionService.Detect(path);
            OnPropertyChanged(nameof(DisplayedDistro));
        }

        private static bool IsValidIso(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            return new FileInfo(path).Length > MinimumIsoSizeBytes;
        }

        private async Task DownloadAsync()
        {
            if (SelectedDistro is not { } distro)
                return;

            var loc = LocalizationManager.Instance;

            IsDownloading = true;
            DownloadPercent = 0;
            IsDownloadIndeterminate = false;
            DownloadStatusText = loc["Wizard_DownloadStarting"];

            _downloadCts = new CancellationTokenSource();
            var progress = new Progress<IsoDownloadProgress>(p =>
            {
                DownloadPercent = p.PercentComplete ?? 0;
                IsDownloadIndeterminate = p.TotalBytes is null;

                var speedText = $"{FormatBytes(p.BytesPerSecond)}/s";

                if (p.TotalBytes is not { } totalBytes)
                {
                    DownloadStatusText = loc.Format(
                        "Wizard_DownloadProgressUnknownTotal",
                        FormatBytes(p.BytesReceived),
                        speedText);
                    return;
                }

                var remaining = p.BytesPerSecond > 0
                    ? TimeSpan.FromSeconds((totalBytes - p.BytesReceived) / p.BytesPerSecond)
                    : TimeSpan.Zero;

                DownloadStatusText = loc.Format(
                    "Wizard_DownloadProgress",
                    FormatBytes(p.BytesReceived),
                    FormatBytes(totalBytes),
                    speedText,
                    FormatDuration(remaining));
            });

            try
            {
                var path = await _downloadService.DownloadAsync(distro, progress, _downloadCts.Token);
                ResolvedIsoPath = path;
                OnPropertyChanged(nameof(DisplayedDistro));
                Notify?.Invoke(loc["Wizard_InstallSuccessTitle"], loc.Format("Wizard_DownloadCompleted", path), false);
            }
            catch (OperationCanceledException)
            {
                Notify?.Invoke(loc["Wizard_InstallSuccessTitle"], loc["Wizard_DownloadCancelled"], false);
            }
            catch (Exception ex)
            {
                Notify?.Invoke(loc["Wizard_DownloadErrorTitle"], ex.Message, true);
            }
            finally
            {
                IsDownloading = false;
                _downloadCts = null;
            }
        }

        private static string FormatBytes(double bytes)
        {
            const double Mb = 1024 * 1024;
            const double Gb = Mb * 1024;

            return bytes >= Gb ? $"{bytes / Gb:n1} GB" : $"{Math.Max(0, bytes) / Mb:n0} MB";
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            if (duration.TotalMinutes >= 1)
                return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

            return $"{Math.Max(1, duration.Seconds)}s";
        }
    }
}
