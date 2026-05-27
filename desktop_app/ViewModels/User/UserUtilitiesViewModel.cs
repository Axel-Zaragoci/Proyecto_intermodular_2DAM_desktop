using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using desktop_app.Commands;
using desktop_app.Models;
using System.Windows.Input;
using desktop_app.Services;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace desktop_app.ViewModels.User
{
    public enum UserUtilitiesMode { Create, View, Edit }

    public class UserUtilitiesViewModel : ViewModelBase
    {
        private readonly UsersService _usersService = new();

        public UserUtilitiesMode Mode { get; }
        public bool IsReadOnly => Mode == UserUtilitiesMode.View;

        public UserModel User { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        
        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public UserUtilitiesViewModel(UserUtilitiesMode mode, UserModel? user = null)
        {
            Mode = mode;

            User = user ?? new UserModel
            {
                Rol = "Usuario",
                BirthDate = DateTime.Today.AddYears(-16),
                VipStatus = false
            };

            if (!string.IsNullOrEmpty(User.ImageRoute))
            {
                ImageRoute = ApiService.BaseUrl.Substring(0, ApiService.BaseUrl.Length - 1) + User.ImageRoute;
            }
            
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => Mode != UserUtilitiesMode.View);
            CancelCommand = new RelayCommand<object>(_ => Cancel());
            PickImageCommand = new RelayCommand(_ => PickImageLocal());
        }
        
        private String Validate()
        {
            if (string.IsNullOrWhiteSpace(User.FirstName))
            {
                return "El nombre no puede estar vacío.";
            }

            if (string.IsNullOrWhiteSpace(User.LastName))
            {
                return "El apellido no puede estar vacío.";
            }

            if (string.IsNullOrWhiteSpace(User.Email))
            {
                return "El correo no puede estar vacío.";
            }

            var emailOk = Regex.IsMatch(User.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailOk)
            {
                return "El correo no tiene un formato válido.";
            }

            if (string.IsNullOrWhiteSpace(User.Dni))
            {
                return "El dni no puede estar vacío.";
            }

            if (User.BirthDate == default)
            {
                return "La fecha no puede estar vacía.";
            }

            if (string.IsNullOrWhiteSpace(User.CityName))
            {
                return "La ciudad no puede estar vacía.";
            }

            if (string.IsNullOrWhiteSpace(User.Gender))
            {
                return "Tienes que seleccionar un género.";
            }

            return "";
        }
        private async Task SaveAsync()
        {
            ErrorMessage = null;
            var err = Validate();
            if (!string.IsNullOrWhiteSpace(err))
            {
                ErrorMessage = err;
                return;
            }
            
            if (!string.IsNullOrWhiteSpace(_imagePath))
            {
                var uploaded = await ImageService.UploadSingleAsync(_imagePath);
                if (uploaded == null)
                {
                    ErrorMessage = "No se puedo subir la imagen";
                    return;
                }
                User.ImageRoute = uploaded.Url;
            }

            try
            {
                await _usersService.SaveAsync(Mode, User);
                NavigationService.Instance.NavigateTo<desktop_app.Views.UserView>();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void Cancel()
        {
            NavigationService.Instance.NavigateTo<desktop_app.Views.UserView>();
        }
        
        
        
        
        
        
        private string _imageLabel = "Sin seleccionar";
        private string _imagePath = string.Empty;
        
        private string _imageRoute = string.Empty;
        public string ImageRoute
        {
            get => _imageRoute;
            set
            {
                SetProperty(ref _imageRoute, value);
                OnPropertyChanged();
            }
        }
        
        public string ImageLabel
        {
            get => _imageLabel;
            set => SetProperty(ref _imageLabel, value);
        }
        
        public ICommand PickImageCommand { get; }

        private void PickImageLocal()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.webp;*.gif",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            var path = dlg.FileName;
            _imagePath = path;
            ImageLabel = Path.GetFileName(path);
            ImageRoute = path;
        }
    }
}
