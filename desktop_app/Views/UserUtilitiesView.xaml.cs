using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using desktop_app.ViewModels;
using static desktop_app.Services.UsersService;

namespace desktop_app.Views
{
    /// <summary>
    /// Lógica de interacción para UserUtilitiesView.xaml
    /// </summary>
    public partial class UserUtilitiesView : UserControl
    {
        public UserUtilitiesView()
        {
            InitializeComponent();
            IsVisibleChanged += UserUtilitiesView_IsVisibleChanged;
        }

        private void UserUtilitiesView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                DataContext = new UserUtilitiesViewModel(
                    UserNavigationContext.Mode,
                    UserNavigationContext.User
                );
            }
        }
    }
}
