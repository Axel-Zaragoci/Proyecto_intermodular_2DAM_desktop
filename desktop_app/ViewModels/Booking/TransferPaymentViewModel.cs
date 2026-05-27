using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels.Booking;

public class TransferPaymentViewModel : ViewModelBase
{
    /// <summary>
    /// Implementación del patrón Singleton
    /// </summary>
    private static TransferPaymentViewModel? _instance;
    public static TransferPaymentViewModel Instance => _instance ??= new TransferPaymentViewModel();

    /// <summary>
    /// Propiedad para almacenar el ID de la reserva cuyo pago se quiere registrar
    /// </summary>
    private string _bookingId;
    public string BookingId
    {
        get => _bookingId;
        set
        {
            _bookingId = value;
            _ = LoadBooking();
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad para almacenar el objeto de la reserva cuyo pago se quiere registrar
    /// </summary>
    private BookingModel _booking;
    public BookingModel Booking
    {
        get => _booking;
        set
        {
            _booking = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena el total a pagar en la reserva
    /// </summary>
    public decimal TotalToPay => Booking?.TotalPrice - Booking?.TotalPaid ?? 0;

    /// <summary>
    /// Propiedad que almacena la cantidad que se va a pagar
    /// </summary>
    private decimal _pricePaid;
    public decimal PricePaid
    {
        get => _pricePaid;
        set
        {
            _pricePaid = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena la referencia de la transferencia
    /// </summary>
    private string _reference;
    public string Reference
    {
        get => _reference;
        set
        {
            _reference = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena el nombre del propietario de la cuenta
    /// </summary>
    private string _holder;
    public string Holder
    {
        get => _holder;
        set
        {
            _holder = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena el número de la cuenta
    /// </summary>
    private string _originAccount;
    public string OriginAccount
    {
        get => _originAccount;
        set
        {
            _originAccount = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena la fecha en la que se ha realizado la transferencia
    /// </summary>
    private DateTime _transferDate;
    public DateTime TransferDate
    {
        get => _transferDate;
        set
        {
            _transferDate = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Constructor
    /// Se encarga de iniciar las propiedades, cargar la reserva e iniciar el comando para registrar el pago
    /// </summary>
    public TransferPaymentViewModel()
    {
        PricePaid = 0;
        Reference = "";
        Holder = "";
        OriginAccount = "";
        TransferDate = DateTime.Now;
        _ = LoadBooking();
        SaveCommand = new AsyncRelayCommand(SavePayment);
    }
    
    /// <summary>
    /// Comando para registrar un pago
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Función que registra el pago
    /// FLUJO:
    /// - Crea un nuevo pago y le asigna los valores de las propiedades
    /// - Crea el pago en la API
    /// - Carga la reserva con el pago actualizado y refresca las propiedades
    /// </summary>
    private async Task SavePayment()
    {
        try
        {
            var payment = new TransferPaymentModel();
            payment.BookingId = BookingId;
            payment.PricePaid = PricePaid;
            payment.Holder = Holder;
            payment.Reference = Reference;
            payment.TransferDate = TransferDate;
            payment.OriginAccount = OriginAccount;
            
            var createdPayment = await PaymentService.CreatePayment(payment, BookingId);
            await LoadBooking();
            RefreshAll();
            
            MessageBox.Show("Pago guardado", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Función de carga de la reserva
    /// Solicita a la API la reserva filtrando por el ID y refresca las propiedades
    /// </summary>
    public async Task LoadBooking()
    {
        if (BookingId == "")
        {
            Booking = new BookingModel();
            return;
        }
        Booking = await BookingService.GetBookingAsync(BookingId);
        RefreshAll();
    }

    /// <summary>
    /// Función que refresca todas las propiedades en la vista
    /// </summary>
    private void RefreshAll()
    {
        OnPropertyChanged(nameof(BookingId));
        OnPropertyChanged(nameof(Booking));
        OnPropertyChanged(nameof(TotalToPay));
        OnPropertyChanged(nameof(PricePaid));
        OnPropertyChanged(nameof(Reference));
        OnPropertyChanged(nameof(Holder));
        OnPropertyChanged(nameof(OriginAccount));
        OnPropertyChanged(nameof(TransferDate));
    }
}