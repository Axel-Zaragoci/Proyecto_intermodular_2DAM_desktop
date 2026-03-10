using desktop_app.Commands;
using desktop_app.Services;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace desktop_app.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly LoginService _loginService = new LoginService();

        private string? _email;
        private string? _password;
        private string _statusText = "";
        private bool _isBusy;
        private bool _isPasswordVisible;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// La vista asigna esta acción para navegar tras login correcto.
        /// </summary>
        public Action? OnLoginSuccess { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
            TogglePasswordVisibilityCommand = new RelayCommand(_ => IsPasswordVisible = !IsPasswordVisible);
        }

        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Password que usa el LoginCommand.
        /// - PasswordTextBox lo bindea TwoWay
        /// - PasswordBox se sincroniza desde code-behind
        /// </summary>
        public string? Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensaje de estado para informar al usuario sobre el resultado del login
        /// o posibles errores durante la operación.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Indica si una operación está en curso y controla el estado de la UI.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// True => se muestra PasswordTextBox
        /// False => se muestra PasswordBox
        /// </summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        /// <summary>
        /// Realiza el inicio de sesión del usuario de forma asíncrona.
        /// </summary>
        /// <remarks>
        /// Valida las credenciales, ejecuta el login contra el servicio,
        /// verifica el rol del usuario y notifica el éxito del proceso.
        /// Maneja errores de red y autenticación y controla el estado de carga.
        /// </remarks>
        private async Task LoginAsync()
        {
            StatusText = "";
            IsBusy = true;

            try
            {
                var email = Email?.Trim();
                var password = Password;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    StatusText = "Email y Contraseña son obligatorios.";
                    return;
                }

                var rol = await _loginService.LoginAsync(email, password);

                if (string.Equals(rol, "Usuario"))
                {
                    StatusText = "No tienes permisos para acceder.";
                    return;
                }

                OnLoginSuccess?.Invoke();
            }
            catch (HttpRequestException ex)
            {
                StatusText = $"Error de red: {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}