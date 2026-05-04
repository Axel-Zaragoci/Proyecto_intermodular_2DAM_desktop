using System.Collections.ObjectModel;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

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
        
            string roomId = log.OldBooking?.Room == "" ? log?.NewBooking.Room : log.OldBooking?.Room;
        
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
            string clientId = log.OldBooking?.Client == "" ? log.NewBooking?.Client : log.OldBooking?.Client;
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
            log.User = $"{user?.FirstName} {user?.LastName} - {user?.Dni}";
            var roomId = log.OldBooking?.Room == "" ? log.NewBooking?.Room : log.OldBooking?.Room;
            var room = rooms.Find(r => r.Id == roomId);
            log.Room = room?.RoomNumber;
            if (log.OldBooking != null && log.NewBooking != null) log.Differences = BookingModelDifference.GetDifferences(log.OldBooking, log.NewBooking);
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