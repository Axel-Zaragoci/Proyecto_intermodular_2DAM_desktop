using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static desktop_app.Services.UsersService;

namespace desktop_app.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        private readonly UsersService _usersService = new();

        public ObservableCollection<UserModel> Users { get; } = new();
        public ICollectionView UsersView { get; }
        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Todos", "Usuario", "Trabajador", "Admin" };

        private string _selectedRole = "Todos";
        public string SelectedRole
        {
            get => _selectedRole;
            set { if (SetProperty(ref _selectedRole, value)) UsersView.Refresh(); }
        }
        private string _filterName = "";
        public string FilterName
        {
            get => _filterName;
            set { if (SetProperty(ref _filterName, value)) UsersView.Refresh(); }
        }

        private string _filterDni = "";
        public string FilterDni
        {
            get => _filterDni;
            set { if (SetProperty(ref _filterDni, value)) UsersView.Refresh(); }
        }

        private string _filterEmail = "";
        public string FilterEmail
        {
            get => _filterEmail;
            set { if (SetProperty(ref _filterEmail, value)) UsersView.Refresh(); }
        }

        private string _filterPhone = "";
        public string FilterPhone
        {
            get => _filterPhone;
            set { if (SetProperty(ref _filterPhone, value)) UsersView.Refresh(); }
        }

        private bool? _filterVip = null;
        public bool? FilterVip
        {
            get => _filterVip;
            set { if (SetProperty(ref _filterVip, value)) UsersView.Refresh(); }
        }

        public ICommand ReloadCommand { get; }
        public ICommand CreateNewCommand { get; }
        public ICommand ViewUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UserViewModel()
        {
            UsersView = CollectionViewSource.GetDefaultView(Users);
            UsersView.Filter = FilterUser;

            ReloadCommand = new AsyncRelayCommand(() => _usersService.ReloadIntoAsync(Users, UsersView));

            CreateNewCommand = new RelayCommand<object>(_ => NavigateToUtilities(UserUtilitiesMode.Create, null));
            ViewUserCommand = new RelayCommand<UserModel>(u => NavigateToUtilities(UserUtilitiesMode.View, u));
            EditUserCommand = new RelayCommand<UserModel>(u => NavigateToUtilities(UserUtilitiesMode.Edit, CloneUser(u)));

            DeleteUserCommand = new AsyncRelayCommand<UserModel>(DeleteUserAsync);
            _ = _usersService.ReloadIntoAsync(Users, UsersView);
        }

        private bool FilterUser(object obj)
        {
            if (obj is not UserModel u) return false;

            bool Match(string? value, string filter)
            {
                if (string.IsNullOrWhiteSpace(filter)) return true;
                return (value ?? "").IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
            }

            var fullName = $"{u.FirstName} {u.LastName}".Trim();

            bool roleOk = SelectedRole == "Todos" || string.Equals(u.Rol ?? "", SelectedRole, StringComparison.OrdinalIgnoreCase);

            bool isVip = u.VipStatus == true;

            bool vipOk = FilterVip switch
            {
                null => true,
                true => isVip,
                false => !isVip
            };

            return Match(fullName, FilterName) && Match(u.Dni, FilterDni) && Match(u.Email, FilterEmail) && Match(u.PhoneNumber + "", FilterPhone) && roleOk && vipOk;
        }

        private void NavigateToUtilities(UserUtilitiesMode mode, UserModel? user)
        {
            UserNavigationContext.Mode = mode;
            UserNavigationContext.User = user;
            NavigationService.Instance.NavigateTo<desktop_app.Views.UserUtilitiesView>();
        }

        private static UserModel CloneUser(UserModel user)
        {
            return new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Dni = user.Dni,
                PhoneNumber = user.PhoneNumber,
                CityName = user.CityName,
                ImageRoute = user.ImageRoute,
                Rol = user.Rol,
                VipStatus = user.VipStatus,
                BirthDate = user.BirthDate,
                Gender = user.Gender
            };
        }
        private async Task DeleteUserAsync(UserModel? u)
        {
            if (u is null) return;

            var fullName = $"{u.FirstName} {u.LastName}".Trim();
            var text = $"¿Seguro que quieres eliminar al usuario " + fullName + "?";

            var result = MessageBox.Show(text,"Confirmar eliminación",  MessageBoxButton.YesNo,  MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _usersService.DeleteAndRemoveAsync(u, Users, UsersView);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Error al eliminar", MessageBoxButton.OK,  MessageBoxImage.Error);
            }
        }
    }
}