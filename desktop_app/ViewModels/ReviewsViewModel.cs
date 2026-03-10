using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace desktop_app.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de reseñas de una habitación.
    /// </summary>
    public class ReviewsViewModel : ViewModelBase
    {
        private RoomModel _room;
        /// <summary>Habitación actual.</summary>
        public RoomModel Room
        {
            get => _room;
            set => SetProperty(ref _room, value);
        }

        /// <summary>Lista de reseñas.</summary>
        public ObservableCollection<ReviewModel> Reviews { get; } = new();

        private bool _isEmpty = true;
        /// <summary>Indica si no hay reseñas.</summary>
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        /// <summary>Comando para volver a habitaciones.</summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>Comando para eliminar una reseña.</summary>
        public ICommand DeleteReviewCommand { get; }

        /// <summary>
        /// Inicializa el ViewModel con una habitación.
        /// </summary>
        /// <param name="room">Habitación de la que mostrar reseñas.</param>
        public ReviewsViewModel(RoomModel room)
        {
            _room = room;

            CancelCommand = new RelayCommand(_ => NavigationService.Instance.NavigateTo<RoomView>());

            DeleteReviewCommand = new RelayCommand(
                async (p) => await DeleteReviewAsync(p),
                (p) => p is ReviewModel
            );

            _ = LoadReviewsAsync();
        }

        /// <summary>
        /// Carga las reseñas de la habitación.
        /// </summary>
        private async Task LoadReviewsAsync()
        {
            Reviews.Clear();
            var reviews = await ReviewService.GetReviewsByRoomAsync(Room.Id);

            foreach (var r in reviews)
                Reviews.Add(r);

            IsEmpty = Reviews.Count == 0;
        }

        /// <summary>
        /// Elimina una reseña.
        /// </summary>
        private async Task DeleteReviewAsync(object p)
        {
            if (p is not ReviewModel review) return;

            var confirm = MessageBox.Show(
                "¿Estás seguro de que quieres eliminar esta reseña?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var success = await ReviewService.DeleteReviewAsync(review.Id);

            if (success)
            {
                await LoadReviewsAsync();
            }
            else
            {
                MessageBox.Show("No se pudo eliminar la reseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
