using System.Collections.ObjectModel;
using System.Windows.Controls;
using desktop_app.Models;
using desktop_app.Views.BookingViews;

namespace desktop_app.ViewModels.Booking;

public class BasePaymentsViewModel : ViewModelBase
{
    /// <summary>
    /// Implementación del patrón singleton
    /// </summary>
    private static BasePaymentsViewModel? _instance;
    public static BasePaymentsViewModel Instance => _instance ??= new BasePaymentsViewModel();
    
    /// <summary>
    /// Propiedad para almacenar el ID de la reserva sobre la que se registraría el pago
    /// </summary>
    private string _bookingId = "";
    public string BookingId
    {
        get => _bookingId;
        set => _bookingId = value;
    }
    
    /// <summary>
    /// Propiedad de la lista de posibles formas de pago a registrar
    /// </summary>
    private ObservableCollection<String> _paymentMethods;
    public ObservableCollection<String> PaymentMethods
    {
        get => _paymentMethods;
        set => SetProperty(ref _paymentMethods, value);
    }
    
    /// <summary>
    /// Propiedad de la forma de pago seleccionada
    /// </summary>
    private string _selectedPaymentMethod;
    public string SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            SetProperty(ref _selectedPaymentMethod, value);
            NavigateToFormView(value);
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Propiedad de la vista activa (cada vista es un formulario para cada tipo de pago)
    /// </summary>
    private UserControl _currentView;
    public UserControl CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged(nameof(CurrentView));
        } 
    }

    /// <summary>
    /// Constructor
    /// Inicia una vista y su método de pago, la lista de métodos y le añade los mismos
    /// </summary>
    public BasePaymentsViewModel()
    {
        CurrentView = new CashPaymentView();
        SelectedPaymentMethod = "Efectivo";
        PaymentMethods = new ObservableCollection<string>();
        AddPaymentMethod();
        
    }

    /// <summary>
    /// Añade los métodos de pagos a su lista
    /// </summary>
    private void AddPaymentMethod()
    {
        PaymentMethods.Add("Efectivo");
        PaymentMethods.Add("Transferencia");
    }

    /// <summary>
    /// Navega a la vista (formulario) correspondiente a la forma de pago
    /// </summary>
    /// 
    /// <param name="value">
    /// Cadena de texto con la forma de pago seleccionada
    /// </param>
    public void NavigateToFormView(string value)
    {
        switch (value)
        {
            case "Efectivo":
            {
                CurrentView = new CashPaymentView();
                CashPaymentsViewModel.Instance.BookingId = BookingId;
                _ = CashPaymentsViewModel.Instance.LoadBooking();
                break;
            }
            case "Transferencia":
            {
                CurrentView = new TransferPaymentView();
                TransferPaymentViewModel.Instance.BookingId = BookingId;
                _ = TransferPaymentViewModel.Instance.LoadBooking();
                
                break;
            }
        }
    }
}