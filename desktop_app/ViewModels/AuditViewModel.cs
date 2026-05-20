using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class AuditViewModel : ViewModelBase
{
    private List<BookingLogModel> _allLogs = new List<BookingLogModel>();
    
    public ObservableCollection<BookingLogModel> AuditLog { get; set; }

    public ObservableCollection<string> Types { get; } = new ObservableCollection<string> { "Todos", "Crear", "Actualizar", "Eliminar" };
    
    private string _selectedType = "Todos";
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination(); 
            }
        }
    }

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

    private string _filterId = "";
    public string FilterId
    {
        get => _filterId;
        set {
            if (SetProperty(ref _filterId, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            } 
        }
    }

    public ObservableCollection<string> Collections { get; } = new ObservableCollection<string> { "Reserva", "Habitación", "Usuarios" };
    private string _selectedCollection = "Reserva";
    public string SelectedCollection
    {
        get => _selectedCollection;
        set { 
            if (SetProperty(ref _selectedCollection, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            } 
        }
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set
        { if (SetProperty(ref _currentPage, value)) ApplyPagination(); }
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
    
    public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 5, 10, 15, 20, 50 };
    
    public AuditViewModel()
    {
        _ = LoadAuditLogs();
        
        AuditLog = new ObservableCollection<BookingLogModel>();
        
        FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => HasPreviousPage);
        PreviousPageCommand  = new RelayCommand(_ => GoToPreviousPage(), _ => HasPreviousPage);
        NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => HasNextPage);
        LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => HasNextPage);
        GoToPageCommand = new RelayCommand<string>(GoToPage, CanGoToPage);
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

    private List<BookingLogModel> GetFilteredLogs()
    {
        if (_allLogs == null || !_allLogs.Any()) return new List<BookingLogModel>();

        return _allLogs.Where(log => FilterLogs(log)).ToList();
    }
    
    private void ApplyFiltersAndPagination()
    {
        var filtered = GetFilteredLogs();
        _totalItems = filtered.Count;
        ApplyPagination();
    }
    
    private void ApplyPagination()
    {
        var filtered = GetFilteredLogs();
        var paginated = filtered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
            
        AuditLog.Clear();
        foreach (var booking in paginated)
        {
            AuditLog.Add(booking);
        }
        
        CommandManager.InvalidateRequerySuggested();

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(CurrentPage));
    }
    

    private bool FilterLogs(object obj)
    {
        if (obj is not BookingLogModel log) return false;

        bool Match(string? value, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;
            return (value ?? "").IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool actionMatch = SelectedType switch
        {
            "Todos" => true,
            "Crear" => log.Action?.Equals("create", StringComparison.OrdinalIgnoreCase) == true,
            "Actualizar" => log.Action?.Equals("update", StringComparison.OrdinalIgnoreCase) == true,
            "Eliminar" => log.Action?.Equals("delete", StringComparison.OrdinalIgnoreCase) == true
        };
        
        bool collectionMatch = SelectedCollection switch
        {
            "Todos" => true,
            "Reserva" => log.Collections?.Equals("bookings", StringComparison.OrdinalIgnoreCase) == true,
            "Habitación" => log.Collections?.Equals("rooms", StringComparison.OrdinalIgnoreCase) == true,
            "Usuarios" => log.Collections?.Equals("users", StringComparison.OrdinalIgnoreCase) == true
        };
        
        bool nameMatch = Match(log.UserName, FilterName);
        bool idMatch = Match(log.DocumentId, FilterId);
        
        return actionMatch && collectionMatch && nameMatch && idMatch;
    }

    private async Task LoadAuditLogs()
    {
        _allLogs.Clear();
     
        List<BookingLogModel> logs = await BookingLogService.GetLogs();
        List<string> roomIds = new List<string>();
    
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
        
            string roomId = "";
            if (log.OldBooking != null && !string.IsNullOrEmpty(log.OldBooking?.Room))
            {
                roomId = log.OldBooking.Room;
            }
            else if (log.NewBooking != null && !string.IsNullOrEmpty(log.NewBooking?.Room))
            {
                roomId = log.NewBooking.Room;
            }
        
            if (!string.IsNullOrEmpty(roomId) && !roomIds.Contains(roomId))
            {
                roomIds.Add(roomId);
            }
        }
        List<String> userIds = logs.Select(b => b.UserId).Distinct().ToList();
        List<string> clientIds = new List<string>();
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            string clientId = "";
            if (log.OldBooking != null && !string.IsNullOrEmpty(log.OldBooking?.Client))
            {
                clientId = log.OldBooking.Client;
            }
            else if (log.NewBooking != null && !string.IsNullOrEmpty(log.NewBooking?.Client))
            {
                clientId = log.NewBooking.Client;
            }
            
            if (!string.IsNullOrEmpty(clientId) && !clientIds.Contains(clientId))
            {
                clientIds.Add(clientId);
            }
        }
        List<String> allUserIds = userIds.Concat(clientIds).Distinct().ToList();
        
        List<UserModel> users = new List<UserModel>();
        List<RoomModel> rooms = new List<RoomModel>();
        
        foreach (string userId in allUserIds)
        {
            users.Add(await UserService.GetClientByIdAsync(userId));
        }
        foreach (string roomId in roomIds)
        {
            var room = await RoomService.GetRoomByIdAsync(roomId);
            if (room != null) rooms.Add(room);
        }
        foreach (var log in logs)
        {
            var user = users.Find(u => u.Id == log.UserId);
            if (user == null)
            {
                log.UserName = "Usuario no encontrado";
                log.UserDni = "Usuario no encontrado";
                log.UserNameDni = "Usuario no encontrado";
            }
            else
            {
                log.UserName = $"{user?.FirstName} {user?.LastName}";
                log.UserDni = $"{user?.Dni}";
                log.UserNameDni = $"{user?.FirstName} {user?.LastName} - {user?.Dni}";
            }
            
            var roomId = "";
            if (log.OldBooking != null && !string.IsNullOrEmpty(log.OldBooking?.Room))
            {
                roomId = log.OldBooking.Room;
            }
            else if (log.NewBooking != null && !string.IsNullOrEmpty(log.NewBooking?.Room))
            {
                roomId = log.NewBooking.Room;
            }
            var room = rooms.Find(r => r.Id == roomId);
            log.Room = room?.RoomNumber;

            if ((log.OldBooking != null && !string.IsNullOrEmpty(log.OldBooking?.Room)) ||
                (log.NewBooking != null && !string.IsNullOrEmpty(log.NewBooking?.Room)))
            {
                log.Differences = BookingModelDifference.GetDifferences(log.OldBooking ?? new BookingModel(), log.NewBooking ?? new BookingModel());
            }
            log.DifferenceString = String.Join("\n", log.Differences);
            _allLogs.Add(log);
        }

        CurrentPage = 1;
        ApplyFiltersAndPagination();
    }
}