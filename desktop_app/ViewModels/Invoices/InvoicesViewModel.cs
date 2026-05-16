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
    private List<BookingModel> _allInvoicedBookings = new List<BookingModel>();
    public ObservableCollection<BookingModel> InvoicedBookings { get; }
    
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set {if(SetProperty(ref _currentPage, value)) ApplyPagination();}
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set { if (SetProperty(ref _pageSize, value)) ApplyPagination(); }
    }
        
    private int _totalItems = 0;
        
    public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
        
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand GoToPageCommand { get; }
        
    public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 1, 5, 10, 15, 20, 50 };
    
    public ICommand DownloadInvoiceCommand { get; }
    public ICommand SendInvoiceCommand { get; }
    public ICommand ReloadDataCommand { get; }
    public ICommand NavigateToHotelFormCommand { get; }

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
    
    public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
    
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
    
    private void GoToFirstPage() => CurrentPage = 1;
    private void GoToPreviousPage() => CurrentPage--;
    private void GoToNextPage() => CurrentPage++;
    private void GoToLastPage() => CurrentPage = TotalPages;
        
    private void GoToPage(string? pageNumber)
    {
        if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }
        
    private bool CanGoToPage(string? pageNumber)
    {
        return int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages;
    }

    private List<BookingModel> GetFilteredInvoicedBookings()
    {
        if (_allInvoicedBookings == null || !_allInvoicedBookings.Any()) return new List<BookingModel>();

        return _allInvoicedBookings.Where(booking => FilterInvoicedBooking(booking)).ToList();
    }
    
    private void ApplyFiltersAndPagination()
    {
        var filtered = GetFilteredInvoicedBookings();
        _totalItems = filtered.Count;
        ApplyPagination();
    }

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

    private void NavigateToHotelForm(object? parameter)
    {
        NavigationService.Instance.NavigateTo<FormHotelView>();
    }
    
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
                if (booking.InvoiceId != "" || booking.TotalPaid == booking.TotalPrice)
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