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

        private List<UserModel> _allUsers = new List<UserModel>();
        
        public ObservableCollection<UserModel> Users { get; } = new();

        public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 5, 10, 15, 20, 50 };
        
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set {if(SetProperty(ref _currentPage, value)) ApplyPagination();}
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set { if (SetProperty(ref _pageSize, value)) ApplyPagination(); }
        }
        
        private int _totalItems = 0;
        
        public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        
        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Todos", "Usuario", "Trabajador", "Admin" };

        private string _selectedRole = "Todos";
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }
        private string _filterName = "";
        public string FilterName
        {
            get => _filterName;
            set {
                if (SetProperty(ref _filterName, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                } 
            }
        }

        private string _filterDni = "";
        public string FilterDni
        {
            get => _filterDni;
            set
            {
                if (SetProperty(ref _filterDni, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }

        private string _filterEmail = "";
        public string FilterEmail
        {
            get => _filterEmail;
            set {
                if (SetProperty(ref _filterEmail, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();   
                }
            }
        }

        private string _filterPhone = "";
        public string FilterPhone
        {
            get => _filterPhone;
            set
            {
                if (SetProperty(ref _filterPhone, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }

        private bool? _filterVip = null;
        public bool? FilterVip
        {
            get => _filterVip;
            set {
                if (SetProperty(ref _filterVip, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }

        public ICommand ReloadCommand { get; }
        public ICommand CreateNewCommand { get; }
        public ICommand ViewUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UserViewModel()
        {
            _ = LoadUsers();
            
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => HasPreviousPage);
            PreviousPageCommand  = new RelayCommand(_ => GoToPreviousPage(), _ => HasPreviousPage);
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => HasNextPage);
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => HasNextPage);
            GoToPageCommand = new RelayCommand<string>(GoToPage, CanGoToPage);
            
            ReloadCommand = new AsyncRelayCommand(() => LoadUsers());

            CreateNewCommand = new RelayCommand<object>(_ => NavigateToUtilities(UserUtilitiesMode.Create, null));
            ViewUserCommand = new RelayCommand<UserModel>(u => NavigateToUtilities(UserUtilitiesMode.View, u));
            EditUserCommand = new RelayCommand<UserModel>(u => NavigateToUtilities(UserUtilitiesMode.Edit, CloneUser(u)));

            DeleteUserCommand = new AsyncRelayCommand<UserModel>(DeleteUserAsync);
        }
        
        private void GoToFirstPage() => CurrentPage = 1;
        private void GoToPreviousPage() => CurrentPage--;
        private void GoToNextPage() => CurrentPage++;
        private void GoToLastPage() => CurrentPage = TotalPages;
        
        private void GoToPage(string? pageNumber)
        {
            if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
            }
        }
        
        private bool CanGoToPage(string? pageNumber)
        {
            return int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages;
        }

        private List<UserModel> GetFilteredUsers()
        {
            if (_allUsers == null || !_allUsers.Any()) return new List<UserModel>();
            
            return _allUsers.Where(user => FilterUser(user)).ToList();
        }

        private void ApplyFiltersAndPagination()
        {
            var filtered = GetFilteredUsers();
            _totalItems = filtered.Count;
            ApplyPagination();
        }

        private void ApplyPagination()
        {
            var filtered = GetFilteredUsers();
            var paginated = filtered
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            Users.Clear();
            foreach (var booking in paginated)
            {
                Users.Add(booking);
            }
            
            CommandManager.InvalidateRequerySuggested();

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(CurrentPage));
        }
        
        private async Task LoadUsers()
        {
            _allUsers = await _usersService.GetAllAsync();
            CurrentPage = 1;
            ApplyFiltersAndPagination();
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
                await _usersService.DeleteByIdAsync(u.Id);
                Users.Remove(u);
                _allUsers.Remove(u);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Error al eliminar", MessageBoxButton.OK,  MessageBoxImage.Error);
            }
        }
    }
}