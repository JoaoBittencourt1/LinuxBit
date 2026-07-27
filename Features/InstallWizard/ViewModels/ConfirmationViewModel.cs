using System.Windows.Input;
using LinuxHub.Common.Mvvm;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Passo de confirmação destrutiva exibido antes de qualquer operação de disco
    /// (constitution §6). Para o modo substituir exige digitar a palavra de
    /// confirmação; para o shrink (dual-boot) um clique simples basta, já que a
    /// operação é reversível. Ver specs/disk-provisioning/spec.md — "Exigir
    /// confirmação explícita antes de qualquer operação de disco".
    /// </summary>
    public sealed class ConfirmationViewModel : ObservableObject
    {
        private readonly RelayCommand _confirmCommand;
        private string _typedConfirmation = string.Empty;

        public ConfirmationViewModel(string summary, bool requiresTypedConfirmation, string confirmationWord)
        {
            Summary = summary;
            RequiresTypedConfirmation = requiresTypedConfirmation;
            ConfirmationWord = confirmationWord;

            _confirmCommand = new RelayCommand(() => Confirmed?.Invoke(), () => CanConfirm);
            CancelCommand = new RelayCommand(() => Cancelled?.Invoke());
        }

        public string Summary { get; }
        public bool RequiresTypedConfirmation { get; }
        public string ConfirmationWord { get; }

        public string TypedConfirmation
        {
            get => _typedConfirmation;
            set
            {
                if (SetProperty(ref _typedConfirmation, value))
                    _confirmCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanConfirm => !RequiresTypedConfirmation ||
            string.Equals(TypedConfirmation.Trim(), ConfirmationWord, StringComparison.OrdinalIgnoreCase);

        public ICommand ConfirmCommand => _confirmCommand;
        public ICommand CancelCommand { get; }

        public event Action? Confirmed;
        public event Action? Cancelled;
    }
}
