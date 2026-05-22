using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels.Booking;

public class CashPaymentsViewModel : ViewModelBase
{
    /// <summary>
    /// Implementacion del patrón singleton
    /// </summary>
    private static CashPaymentsViewModel? _instance;
    public static CashPaymentsViewModel Instance => _instance ??= new CashPaymentsViewModel();

    /// <summary>
    /// Propiedad para almacenar el ID de la reserva cuyo pago se quiere registrar
    /// </summary>
    private string _bookingId = "";
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
    /// Objeto de la reserva cuyo pago se quiere registrar
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
    /// Total de precio que queda a pagar
    /// </summary>
    public decimal TotalToPay => Booking?.TotalPrice - Booking?.TotalPaid ?? 0;
    
    /// <summary>
    /// Propiedad para el precio pagado
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
    /// Propiedad del cambio a entregar
    /// </summary>
    public decimal ChangeGiven => decimal.Max(AmountReceived - PricePaid, 0);

    /// <summary>
    /// Propiedad para la cantidad entregada por el cliente
    /// </summary>
    private decimal _amountReceived;
    public decimal AmountReceived
    {
        get => _amountReceived;
        set
        {
            _amountReceived = value;
            RefreshAll();
        }
    }
    
    /// <summary>
    /// Constructor
    ///
    /// Inicia los valores de precio pagado y cantidad recibida
    /// Carga el objeto de la reserva
    /// Inicia el comando de guardar pago
    /// </summary>
    public CashPaymentsViewModel()
    {
        PricePaid = 0;
        AmountReceived = 0;
        _ = LoadBooking();
        SaveCommand = new AsyncRelayCommand(SavePayment);
    }

    /// <summary>
    /// Comando para guardar el pago
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Función que guarda el pago
    /// Crea el pago, completa sus datos con los de las propiedades y la guarda en la api
    /// Vuelve a cargar la reserva para obtener sus campos actualizados después del pago
    /// </summary>
    private async Task SavePayment()
    {
        try
        {
            var payment = new CashPaymentModel();
            payment.BookingId = BookingId;
            payment.PricePaid = PricePaid;
            payment.AmountReceived = AmountReceived;

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
    /// Carga el objeto de la reserva desde la api
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
    /// Indica a la vista que debe refrescar todos sus datos
    /// </summary>
    private void RefreshAll()
    {
        OnPropertyChanged(nameof(Booking));
        OnPropertyChanged(nameof(BookingId));
        OnPropertyChanged(nameof(PricePaid));
        OnPropertyChanged(nameof(ChangeGiven));
        OnPropertyChanged(nameof(AmountReceived));
        OnPropertyChanged(nameof(TotalToPay));
    }
}