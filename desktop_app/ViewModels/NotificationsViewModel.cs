using System.Collections.ObjectModel;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class NotificationsViewModel : ViewModelBase
{
    public ObservableCollection<BookingModel> Bookings24H { get; set; }
    public ObservableCollection<BookingModel> Bookings48H { get; set; }
    
    public NotificationsViewModel()
    {
        Bookings24H = new ObservableCollection<BookingModel>();
        Bookings48H = new ObservableCollection<BookingModel>();
        _ = LoadBookingsAsync();
        ReloadBookingsCommand = new AsyncRelayCommand(LoadBookingsAsync);
    }

    public ICommand ReloadBookingsCommand { get; set; }
    
    private async Task LoadBookingsAsync()
    {
        var list = await BookingService.GetAllBookingsAsync();
        var h24 = DateTime.Now.AddDays(1);
        var h48 = DateTime.Now.AddDays(2);
        foreach (var booking in list)
        {
            UserModel u = await UserService.GetClientByIdAsync(booking.Client);
            booking.ClientDni = u.Dni;
            booking.ClientName = u.FirstName + " " + u.LastName;
            RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
            booking.RoomNumber = room != null ? room.RoomNumber : "Error";
            
            if (IsBetween(booking.CheckInDate, DateTime.Now, h24))
            {
                Bookings24H.Add(booking);
            }
            else if (IsBetween(booking.CheckInDate, h24, h48))
            {
                Bookings48H.Add(booking);
            }
        }

        Bookings24H = new ObservableCollection<BookingModel>(Bookings24H.OrderBy(booking => booking.CheckInDate).ToList());
        Bookings48H = new ObservableCollection<BookingModel>(Bookings48H.OrderBy(booking => booking.CheckInDate).ToList());
    }

    private static bool IsBetween(DateTime date, DateTime start, DateTime end)
    {
        return date >= start && date <= end;
    }
}