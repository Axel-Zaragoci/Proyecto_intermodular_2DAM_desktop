using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class AuditViewModel : ViewModelBase
{
    public ObservableCollection<BookingLogModel> AuditLog { get; set; }
    
    public ICollectionView AuditLogView { get; set; }

    public ObservableCollection<string> Types { get; } = new ObservableCollection<string> { "Todos", "Crear", "Actualizar", "Eliminar" };
    
    private string _selectedType = "Todos";
    public string SelectedType
    {
        get => _selectedType;
        set {if (SetProperty(ref _selectedType, value)) AuditLogView.Refresh();}
    }

    private string _filterName = "";
    public string FilterName
    {
        get => _filterName;
        set {if (SetProperty(ref _filterName, value)) AuditLogView.Refresh();}
    }

    private string _filterId = "";
    public string FilterId
    {
        get => _filterId;
        set {if (SetProperty(ref _filterId, value)) AuditLogView.Refresh();}
    }

    public ObservableCollection<string> Collections { get; } = new ObservableCollection<string> { "Reserva", "Habitación", "Usuarios" };
    private string _selectedCollection = "Reserva";
    public string SelectedCollection
    {
        get => _selectedCollection;
        set { if (SetProperty(ref _selectedCollection, value)) AuditLogView.Refresh();}
    }
    
    public AuditViewModel()
    {
        AuditLog = new ObservableCollection<BookingLogModel>();
        AuditLogView = CollectionViewSource.GetDefaultView(AuditLog);
        
        AuditLogView.Filter = FilterLogs;
        _ = LoadAuditLogs();
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
        AuditLog.Clear();
     
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
            AuditLog.Add(log);
        }
        AuditLogView.Refresh();
    }
}