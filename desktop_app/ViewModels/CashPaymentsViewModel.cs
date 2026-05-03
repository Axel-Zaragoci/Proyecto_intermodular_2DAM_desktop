using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels;

public class CashPaymentsViewModel : ViewModelBase
{
    private static CashPaymentsViewModel? _instance;
    public static CashPaymentsViewModel Instance => _instance ??= new CashPaymentsViewModel();

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

    public decimal TotalToPay => Booking?.TotalPrice - Booking?.TotalPaid ?? 0;
    
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
    
    public decimal ChangeGiven => AmountReceived - PricePaid;

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

    public CashPaymentsViewModel()
    {
        PricePaid = 0;
        AmountReceived = 0;
        _ = LoadBooking();
        SaveCommand = new AsyncRelayCommand(SavePayment);
    }




    public ICommand SaveCommand { get; }

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