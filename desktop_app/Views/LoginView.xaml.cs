using System;
using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using desktop_app.Services;
using desktop_app.ViewModels;

namespace desktop_app.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _vm;

        /// <summary>
        /// Inicializa la vista de login y configura el enlace con su ViewModel.
        /// </summary>
        public LoginView()
        {
            InitializeComponent();

            _vm = new LoginViewModel();
            _vm.OnLoginSuccess = () =>
            {
                new MainWindow().Show();
                Close();
            };

            DataContext = _vm;
            PasswordBox.PasswordChanged += (_, __) => {if (!_vm.IsPasswordVisible)_vm.Password = PasswordBox.Password;};
            _vm.PropertyChanged += VmOnPropertyChanged;
        }

        /// <summary>
        /// Maneja los cambios de propiedades del ViewModel relevantes para la vista.
        /// </summary>
        private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LoginViewModel.IsPasswordVisible)) return;

            if (_vm.IsPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
                PasswordTextBox.Focus();
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Focus();
            }
        }
    }
}
