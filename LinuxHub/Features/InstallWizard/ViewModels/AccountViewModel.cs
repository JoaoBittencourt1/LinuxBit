using System.Windows.Input;
using LinuxHub.Common.Mvvm;

namespace LinuxHub.Features.InstallWizard.ViewModels
{
    /// <summary>
    /// Dados da conta de usuário a ser criada pelo instalador. Ver
    /// specs/install-wizard/spec.md — "Coletar dados da conta de usuário".
    /// </summary>
    public class AccountViewModel : ObservableObject
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _hostname = string.Empty;
        private bool _isPasswordVisible;
        private bool _isConfirmPasswordVisible;

        public AccountViewModel()
        {
            TogglePasswordVisibilityCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    OnPropertyChanged(nameof(PasswordsMismatch));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                    OnPropertyChanged(nameof(PasswordsMismatch));
            }
        }

        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        public bool PasswordsMismatch =>
            !string.IsNullOrEmpty(ConfirmPassword) && ConfirmPassword != Password;

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            private set => SetProperty(ref _isPasswordVisible, value);
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            private set => SetProperty(ref _isConfirmPasswordVisible, value);
        }

        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
    }
}
