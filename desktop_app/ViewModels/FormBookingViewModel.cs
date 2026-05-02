using System.Windows.Controls;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Services;
using desktop_app.Views;
using desktop_app.Views.BookingViews;

namespace desktop_app.ViewModels
{
    public class FormBookingViewModel : ViewModelBase
    {
        private static FormBookingViewModel? _instance;
        /// <summary>
        /// Obtiene la instancia única del ViewModel (patrón Singleton).
        /// </summary>
        public static FormBookingViewModel Instance => _instance ??= new FormBookingViewModel();

        /// <summary>
        /// Obtiene el título de la ventana basado en si la reserva es nueva o existente.
        /// </summary>
        public string Title => string.IsNullOrEmpty(BookingId) ? "Crear reserva" : "Actualizar reserva";
        
        private String _bookingId;
        /// <summary>
        /// Obtiene o establece el ID de reserva actual.
        /// </summary>
        public String BookingId
        {
            get => _bookingId;
            set
            {
                _bookingId = value;
                OnPropertyChanged(nameof(BookingId));
                OnPropertyChanged(nameof(Title));
            }
        }

        private UserControl _currentView;

        public UserControl CurrentView
        {
            get => _currentView;
            set => _currentView = value;
        }


        private FormBookingViewModel()
        {
            BookingId = "";
            CurrentView = new UpdateView();
            NavigateToDetailsCommand = new RelayCommand(NavigateToDetails);
        }

        public void NavigateToDetails(object obj)
        {
            CurrentView = new UpdateView();
            UpdateBookingFormViewModel.Instance.BookingId = BookingId;
        }
        
        public ICommand ReturnCommand { get; } =
            new RelayCommand(_ =>
                NavigationService.Instance.NavigateTo<BookingView>());

        public ICommand NavigateToDetailsCommand { get; }
    }
}
