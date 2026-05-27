using desktop_app.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace desktop_app.Views
{
    /// <summary>
    /// Lógica de interacción para CreateRoomWindow.xaml
    /// </summary>
    public partial class CreateRoomWindow : UserControl
    {
        public CreateRoomWindow()
        {
            InitializeComponent();
            var vm = new desktop_app.ViewModels.Room.CreateRoomViewModel();
            DataContext = vm;
        }

    }
}
