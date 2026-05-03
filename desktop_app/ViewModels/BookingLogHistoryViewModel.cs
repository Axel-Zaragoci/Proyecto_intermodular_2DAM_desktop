namespace desktop_app.ViewModels;

public class BookingLogHistoryViewModel : ViewModelBase
{
    private static BookingLogHistoryViewModel? _instance;
    public static BookingLogHistoryViewModel Instance => _instance ??= new BookingLogHistoryViewModel();
    
    private string _bookingId = "";
    public string BookingId
    {
        get => _bookingId;
        set
        {
            _bookingId = value;
            Console.WriteLine(_bookingId);
        } 
    }
}