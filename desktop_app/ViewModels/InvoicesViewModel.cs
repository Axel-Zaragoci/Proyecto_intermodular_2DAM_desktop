using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels;

public class InvoicesViewModel : ViewModelBase
{
    public ObservableCollection<BookingModel> InvoicedBookings { get; }

    public ObservableCollection<BookingModel> OtherBookings { get; }
    
    public ICommand DownloadInvoiceCommand { get; }
    public ICommand AddInvoiceCommand { get; }
    public ICommand SendInvoiceCommand { get; }
    public ICommand ReloadDataCommand { get; }
    public ICommand NavigateToHotelFormCommand { get; }
    
    public InvoicesViewModel()
    {
        InvoicedBookings = new ObservableCollection<BookingModel>();
        OtherBookings = new ObservableCollection<BookingModel>();
        _ = LoadBookingsWithInvoiceAsync();
        DownloadInvoiceCommand = new AsyncRelayCommand<BookingModel>(DownloadInvoiceAsync);
        ReloadDataCommand = new AsyncRelayCommand(LoadBookingsWithInvoiceAsync);
        SendInvoiceCommand = new AsyncRelayCommand<BookingModel>(SendInvoiceAsync);
        AddInvoiceCommand = new AsyncRelayCommand<BookingModel>(AddInvoiceAsync);
        NavigateToHotelFormCommand = new RelayCommand(NavigateToHotelForm);
    }

    private void NavigateToHotelForm(object? parameter)
    {
        NavigationService.Instance.NavigateTo<FormHotelView>();
    }
    
    private async Task AddInvoiceAsync(BookingModel booking)
    {
        await DownloadInvoiceAsync(booking);
        await LoadBookingsWithInvoiceAsync();
    }
    
    private async Task DownloadInvoiceAsync(BookingModel booking)
    {
        try
        {
            InvoiceService service = new();

            byte[] pdfBytes = await service.DownloadPdfAsync(booking);

            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                string tempFile = Path.Combine(Path.GetTempPath(), booking.InvoiceId);
            
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
        InvoiceService service = new();
        await service.SendPdfAsync(booking);
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