using System.Collections.ObjectModel;
using System.Windows.Controls;
using desktop_app.Models;
using desktop_app.Views.BookingViews;

namespace desktop_app.ViewModels;

public class BasePaymentsViewModel : ViewModelBase
{
    private static BasePaymentsViewModel? _instance;
    public static BasePaymentsViewModel Instance => _instance ??= new BasePaymentsViewModel();
    
    private string _bookingId = "";
    public string BookingId
    {
        get => _bookingId;
        set => _bookingId = value;
    }
    
    private ObservableCollection<String> _paymentMethods;
    public ObservableCollection<String> PaymentMethods
    {
        get => _paymentMethods;
        set => SetProperty(ref _paymentMethods, value);
    }
    
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

    public BasePaymentsViewModel()
    {
        CurrentView = new CashPaymentView();
        SelectedPaymentMethod = "Efectivo";
        PaymentMethods = new ObservableCollection<string>();
        AddPaymentMethod();
        
    }

    private void AddPaymentMethod()
    {
        PaymentMethods.Add("Efectivo");
        PaymentMethods.Add("Transferencia");
    }

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