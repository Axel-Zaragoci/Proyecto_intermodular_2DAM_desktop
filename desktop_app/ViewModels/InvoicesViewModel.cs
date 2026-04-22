using System.Collections.ObjectModel;
using System.Windows;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class InvoicesViewModel : ViewModelBase
{
    public ObservableCollection<BookingModel> InvoicedBookings { get; }

    public ObservableCollection<BookingModel> OtherBookings { get; }
    
    public InvoicesViewModel()
    {
        InvoicedBookings = new ObservableCollection<BookingModel>();
        OtherBookings = new ObservableCollection<BookingModel>();
        _ = LoadBookingsWithInvoiceAsync();
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
            InvoicedBookings.Clear();
            OtherBookings.Clear();
            foreach (var booking in list)
            {
                UserModel u = await UserService.GetClientByIdAsync(booking.Client);
                booking.ClientDni = u.Dni;
                booking.ClientName = u.FirstName + " " + u.LastName;
                RoomModel? room = await RoomService.GetRoomByIdAsync(booking.Room);
                booking.RoomNumber = room != null ? room.RoomNumber : "Error";
                if (booking.InvoiceId != "")
                {
                    InvoicedBookings.Add(booking);
                }
                else
                {
                    OtherBookings.Add(booking);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}