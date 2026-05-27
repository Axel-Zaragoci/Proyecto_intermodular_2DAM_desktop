using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Events;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels.Booking
{
    public class BookingViewModel : ViewModelBase
    {
        /// <summary>
        /// Colección completa de reservas (sin paginar)
        /// </summary>
        private List<BookingModel> _allBookings = new List<BookingModel>();
        
        /// <summary>
        /// Colección paginada de las reservas
        /// </summary>
        public ObservableCollection<BookingModel> Bookings { get; }

        /// <summary>
        /// Propiedad de la página actual
        /// </summary>
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set {if(SetProperty(ref _currentPage, value)) ApplyPagination();}
        }

        /// <summary>
        /// Propiedad que maneja la cantidad de registros por página
        /// </summary>
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set { if (SetProperty(ref _pageSize, value)) ApplyPagination(); }
        }
        
        /// <summary>
        /// Total de registros a mostrar
        /// </summary>
        private int _totalItems = 0;
        
        /// <summary>
        /// Total de páginas
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
        /// <summary>
        /// Valores para permitir la navegación entre páginas de datos
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        /// <summary>
        /// Comandos para la paginación
        /// </summary>
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        
        /// <summary>
        /// Posibles cantidades de registros por página
        /// </summary>
        public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 5, 10, 15, 20, 50 };

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

        /// <summary>
        /// Lista de posibles estados y la propiedad para poder filtrar por estado en la vista
        /// </summary>
        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string> { "Abierta", "Cancelada", "Check-in", "Check-out", "Todos" };
        private string _selectedStatus = "Todos";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }
        
        /// <summary>
        /// Propiedad para filtrar por nombre del cliente
        /// </summary>
        private string _filterName = "";
        public string FilterName
        {
            get => _filterName;
            set
            {
                if (SetProperty(ref _filterName, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }

        /// <summary>
        /// Propiedad para filtrar por número de habitación
        /// </summary>
        private string _filterRoom = "";
        public string FilterRoom
        {
            get => _filterRoom;
            set
            {
                if (SetProperty(ref _filterRoom, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }

        /// <summary>
        /// Propiedad para filtrar por fecha de inicio
        /// </summary>
        private DateTime _selectedStartDate = DateTime.Now;
        public DateTime SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                if (SetProperty(ref _selectedStartDate, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }
        
        /// <summary>
        /// Propiedad para filtrar por fecha de fin
        /// </summary>
        private DateTime _selectedEndDate = DateTime.Now;
        public DateTime SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                if (SetProperty(ref _selectedEndDate, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }
        
        /// <summary>
        /// Lista de tipos de acciones de filtrado para fechas
        /// </summary>
        public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
        
        /// <summary>
        /// Propiedad para el tipo de acción de filtrado para la fecha de inicio
        /// </summary>
        private string _selectedStartDateFilter = "Después de";
        public string SelectedStartDateFilter
        {
            get => _selectedStartDateFilter;
            set
            {
                if (SetProperty(ref _selectedStartDateFilter, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
        }
        
        /// <summary>
        /// Propiedad para el tipo de acción de filtrado para la fecha de fin
        /// </summary>
        private string _selectedEndDateFilter = "Después de";
        public string SelectedEndDateFilter
        {
            get => _selectedEndDateFilter;
            set
            {
                if (SetProperty(ref _selectedEndDateFilter, value))
                {
                    CurrentPage = 1;
                    ApplyFiltersAndPagination();
                }
            }
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
            _ = LoadBookingsAsync();
            
            Bookings = new ObservableCollection<BookingModel>();
            
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => HasPreviousPage);
            PreviousPageCommand  = new RelayCommand(_ => GoToPreviousPage(), _ => HasPreviousPage);
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => HasNextPage);
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => HasNextPage);
            GoToPageCommand = new RelayCommand<string>(GoToPage, CanGoToPage);

            DeleteBookingCommand = new AsyncRelayCommand<BookingModel>(DeleteBookingAsync);
            EditBookingCommand = new RelayCommand(EditBooking);
            CreateBookingCommand = new RelayCommand(CreateBooking);
            ReloadBookingCommand = new AsyncRelayCommand(LoadBookingsAsync);
            
            BookingEvents.OnBookingChanged += async () => await LoadBookingsAsync();
        }

        /// <summary>
        /// Funciones para saber si se puede navegar en la paginación
        /// </summary>
        private void GoToFirstPage() => CurrentPage = 1;
        private void GoToPreviousPage() => CurrentPage--;
        private void GoToNextPage() => CurrentPage++;
        private void GoToLastPage() => CurrentPage = TotalPages;
        
        /// <summary>
        /// Función para navegar a una página en concreto de los datos
        /// </summary>
        /// <param name="pageNumber"></param>
        private void GoToPage(string? pageNumber)
        {
            if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
            }
        }
        
        /// <summary>
        /// Función para saber si puede navegar a una página específica
        /// </summary>
        /// 
        /// <param name="pageNumber">
        /// Número de página a la que quiere navegar en texto
        /// </param>
        /// 
        /// <returns>
        /// Valor booleano:
        ///     - true -> Puede navegar
        ///     - false -> No puede navegar
        /// </returns>
        private bool CanGoToPage(string? pageNumber)
        {
            return int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages;
        }
        
        /// <summary>
        /// Obtiene la lista de reservas filtradas
        /// </summary>
        /// 
        /// <returns>
        /// Lista de reservas pasadas por los filtros seleccionados
        /// </returns>
        private List<BookingModel> GetFilteredBookings()
        {
            if (_allBookings == null || !_allBookings.Any())
                return new List<BookingModel>();
                
            return _allBookings.Where(booking => FilterBookings(booking)).ToList();
        }
        
        /// <summary>
        /// Aplica los filtros y pagina los resultados
        /// </summary>
        private void ApplyFiltersAndPagination()
        {
            var filtered = GetFilteredBookings();
            _totalItems = filtered.Count;
            ApplyPagination();
        }

        /// <summary>
        /// Aplica la paginación obteniendo las siguientes reservas según la página
        /// </summary>
        private void ApplyPagination()
        {
            var filtered = GetFilteredBookings();
            var paginated = filtered
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            Bookings.Clear();
            foreach (var booking in paginated)
            {
                Bookings.Add(booking);
            }
            
            CommandManager.InvalidateRequerySuggested();

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(CurrentPage));
        }
        
        /// <summary>
        /// Filtra la reserva
        /// </summary>
        /// 
        /// <param name="obj">
        /// Reserva a filtrar
        /// </param>
        /// 
        /// <returns>
        /// Valor booleano:
        ///     - true -> La reserva pasa los filtros
        ///     - false -> La reserva no pasa los filtros
        /// </returns>
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
                _allBookings.Clear();
                foreach (var booking in list)
                {
                    UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                    booking.ClientDni = u.Dni;
                    booking.ClientName = u.FirstName + " " + u.LastName;
                    RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                    booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                    _allBookings.Add(booking);
                }

                CurrentPage = 1;
                ApplyFiltersAndPagination();
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
                    _allBookings.Remove(booking);
                    
                    ApplyFiltersAndPagination();
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
