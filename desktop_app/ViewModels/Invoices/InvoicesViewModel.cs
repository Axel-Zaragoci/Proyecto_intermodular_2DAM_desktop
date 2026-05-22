using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels.Invoices;

public class InvoicesViewModel : ViewModelBase
{
    /// <summary>
    /// Propiedad para la lista con todos las reservas que se pueden crear las facturas
    /// </summary>
    private List<BookingModel> _allInvoicedBookings = new List<BookingModel>();
    public ObservableCollection<BookingModel> InvoicedBookings { get; }
    
    /// <summary>
    /// Propiedad para guardar la página actual en la lista
    /// </summary>
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set {if(SetProperty(ref _currentPage, value)) ApplyPagination();}
    }

    /// <summary>
    /// Propiedad para almacenar la cantidad de registros por página
    /// </summary>
    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set { if (SetProperty(ref _pageSize, value)) ApplyPagination(); }
    }
    
    /// <summary>
    /// Propiedad que almacena el total de registros
    /// </summary>
    private int _totalItems = 0;
    public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
    /// <summary>
    /// Propiedades para saber si puede navegar a la siguiente o anterior página
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
        
    /// <summary>
    /// Comandos para la navegación
    /// </summary>
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand GoToPageCommand { get; }
        
    /// <summary>
    /// Lista con las posibles cantidades de registros por página
    /// </summary>
    public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 1, 5, 10, 15, 20, 50 };
    
    /// <summary>
    /// Comandos para descargar y enviar factura, recargar datos y navegar al formulario de datos del hotel
    /// </summary>
    public ICommand DownloadInvoiceCommand { get; }
    public ICommand SendInvoiceCommand { get; }
    public ICommand ReloadDataCommand { get; }
    public ICommand NavigateToHotelFormCommand { get; }

    /// <summary>
    /// Filtro para el nombre del cliente
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
    /// Filtro para el número de factura
    /// </summary>
    private string _filterNumber = "";
    public string FilterNumber
    {
        get => _filterNumber;
        set
        {
            if (SetProperty(ref _filterNumber, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            }
        }
    }

    /// <summary>
    /// Filtro para el número de habitación
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
    /// Filtro para la fecha de inicio de la reserva asociada a la factura
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
    /// Filtro de tipo de acción según la fecha de inicio indicada en su filtro
    /// </summary>
    public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
    private string _selectedStartDateFilter = "Antes de";
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
    /// Constructor
    /// Se encarga de cargar las reservas con factura, iniciar la lista de reservas y cargar los comandos
    /// </summary>
    public InvoicesViewModel()
    {
        _ = LoadBookingsWithInvoiceAsync();
        
        InvoicedBookings = new ObservableCollection<BookingModel>();
        
        FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => HasPreviousPage);
        PreviousPageCommand  = new RelayCommand(_ => GoToPreviousPage(), _ => HasPreviousPage);
        NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => HasNextPage);
        LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => HasNextPage);
        GoToPageCommand = new RelayCommand<string>(GoToPage, CanGoToPage);
        
        DownloadInvoiceCommand = new AsyncRelayCommand<BookingModel>(DownloadInvoiceAsync);
        ReloadDataCommand = new AsyncRelayCommand(LoadBookingsWithInvoiceAsync);
        SendInvoiceCommand = new AsyncRelayCommand<BookingModel>(SendInvoiceAsync);
        NavigateToHotelFormCommand = new RelayCommand(NavigateToHotelForm);
    }
    
    /// <summary>
    /// Funciones para la navegación a la primera, última, siguiente y anterior página
    /// </summary>
    private void GoToFirstPage() => CurrentPage = 1;
    private void GoToPreviousPage() => CurrentPage--;
    private void GoToNextPage() => CurrentPage++;
    private void GoToLastPage() => CurrentPage = TotalPages;
        
    /// <summary>
    /// Función para navegar a una página en concreto dado su número
    /// </summary>
    /// 
    /// <param name="pageNumber">
    /// Número de página en cadena de texto
    /// </param>
    private void GoToPage(string? pageNumber)
    {
        if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }
        
    /// <summary>
    /// Función para saber si puede navegar a una página en concreto dado su número
    /// </summary>
    ///
    /// <param name="pageNumber">
    /// Número de la página en cadena de texto
    /// </param>
    /// 
    /// <returns>
    /// Valor booleano:
    ///    - true -> Puede navegar
    ///     - false -> No puede navegar
    /// </returns>
    private bool CanGoToPage(string? pageNumber)
    {
        return int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages;
    }

    /// <summary>
    /// Función que devuelve la lista de reservas facturables filtrada
    /// </summary>
    /// 
    /// <returns>
    /// Lista de reservas filtrada 
    /// </returns>
    private List<BookingModel> GetFilteredInvoicedBookings()
    {
        if (_allInvoicedBookings == null || !_allInvoicedBookings.Any()) return new List<BookingModel>();

        return _allInvoicedBookings.Where(booking => FilterInvoicedBooking(booking)).ToList();
    }
    
    /// <summary>
    /// Función que obtiene la lista de reservas filtradas, actualiza la cantidad de registros y aplica paginación
    /// </summary>
    private void ApplyFiltersAndPagination()
    {
        var filtered = GetFilteredInvoicedBookings();
        _totalItems = filtered.Count;
        ApplyPagination();
    }

    /// <summary>
    /// Función que aplica la paginación
    /// FLUJO:
    /// - Obtiene la lista de reservas filtrada
    /// - Obtiene solo las necesarias según la página y cantidad de registros por página
    /// - Vacía la lista que se muestra de reservas
    /// - Carga las reservas en la lista que se muestra
    /// - Avisa a la interfaz que debe recargar los datos
    /// </summary>
    private void ApplyPagination()
    {
        var filtered = GetFilteredInvoicedBookings();
        var paginated = filtered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        InvoicedBookings.Clear();
        foreach (var booking in paginated)
        {
            InvoicedBookings.Add(booking);
        }
        
        CommandManager.InvalidateRequerySuggested();

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(CurrentPage));
    }
    
    /// <summary>
    /// Función que indica si una reserva pasa los filtros o no
    /// </summary>
    /// 
    /// <param name="obj">
    /// Objeto correspondiente a la reserva
    /// </param>
    /// 
    /// <returns>
    /// Valor booleano:
    ///     - true -> La reserva pasa los filtros
    ///     - false -> La reserva no pasa los filtros
    /// </returns>
    private bool FilterInvoicedBooking(object obj)
    {
        if (obj is not BookingModel booking) return false;

        bool Match(string? value, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;
            return (value ?? "").IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool nameMatch = Match(booking.ClientName, FilterName);
        bool numberMatch = Match(booking.InvoiceId, FilterNumber);
        bool roomMatch = Match(booking.RoomNumber, FilterRoom);

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

        bool result = numberMatch && roomMatch && startDateMatch && nameMatch;
        return result;
    }

    /// <summary>
    /// Función para navegar al formulario de datos del hotel
    /// </summary>
    /// <param name="parameter">
    /// Parametro requerido para el comando que utiliza esta función
    /// </param>
    private void NavigateToHotelForm(object? parameter)
    {
        NavigationService.Instance.NavigateTo<FormHotelView>();
    }
    
    /// <summary>
    /// Comando que descarga la factura
    /// FLUJO:
    /// - Obtiene los bytes de la factura del servicio
    /// - Obtiene una ruta en la carpeta de ficheros temporales
    /// - Carga la factura en la ruta anterior
    /// - Abre el programa por defecto del equipo para mostrar la factura
    /// </summary>
    /// 
    /// <param name="booking">
    /// Reserva cuya factura se quiere descargar
    /// </param>
    private async Task DownloadInvoiceAsync(BookingModel booking)
    {
        try
        {
            byte[] pdfBytes = await InvoiceService.DownloadPdfAsync(booking);

            if (pdfBytes.Length > 0)
            {
                string tempFile = await GetTempFileRoute(booking);
            
                File.WriteAllBytes(tempFile, pdfBytes);
            
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al mostrar factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Función que llama a la API para enviar la factura por mail al cliente
    /// </summary>
    /// 
    /// <param name="booking">
    /// Objeto de la reserva cuya factura se quiere enviar
    /// </param>
    private async Task SendInvoiceAsync(BookingModel booking)
    {
        await InvoiceService.SendPdfAsync(booking);
    }
    
    /// <summary>
    /// Método que carga las reservas
    /// Obtiene todas las reservas y vacía la lista para evitar duplicados
    /// Obtiene los datos necesarios para mostrar la información en la UI
    /// Añade las reservas con los datos extra a la lista de reservas
    /// </summary>
    private async Task LoadBookingsWithInvoiceAsync()
    {
        try
        {
            var list = await BookingService.GetAllBookingsAsync();
            _allInvoicedBookings.Clear();
            foreach (var booking in list)
            {
                if (booking.CheckOutDate.Date < DateTime.Now.Date && booking.TotalPaid == booking.TotalPrice)
                {
                    UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                    booking.ClientDni = u.Dni;
                    booking.ClientName = u.FirstName + " " + u.LastName;
                    RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                    booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                    
                    _allInvoicedBookings.Add(booking);
                }
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
    /// Obtiene la ruta para la factura
    /// Junta el directorio de archivos temporales y el nobmre de fichero de la factura
    /// </summary>
    /// 
    /// <param name="booking">
    /// Objeto de la reserva cuya factura se quiere guardar
    /// </param>
    /// 
    /// <returns>
    /// Cadena de texto de la ruta en la que guardar el fichero
    /// </returns>
    private async Task<String> GetTempFileRoute(BookingModel booking)
    {
        if (booking.InvoiceId != "")
        {
            return Path.Combine(Path.GetTempPath(), booking.InvoiceId);
        }
        var bookingWithInvoice = await BookingService.GetBookingAsync(booking.Id);
        return Path.Combine(Path.GetTempPath(), bookingWithInvoice.InvoiceId);
    }
}