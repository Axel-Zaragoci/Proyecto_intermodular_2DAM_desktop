using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Events;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels.Booking;

public class UpdateBookingFormViewModel :  ViewModelBase
{
    private static UpdateBookingFormViewModel? _instance;
    public static UpdateBookingFormViewModel Instance => _instance ??= new UpdateBookingFormViewModel();

    private string _bookingId;

    public string BookingId
    {
        get => _bookingId;
        set {
            _bookingId = value;
            _ = LoadBooking();
            RefreshAll();
        }
    }
    
    private BookingModel _booking;

    public BookingModel Booking
    {
        get => _booking;
        set {
            _booking = value;
            
            if (_booking != null)
            {
                var priceWithDiscount = _booking.PricePerNight ?? 0;
                var currentOffer = _booking.Offer ?? 0;
            
                if (currentOffer > 0)
                {
                    _basePricePerNight = priceWithDiscount / (1 - currentOffer / 100m);
                }
                else
                {
                    _basePricePerNight = priceWithDiscount;
                }
                OnPropertyChanged(nameof(BasePricePerNight));
                OnPropertyChanged(nameof(PricePerNight));
            }
            RefreshAll();
        }
    }
    
    public bool Enabled => string.IsNullOrEmpty(Booking.Id);
    
    public bool Disabled => !Enabled;

    private ObservableCollection<UserModel> _clients;
    public ObservableCollection<UserModel> Clients
    {
        get => _clients;
        set => SetProperty(ref _clients, value);
    }
    
    public string ClientDni
    {
        get => Booking.ClientDni;
        set
        {
            Booking.ClientDni = value;
            OnPropertyChanged();
        }
    }
    
    private ObservableCollection<RoomModel> _rooms = new();
    public ObservableCollection<RoomModel> Rooms
    {
        get => _rooms;
        set => SetProperty(ref _rooms, value);
    }
    
    public string RoomNumber
    {
        get => Booking.RoomNumber;
        set
        {
            Booking.RoomNumber = value;
            ChangeRoomData(Booking.RoomNumber);
            OnPropertyChanged();
        }
    }
    
    public DateTime CheckInDate
    {
        get => Booking.CheckInDate;
        set
        {
            Booking.CheckInDate = value;
            OnPropertyChanged();
            RecalculateTotal();
        }
    }
    
    public DateTime CheckOutDate
    {
        get => Booking.CheckOutDate;
        set
        {
            Booking.CheckOutDate = value;
            OnPropertyChanged();
            RecalculateTotal();
        }
    }
    
    public DateTime PayDate
    {
        get => Booking.PayDate;
        set
        {
            Booking.PayDate = value;
            OnPropertyChanged();
        }
    }
    
    private decimal _basePricePerNight;

    public decimal BasePricePerNight
    {
        get => _basePricePerNight;
        set
        {
            _basePricePerNight = value;
            OnPropertyChanged(nameof(PricePerNight));
            RecalculateTotal();
        }
    }

    public decimal PricePerNight
    {
        get => _basePricePerNight * (1 - Offer / 100) * 1.1m;
        set
        {
            _basePricePerNight = value / 1.1m / (1 - Offer / 100);
            OnPropertyChanged();
            RecalculateTotal();
        }
    }
    
    public decimal Offer
    {
        get => Booking.Offer ?? 0;
        set
        {
            Booking.Offer = value;
            OnPropertyChanged();
            RecalculateTotal();
        }
    }
    
    public int TotalNights
    {
        get => Booking.TotalNights;
        private set
        {
            Booking.TotalNights = value;
            OnPropertyChanged();
        }
    }
    
    public decimal TotalPrice
    {
        get => Booking.TotalPrice;
        private set
        {
            Booking.TotalPrice = value;
            OnPropertyChanged();
        }
    }
    
    
    
    
    
    
    private async Task LoadClients()
    {
        Clients.Clear();
        var users = await UserService.GetAllUsersAsync();
        foreach (var user in users)
            if (user.Rol == "Usuario") Clients.Add(user);
    }
    
    private async Task LoadRooms()
    {
        Rooms.Clear();
        var rooms = (await RoomService.GetRoomsFilteredAsync()).Items;
        foreach (var room in rooms)
            Rooms.Add(room);
    }

    private async Task LoadBooking()
    {
        if (BookingId == "")
        {
            Booking = new BookingModel();
            return;
        }
        Booking = await BookingService.GetBookingAsync(BookingId);
        var room = await RoomService.GetRoomByIdAsync(Booking.Room);
        Booking.RoomNumber = room.RoomNumber;
        var client = await UserService.GetClientByIdAsync(Booking.Client);
        Booking.ClientDni = client.Dni;
        Booking.ClientName = client.FirstName + " " + client.LastName;
        RefreshAll();
    }






    public UpdateBookingFormViewModel()
    {
        Clients = new ObservableCollection<UserModel>();
        Rooms = new ObservableCollection<RoomModel>();
        _ = LoadClients();
        _ = LoadRooms();
        CreateBookingCommand = new AsyncRelayCommand(Save);
    }
    
    
    
    
    
    
    
    
    
    public void ChangeRoomData(String roomNumber)
    {
        var room = _rooms.First(room => room.RoomNumber == roomNumber);
        BasePricePerNight = room.PricePerNight ?? 0;
        Offer = room.Offer ?? 0;
    }
    
    private void RecalculateTotal()
    {
        if (CheckOutDate <= CheckInDate)
        {
            TotalNights = 0;
            TotalPrice = 0;
            return;
        }

        TotalNights = (CheckOutDate.Date - CheckInDate.Date).Days;

        var pricePerNightWithDiscount = _basePricePerNight * (1 - Offer / 100m);
        var subtotal = TotalNights * pricePerNightWithDiscount;
        TotalPrice = subtotal * 1.1m;
    }
    
    private void RefreshAll()
    {
        OnPropertyChanged(nameof(Enabled));
        OnPropertyChanged(nameof(Disabled));

        OnPropertyChanged(nameof(ClientDni));
        OnPropertyChanged(nameof(RoomNumber));

        OnPropertyChanged(nameof(CheckInDate));
        OnPropertyChanged(nameof(CheckOutDate));
        OnPropertyChanged(nameof(PayDate));

        OnPropertyChanged(nameof(PricePerNight));
        OnPropertyChanged(nameof(BasePricePerNight));
        OnPropertyChanged(nameof(Offer));
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(TotalNights));
        OnPropertyChanged(nameof(Booking));
        OnPropertyChanged(nameof(BookingId));
    }
    
    
    
    
    public ICommand CreateBookingCommand { get; set; }
    
    
    private async Task Save()
    {
        try
        {
            if (string.IsNullOrEmpty(Booking.Id))
            {
                var userId = await UserService.GetUserIdByDniAsync(ClientDni);
                Booking.Client = userId;

                var filter = new RoomsFilter { RoomNumber = RoomNumber };
                Booking.Room =
                    (await RoomService.GetRoomsFilteredAsync(filter))
                    ?.Items.First().Id;

                await BookingService.CreateBookingAsync(Booking);
            }
            else
            {
                await BookingService.UpdateBookingAsync(Booking);
            }
            await BookingEvents.RaiseBookingChanged();
            NavigationService.Instance.NavigateTo<BookingView>();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}