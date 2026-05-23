using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Services;

namespace desktop_app.ViewModels.Booking;

public class RefundViewModel : ViewModelBase
{
    private static RefundViewModel? _instance;
    /// <summary>
    /// Obtiene la instancia única del ViewModel (patrón Singleton).
    /// </summary>
    public static RefundViewModel Instance => _instance ??= new RefundViewModel();
        
    private String _bookingId;
    /// <summary>
    /// Obtiene o establece el ID de reserva actual.
    /// </summary>
    public String BookingId
    {
        get => _bookingId;
        set
        {
            _bookingId = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Obtiene o establece la razón para solicitar el reembolso
    /// </summary>
    private String _reason;
    public String Reason
    {
        get => _reason;
        set
        {
            _reason = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Constructor
    /// Se encarga de iniciar el comando de reembolso
    /// </summary>
    public RefundViewModel()
    {
        RefundCommand = new AsyncRelayCommand(Refund);
    }
    
    /// <summary>
    /// Comando para la acción de reembolso
    /// </summary>
    public ICommand RefundCommand { get; }

    /// <summary>
    /// Función para procesar el reembolso
    /// </summary>
    /// 
    /// <exception cref="Exception">
    /// En caso de no reembolsarse correctamente todos los pagos, lanza una excepción y se muestra un error
    /// </exception>
    private async Task Refund()
    {
        try
        {
            bool refunds = await PaymentService.RefundBookingPayments(Reason, BookingId);
            
            if (!refunds) throw new Exception("No se ha podido reembolsar todos los pagos");
        
            MessageBox.Show("Reembolso completado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Refund - Error: {e.Message}");
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}