using System.Collections.ObjectModel;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels.Booking;

public class BookingLogHistoryViewModel : ViewModelBase
{
    private static BookingLogHistoryViewModel? _instance;
    public static BookingLogHistoryViewModel Instance => _instance ??= new BookingLogHistoryViewModel();
    
    private string _bookingId = "";
    public string BookingId
    {
        get => _bookingId;
        set
        {
            _bookingId = value;
            LoadLogs();
            LoadPayments();
        } 
    }
    
    public ObservableCollection<BookingLogModel> Logs { get; }
    public ObservableCollection<BasePaymentModel> Payments { get; }

    public BookingLogHistoryViewModel()
    {
        Logs = new ObservableCollection<BookingLogModel>();
        Payments = new ObservableCollection<BasePaymentModel>();
    }

    public async Task LoadLogs()
    {
        Logs.Clear();
        List<BookingLogModel> logs = await BookingLogService.GetBookingLogs(_bookingId);
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
            log.UserName = $"{user?.FirstName} {user?.LastName}";
            log.UserDni = $"{user?.Dni}";
            log.UserNameDni = $"{user?.FirstName} {user?.LastName} - {user?.Dni}";
            
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
            Logs.Add(log);
        }
    }
    
    public async Task LoadPayments()
    {
        Payments.Clear();
        List<BasePaymentModel> payments = await PaymentService.GetBookingPaymentHistory(BookingId);
        foreach (BasePaymentModel payment in payments)
        {
            Payments.Add(payment);
        }
    }
}