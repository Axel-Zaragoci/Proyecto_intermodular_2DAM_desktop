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
    /// <summary>
    /// Lista de todos los logs
    /// </summary>
    private List<BookingLogModel> _allLogs = new List<BookingLogModel>();
    
    /// <summary>
    /// Colección de logs que se muestran en la pantalla
    /// </summary>
    public ObservableCollection<BookingLogModel> AuditLog { get; set; }

    /// <summary>
    /// Lista de tipos de log y propiedad para filtrar el tipo de log
    /// </summary>
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

    /// <summary>
    /// Propiedad para filtrar por el nombre y apellidos del usuario que ha realizado la acción
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
    /// Propiedad para filtrar por Id del documento al que se le ha aplicado la acción
    /// </summary>
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

    /// <summary>
    /// Propiedad correspondiente a la página actual
    /// </summary>
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set
        { if (SetProperty(ref _currentPage, value)) ApplyPagination(); }
    }
    
    /// <summary>
    /// Propiedad correspondiente a la cantidad de registros por página
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
    /// Total de páginas necesarias para mostrar los registros
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
    /// <summary>
    /// Propiedades para saber si hay página siguiente y página previa
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
    /// Lista de posibles cantidades de registros mostrados por página
    /// </summary>
    public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 5, 10, 15, 20, 50 };
    
    /// <summary>
    /// Constructor
    /// Carga los logs, inicia la lista de logs que se mostrarán y los comandos
    /// </summary>
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
    
    /// <summary>
    /// Funciones para navegar entre las diferentes páginas de registros
    /// </summary>
    private void GoToFirstPage() => CurrentPage = 1;
    private void GoToPreviousPage() => CurrentPage--;
    private void GoToNextPage() => CurrentPage++;
    private void GoToLastPage() => CurrentPage = TotalPages;
        
    /// <summary>
    /// Función para ir a una página en concreto
    /// </summary>
    /// 
    /// <param name="pageNumber">
    /// Número de la página a la que se quiere ir en formato texto
    /// </param>
    private void GoToPage(string? pageNumber)
    {
        if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }
        
    /// <summary>
    /// Función para saber si se puede navegar a una página en concreto
    /// </summary>
    /// 
    /// <param name="pageNumber">
    /// Número de la página en string
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
    /// Obtiene una lista filtrada de logs de reserva
    /// </summary>
    /// 
    /// <returns>
    /// Lista de logs de reserva
    /// </returns>
    private List<BookingLogModel> GetFilteredLogs()
    {
        if (_allLogs == null || !_allLogs.Any()) return new List<BookingLogModel>();

        return _allLogs.Where(log => FilterLogs(log)).ToList();
    }
    
    /// <summary>
    /// Función para obtener los logs filtrados, cambiar el total de registros a mostrar y aplicar paginación
    /// </summary>
    private void ApplyFiltersAndPagination()
    {
        var filtered = GetFilteredLogs();
        _totalItems = filtered.Count;
        ApplyPagination();
    }
    
    /// <summary>
    /// Función que aplica la paginación
    /// FLUJO:
    /// - Obtiene lista de logs filtrados
    /// - Obtiene los logs para la página a mostrar
    /// - Vacía la lista actual que se muestra
    /// - Añade los logs a mostrar en la lista
    /// - Avisa a la interfaz para que actualice las propiedades
    /// </summary>
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
    
    /// <summary>
    /// Función que filtra el log para saber si se deberá mostrar o no
    /// </summary>
    /// 
    /// <param name="obj">
    /// Log que se ha de filtrar
    /// </param>
    /// 
    /// <returns>
    /// Valor booleano:
    ///     - true -> Se ha de mostrar el log
    ///     - false -> No se ha de mostrar el log
    /// </returns>
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
        
        bool nameMatch = Match(log.UserName, FilterName);
        bool idMatch = Match(log.DocumentId, FilterId);
        
        return actionMatch && nameMatch && idMatch;
    }

    /// <summary>
    /// Función que carga los logs
    /// FLUJO:
    /// - Obtiene todos los logs de la API
    /// - Obtiene los diferens IDs de usuarios y habitaciones
    /// - Obtiene todas las habitaciones y usuarios que necesita
    /// - Carga en cada log los datos que necesita
    /// - Carga los logs en la lista de todos los logs
    /// - Aplica filtros y paginación
    /// </summary>
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