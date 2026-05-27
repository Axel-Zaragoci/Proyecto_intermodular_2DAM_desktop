using System.Collections.ObjectModel;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class NotificationsViewModel : ViewModelBase
{
    /// <summary>
    /// Listas para las notificaciones de 24 y 48 horas
    /// </summary>
    public ObservableCollection<BookingModel> Bookings24H { get; set; }
    public ObservableCollection<BookingModel> Bookings48H { get; set; }
    
    /// <summary>
    /// Constructor
    /// Se encarga de iniciar las listas, cargar las reservas a notificar e iniciar el comando de recarga
    /// </summary>
    public NotificationsViewModel()
    {
        Bookings24H = new ObservableCollection<BookingModel>();
        Bookings48H = new ObservableCollection<BookingModel>();
        _ = LoadBookingsAsync();
        ReloadBookingsCommand = new AsyncRelayCommand(LoadBookingsAsync);
    }

    /// <summary>
    /// Comando para recargar los datos desde la API
    /// </summary>
    public ICommand ReloadBookingsCommand { get; set; }
    
    /// <summary>
    /// Función para cargar las reservas y las notificaciones
    /// FLUJO:
    /// - Obtiene todas las reservas
    /// - Calcula las fechas de los próximos 2 días
    /// - Itera sobre las reservas
    ///     - Si faltan 24 horas o menos, obtiene el usuario y la habitacion, rellena los datos y lo añade a su correspondiente lista
    ///     - Si faltan entre 24 y 48 horas, obtiene el usuario y la habitación, rellena los datos y lo añade a su correspondiente lista
    /// - Ordena las listas por hora de inicio del check-in
    /// </summary>
    private async Task LoadBookingsAsync()
    {
        var list = await BookingService.GetAllBookingsAsync();
        var h24 = DateTime.Now.AddDays(1);
        var h48 = DateTime.Now.AddDays(2);
        foreach (var booking in list)
        {
            if (IsBetween(booking.CheckInDate, DateTime.Now, h24))
            {
                
                UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                booking.ClientDni = u.Dni;
                booking.ClientName = u.FirstName + " " + u.LastName;
                RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                Bookings24H.Add(booking);
            }
            else if (IsBetween(booking.CheckInDate, h24, h48))
            {
                
                UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                booking.ClientDni = u.Dni;
                booking.ClientName = u.FirstName + " " + u.LastName;
                RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                Bookings48H.Add(booking);
            }
        }

        Bookings24H = new ObservableCollection<BookingModel>(Bookings24H.OrderBy(booking => booking.CheckInDate).ToList());
        Bookings48H = new ObservableCollection<BookingModel>(Bookings48H.OrderBy(booking => booking.CheckInDate).ToList());
    }

    /// <summary>
    /// Función que calcula si una fecha se ubica entre 2 otras fechas
    /// </summary>
    /// <param name="date">
    /// Fecha a comprobar
    /// </param>
    /// <param name="start">
    /// Fecha de inicio del intervalo
    /// </param>
    /// <param name="end">
    /// Fecha de fin del intervalo
    /// </param>
    /// 
    /// <returns>
    /// Valor booleano:
    ///     - true -> La fecha está en el intervalo
    ///     - false -> La fecha no está en el intervalo
    /// </returns>
    private static bool IsBetween(DateTime date, DateTime start, DateTime end)
    {
        return date >= start && date <= end;
    }
}