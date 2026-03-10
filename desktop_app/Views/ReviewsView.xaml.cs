using System.Windows.Controls;
using desktop_app.Models;
using desktop_app.ViewModels;

namespace desktop_app.Views
{
    /// <summary>
    /// Vista para mostrar las reseñas de una habitación.
    /// </summary>
    public partial class ReviewsView : UserControl
    {
        public ReviewsView(RoomModel room)
        {
            InitializeComponent();
            DataContext = new ReviewsViewModel(room);
        }
    }
}
