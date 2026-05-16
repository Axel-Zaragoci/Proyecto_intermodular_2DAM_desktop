using System.Windows.Controls;
using desktop_app.Models;
using desktop_app.ViewModels;

namespace desktop_app.Views
{
    public partial class UpdateRoomWindow : UserControl
    {
        public UpdateRoomWindow()
        {
            InitializeComponent();
            DataContext = desktop_app.ViewModels.Room.UpdateRoomViewModel.Instance;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Forzar el foco al título para que el ScrollViewer empiece arriba
            HeaderMain.Focus();
        }
    }
}
