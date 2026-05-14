using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Events;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels
{
    public class BookingViewModel : ViewModelBase
    {
        /// <summary>
        /// Colección de las reservas que se conecta a la tabla de View
        /// </summary>
        public ObservableCollection<BookingModel> Bookings { get; }
        
        public ICollectionView BookingsView { get; set; }
        
        /// <summary>
        /// Comando para el botón de eliminar reserva
        /// </summary>
        public ICommand DeleteBookingCommand { get; }
        
        
        /// <summary>
        /// Comando para el botón de editar reserva
        /// </summary>
        public ICommand EditBookingCommand { get; }
        
        
        /// <summary>
        /// Comando para el botón de crear reserva
        /// </summary>
        public ICommand CreateBookingCommand { get; }
        
        
        /// <summary>
        /// Comando para el botón de recargar
        /// </summary>
        public ICommand ReloadBookingCommand { get; }


        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string> { "Abierta", "Cancelada", "Todos" };
        private string _selectedStatus = "Todos";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set {if (SetProperty(ref _selectedStatus, value)) BookingsView.Refresh();}
        }
        
        private string _filterName = "";
        public string FilterName
        {
            get => _filterName;
            set {if (SetProperty(ref _filterName, value)) BookingsView.Refresh();}
        }

        
        private string _filterRoom = "";
        public string FilterRoom
        {
            get => _filterRoom;
            set {if (SetProperty(ref _filterRoom, value)) BookingsView.Refresh();}
        }

        private DateTime _selectedStartDate = DateTime.Now;
        public DateTime SelectedStartDate
        {
            get => _selectedStartDate;
            set {if (SetProperty(ref _selectedStartDate, value)) BookingsView.Refresh();}
        }
        
        private DateTime _selectedEndDate = DateTime.Now;
        public DateTime SelectedEndDate
        {
            get => _selectedEndDate;
            set {if (SetProperty(ref _selectedEndDate, value)) BookingsView.Refresh();}
        }
        
        public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
        
        private string _selectedStartDateFilter = "Después de";
        public string SelectedStartDateFilter
        {
            get => _selectedStartDateFilter;
            set {if (SetProperty(ref _selectedStartDateFilter, value)) BookingsView.Refresh();}
        }
        
        private string _selectedEndDateFilter = "Después de";
        public string SelectedEndDateFilter
        {
            get => _selectedEndDateFilter;
            set {if (SetProperty(ref _selectedEndDateFilter, value)) BookingsView.Refresh();}
        }
        
        /// <summary>
        /// Constructor del ViewModel
        /// Se encarga de
        ///     - Cargar las reservas en la colección
        ///     - Crear los comandos
        ///     - Añadir el evento para volver a cargar
        /// </summary>
        public BookingViewModel()
        {
            Bookings = new ObservableCollection<BookingModel>();
            BookingsView = CollectionViewSource.GetDefaultView(Bookings);
            BookingsView.Filter = FilterBookings;
            _ = LoadBookingsAsync();
            DeleteBookingCommand = new AsyncRelayCommand<BookingModel>(DeleteBookingAsync);
            EditBookingCommand = new RelayCommand(EditBooking);
            CreateBookingCommand = new RelayCommand(CreateBooking);
            ReloadBookingCommand = new AsyncRelayCommand(LoadBookingsAsync);
            BookingEvents.OnBookingChanged += async () => await LoadBookingsAsync();
        }

        private bool FilterBookings(object obj)
        {
            if (obj is not BookingModel booking) return false;

            bool Match(string? value, string filter)
            {
                if (string.IsNullOrWhiteSpace(filter)) return true;
                return (value ?? "").IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
            }

            bool nameMatch = Match(booking.ClientName, FilterName);
            bool roomMatch = Match(booking.RoomNumber, FilterRoom);
            string status = SelectedStatus == "Todos" ? "" : SelectedStatus;
            bool statusMatch = Match(booking.Status, status);

            bool startDateMatch = true;
            DateTime startDate = booking.CheckInDate;
            switch (_selectedStartDateFilter)
            {
                case "Fecha exacta":
                    startDateMatch = startDate.Date == SelectedStartDate.Date;
                    break;
                
                case "Antes de":
                    startDateMatch = startDate.Date <= SelectedStartDate.Date;
                    break;
                
                case "Después de":
                    startDateMatch = startDate.Date >= SelectedStartDate.Date;
                    break;
            }
            
            bool endDateMatch = true;
            DateTime endDate = booking.CheckOutDate;
            switch (_selectedEndDateFilter)
            {
                case "Fecha exacta":
                    endDateMatch = endDate.Date == SelectedEndDate.Date;
                    break;
                
                case "Antes de":
                    endDateMatch = endDate.Date <= SelectedEndDate.Date;
                    break;
                
                case "Después de":
                    endDateMatch = endDate.Date >= SelectedEndDate.Date;
                    break;
            }

            bool result = nameMatch && roomMatch && statusMatch && startDateMatch && endDateMatch;
            return result;
        }
        
        
        /// <summary>
        /// Método que carga las reservas
        /// Obtiene todas las reservas y vacía la lista para evitar duplicados
        /// Obtiene los datos necesarios para mostrar la información en la UI
        /// Añade las reservas con los datos extra a la lista de reservas
        /// </summary>
        private async Task LoadBookingsAsync()
        {
            try
            {
                var list = await BookingService.GetAllBookingsAsync();
                Bookings.Clear();
                foreach (var booking in list)
                {
                    UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                    booking.ClientDni = u.Dni;
                    booking.ClientName = u.FirstName + " " + u.LastName;
                    RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                    booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                    Bookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        
        /// <summary>
        /// Método al que referencia el comando de eliminar
        /// Requiere de confirmación mediante MesageBox
        /// </summary>
        /// 
        /// <param name="booking">
        /// Recibe el modelo que debe eliminar
        /// </param>
        private async Task DeleteBookingAsync(BookingModel booking)
        {
            var result = MessageBox.Show("¿Seguro que quieres eliminar esta reserva?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                bool deleted = await BookingService.DeleteBooking(booking.Id);
                
                if (deleted)
                {
                    Bookings.Remove(booking);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la reserva", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        
        /// <summary>
        /// Método al que referencia el comando de editar
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parametro a editar que se revisa que debe ser una reserva
        /// </param>
        private void EditBooking(object? parameter)
        {
            if (parameter is not BookingModel booking) return;
            NavigationService.Instance.NavigateTo<FormBookingView>();
            FormBookingViewModel.Instance.BookingId = booking.Id;
            FormBookingViewModel.Instance.NavigateToDetails(new Object());
        }

        
        /// <summary>
        /// Método al que hace referencia el comando de crear
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parametro necesario para los RelayCommand
        /// </param>
        private void CreateBooking(object? parameter)
        {
            NavigationService.Instance.NavigateTo<FormBookingView>();
            FormBookingViewModel.Instance.BookingId = "";
            FormBookingViewModel.Instance.NavigateToDetails(new Object());
        }
    }
}
