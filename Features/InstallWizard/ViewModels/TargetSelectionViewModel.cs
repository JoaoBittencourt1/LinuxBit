using System.IO;
using LinuxHub.Common.Models;
using LinuxHub.Common.Mvvm;
using LinuxHub.Features.InstallWizard.Services;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Modo de instalação (substituir/dual-boot) e seleção de disco ou partição alvo.
    /// Ver specs/install-wizard/spec.md — "Selecionar o alvo da instalação".
    /// </summary>
    public class TargetSelectionViewModel : ObservableObject
    {
        private const double DefaultSliderMaximum = 500;
        private const double DefaultSliderMinimum = 20;

        private readonly IDiskInventoryService _diskInventory;
        private readonly IPartitionInventoryService _partitionInventory;

        private InstallMode _mode = InstallMode.Replace;
        private DiskInfo? _selectedDisk;
        private PartitionInfo? _selectedPartition;
        private double _linuxPartitionSizeGb = 100;
        private double _sliderMaximum = DefaultSliderMaximum;

        public TargetSelectionViewModel(IDiskInventoryService diskInventory, IPartitionInventoryService partitionInventory)
        {
            _diskInventory = diskInventory ?? throw new ArgumentNullException(nameof(diskInventory));
            _partitionInventory = partitionInventory ?? throw new ArgumentNullException(nameof(partitionInventory));

            IsUefi = Directory.Exists(@"C:\Windows\Boot\EFI");

            ReloadDisks();
        }

        public bool IsUefi { get; }

        public InstallMode Mode
        {
            get => _mode;
            set
            {
                if (!SetProperty(ref _mode, value))
                    return;

                OnPropertyChanged(nameof(IsReplaceMode));
                OnPropertyChanged(nameof(IsDualBootMode));

                if (value == InstallMode.Replace)
                    ReloadDisks();
                else
                    ReloadPartitions();
            }
        }

        public bool IsReplaceMode => Mode == InstallMode.Replace;
        public bool IsDualBootMode => Mode == InstallMode.DualBoot;

        public IReadOnlyList<DiskInfo> Disks { get; private set; } = Array.Empty<DiskInfo>();
        public IReadOnlyList<PartitionInfo> Partitions { get; private set; } = Array.Empty<PartitionInfo>();

        public DiskInfo? SelectedDisk
        {
            get => _selectedDisk;
            set => SetProperty(ref _selectedDisk, value);
        }

        public PartitionInfo? SelectedPartition
        {
            get => _selectedPartition;
            set
            {
                if (!SetProperty(ref _selectedPartition, value))
                    return;

                long sizeGb = value is null ? 0 : value.SizeBytes / (1024 * 1024 * 1024);
                SliderMaximum = Math.Max(sizeGb, DefaultSliderMinimum);
                if (LinuxPartitionSizeGb > SliderMaximum)
                    LinuxPartitionSizeGb = SliderMaximum;
            }
        }

        public double SliderMinimum => DefaultSliderMinimum;

        public double SliderMaximum
        {
            get => _sliderMaximum;
            private set => SetProperty(ref _sliderMaximum, value);
        }

        public double LinuxPartitionSizeGb
        {
            get => _linuxPartitionSizeGb;
            set => SetProperty(ref _linuxPartitionSizeGb, value);
        }

        private void ReloadDisks()
        {
            Disks = _diskInventory.GetDisks();
            OnPropertyChanged(nameof(Disks));
            SelectedDisk = Disks.Count > 0 ? Disks[0] : null;
        }

        private void ReloadPartitions()
        {
            Partitions = _partitionInventory.GetEligiblePartitions();
            OnPropertyChanged(nameof(Partitions));
            SelectedPartition = Partitions.Count > 0 ? Partitions[0] : null;
        }
    }
}
