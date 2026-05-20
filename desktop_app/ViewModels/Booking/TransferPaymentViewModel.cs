using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;

namespace desktop_app.ViewModels.Booking;

public class TransferPaymentViewModel : ViewModelBase
{
    private static TransferPaymentViewModel? _instance;
    public static TransferPaymentViewModel Instance => _instance ??= new TransferPaymentViewModel();

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
    
    
    
    public ICommand SaveCommand { get; }

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