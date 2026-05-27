using System.Windows;
using desktop_app.Commands;
using desktop_app.Services;
using System.Windows.Input;
using desktop_app.Models;
using desktop_app.Views;

namespace desktop_app.ViewModels
{
    /// <summary>
    /// ViewModel para MainWindow.xaml
    /// Contiene los comandos para la navegación de la aplicación
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Comando que navega a la vista de usuarios
        /// </summary>
        public ICommand ShowUsersCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<UserView>());

        /// <summary>
        /// Comando que navega a la vista de reservas
        /// </summary>
        public ICommand ShowBookingsCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<BookingView>());

        /// <summary>
        /// Comando que navega a la vista de habitaciones
        /// </summary>
        public ICommand ShowRoomsCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<RoomView>());
        
        /// <summary>
        /// Comando que navega a la vista de facturas
        /// </summary>
        public ICommand ShowInvoicesCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<InvoicesView>());

        /// <summary>
        /// Comando que navega a la vista de notificaciones
        /// </summary>
        public ICommand ShowNotificationsCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<NotificationsView>());

        /// <summary>
        /// Comando que navega a la vista de facturas
        /// </summary>
        public ICommand ShowAuditCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<AuditView>());
        
        /// <summary>
        /// Comando que navega a la vista de pagos
        /// </summary>
        public ICommand ShowPaymentsCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<PaymentsView>());

        public ICommand LogoutCommand { get; }

        private static String _username;
        public String UserName
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        
        private String _imageRoute;
        public String ImageRoute
        {
            get => _imageRoute;
            set
            {
                _imageRoute = value;
                OnPropertyChanged();
            }
        }

        private string _userRol = "";
        public String UserRol
        {
            get => _userRol;
            set
            {
                _userRol = value;
                OnPropertyChanged();
            }
        }
        

        public MainViewModel()
        {
            LoadUser();
            
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private async Task LoadUser()
        {
            try
            {
                var loggedUser = await UserService.GetClientByIdAsync(TokenStore.UserId);

                UserName = $"{loggedUser.FirstName} {loggedUser.LastName}";

                if (!string.IsNullOrEmpty(loggedUser.ImageRoute))
                {
                    ImageRoute = ApiService.BaseUrl.Substring(0, ApiService.BaseUrl.Length - 1) + loggedUser.ImageRoute;
                }

                UserRol = loggedUser.Rol;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private void Logout()
        {
            TokenStore.AccessToken = null;
            
            var currentWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
    
            var loginWindow = new LoginView();
            loginWindow.Show();
    
            currentWindow?.Close();
        }
    }
}
