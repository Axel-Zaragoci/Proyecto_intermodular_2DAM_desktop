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
    /// <summary>
    /// Implementación del patrón Singleton
    /// </summary>
    private static UpdateBookingFormViewModel? _instance;
    public static UpdateBookingFormViewModel Instance => _instance ??= new UpdateBookingFormViewModel();

    /// <summary>
    /// Propiedad que almacena el ID de la reserva que se quiere actualizar
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena el objeto de la reserva a editar
    /// </summary>
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
    
    /// <summary>
    /// Propiedades para activar/desactivar campos según se utilice el formulario para crear o actualizar una reserva
    /// </summary>
    public bool Enabled => string.IsNullOrEmpty(Booking.Id);
    public bool Disabled => !Enabled;

    /// <summary>
    /// Propiedad que almacena una lista de los clientes
    /// </summary>
    private ObservableCollection<UserModel> _clients;
    public ObservableCollection<UserModel> Clients
    {
        get => _clients;
        set => SetProperty(ref _clients, value);
    }
    
    /// <summary>
    /// Propiedad que almacena el DNI del cliente asociado a la reserva
    /// </summary>
    public string ClientDni
    {
        get => Booking.ClientDni;
        set
        {
            Booking.ClientDni = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena una lista de las habitaciones
    /// </summary>
    private ObservableCollection<RoomModel> _rooms = new();
    public ObservableCollection<RoomModel> Rooms
    {
        get => _rooms;
        set => SetProperty(ref _rooms, value);
    }
    
    /// <summary>
    /// Propiedad que almacena el número de la habitación asociada a la reserva
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena la fecha de inicio de la reserva
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena la fecha de fin de la reserva
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena la fecha de creación de la reserva
    /// </summary>
    public DateTime CreationDate
    {
        get => Booking.CreationDate;
        set
        {
            Booking.CreationDate = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena el precio por noche de la habitación
    /// </summary>
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

    /// <summary>
    /// Propiedad que almacena el precio por noche de la habitación aplicando el descuento y el IVA
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena el procentaje de descuento
    /// </summary>
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
    
    /// <summary>
    /// Propiedad que almacena el total de noches de la reserva
    /// </summary>
    public int TotalNights
    {
        get => Booking.TotalNights;
        private set
        {
            Booking.TotalNights = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Propiedad que almacena el precio total de la reserva
    /// </summary>
    public decimal TotalPrice
    {
        get => Booking.TotalPrice;
        private set
        {
            Booking.TotalPrice = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Función que carga los clientes
    /// FLUJO:
    /// - Vacía la lista existente de clientes
    /// - Obtiene todos los usuarios
    /// - Añade a la lista todos los usuarios con el rol de cliente
    /// </summary>
    private async Task LoadClients()
    {
        Clients.Clear();
        var users = await UserService.GetAllUsersAsync();
        foreach (var user in users)
            if (user.Rol == "Usuario") Clients.Add(user);
    }
    
    /// <summary>
    /// Función que carga la lista de habitaciones
    /// FLUJO:
    /// - Vacía la lista existente de habitaciones
    /// - Obtiene todas las habitaciones
    /// - Añade a la lista todas las habitaciones
    /// </summary>
    private async Task LoadRooms()
    {
        Rooms.Clear();
        var rooms = (await RoomService.GetRoomsFilteredAsync()).Items;
        foreach (var room in rooms)
            Rooms.Add(room);
    }

    /// <summary>
    /// Función que carga la reserva
    /// FLUJO:
    /// - Obtiene la reserva filtrando por ID (o en caso de crear una reserva nueva se crea una reserva en blanco)
    /// - Obtiene la habitación y el cliente
    /// - Asigna los datos necesarios a la reserva
    /// - Refresca todas las propiedades en la interfaz
    /// </summary>
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

    /// <summary>
    /// Constructor
    /// Inicia las listas de clientes y habitaciones, carga sus datos e inicia el comando de guardado
    /// </summary>
    public UpdateBookingFormViewModel()
    {
        Clients = new ObservableCollection<UserModel>();
        Rooms = new ObservableCollection<RoomModel>();
        _ = LoadClients();
        _ = LoadRooms();
        CreateBookingCommand = new AsyncRelayCommand(Save);
    }
    
    /// <summary>
    /// Función que cambia los datos si se cambia la habitación
    /// </summary>
    /// <param name="roomNumber">
    /// Número de la habitación seleccionada
    /// </param>
    public void ChangeRoomData(String roomNumber)
    {
        var room = _rooms.First(room => room.RoomNumber == roomNumber);
        BasePricePerNight = room.PricePerNight ?? 0;
        Offer = room.Offer ?? 0;
    }
    
    /// <summary>
    /// Función que calcula el total de noches y el precio total al realizar un cambio
    /// </summary>
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
    
    /// <summary>
    /// Función que refresca todas las propiedades en la vista
    /// </summary>
    private void RefreshAll()
    {
        OnPropertyChanged(nameof(Enabled));
        OnPropertyChanged(nameof(Disabled));

        OnPropertyChanged(nameof(ClientDni));
        OnPropertyChanged(nameof(RoomNumber));

        OnPropertyChanged(nameof(CheckInDate));
        OnPropertyChanged(nameof(CheckOutDate));
        OnPropertyChanged(nameof(CreationDate));

        OnPropertyChanged(nameof(PricePerNight));
        OnPropertyChanged(nameof(BasePricePerNight));
        OnPropertyChanged(nameof(Offer));
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(TotalNights));
        OnPropertyChanged(nameof(Booking));
        OnPropertyChanged(nameof(BookingId));
    }
    
    /// <summary>
    /// Comando de guardado de la reserva creada o modificada
    /// </summary>
    public ICommand CreateBookingCommand { get; set; }
    
    /// <summary>
    /// Función de guardado de la reserva
    /// FLUJO:
    /// - En caso de que NO hay un ID de la reserva (caso de creación)
    ///     - Obtiene el ID del cliente y de la habitación
    ///     - Asigna los IDs a la reserva
    ///     - Manda la reserva a la API para su registro en la base de datos
    ///     - Avisa de que ha habido un cambio en las reservas
    ///     - Navega a la vista de todas las reservas
    /// - En caso de haber ID de la reserva (caso de actualizar)
    ///     - Manda a la API la reserva para actualizarla
    ///     - Avisa de que ha habido un cambio en las reservas
    ///     - Navega a la vista de todas las reservas
    /// </summary>
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