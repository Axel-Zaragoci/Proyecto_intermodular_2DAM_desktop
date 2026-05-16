using System.Windows;
using System.Windows.Controls;
using desktop_app.ViewModels.Room;

namespace desktop_app.Views
{
    public partial class RoomView : UserControl
    {
        public RoomView()
        {
            InitializeComponent();

            // Conectamos View con ViewModel
            DataContext = new RoomViewModel();
            
            // Recargar datos cada vez que la vista se hace visible
            IsVisibleChanged += RoomView_IsVisibleChanged;
        }

        private void RoomView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Si la vista se hace visible, refrescar los datos
            if (e.NewValue is true && DataContext is RoomViewModel vm)
            {
                vm.RefreshCommand.Execute(null);
            }
        }
    }
}
