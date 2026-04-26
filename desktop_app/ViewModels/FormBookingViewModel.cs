using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Events;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels
{
    public class FormBookingViewModel : ViewModelBase
    {
        private static FormBookingViewModel? _instance;
        /// <summary>
        /// Obtiene la instancia única del ViewModel (patrón Singleton).
        /// </summary>
        public static FormBookingViewModel Instance => _instance ??= new FormBookingViewModel();

        /// <summary>
        /// Obtiene el título de la ventana basado en si la reserva es nueva o existente.
        /// </summary>
        public string Title => string.IsNullOrEmpty(Booking.Id) ? "Crear reserva" : "Actualizar reserva";

        /// <summary>
        /// Indica si los campos del formulario deben estar habilitados.
        /// Devuelve true para nuevas reservas, false para reservas existentes.
        /// </summary>
        public bool Enabled => string.IsNullOrEmpty(Booking.Id);

        public bool InvoiceEnable 
        { 
            get 
            {
                var result = Math.Abs(Booking.TotalPaid - Booking.TotalPrice) < 0.01m;
                return result;
            }
        }
        
        /// <summary>
        /// Indica si los campos del formulario deben estar deshabilitados.
        /// </summary>
        public bool Disabled => !Enabled;


        private BookingModel _booking;
        /// <summary>
        /// Obtiene o establece el modelo de reserva actual.
        /// </summary>
        public BookingModel Booking
        {
            get => _booking;
            set
            {
                _booking = value;
                OnPropertyChanged();
                
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
                    OnPropertyChanged(nameof(InvoiceEnable));
                }
                RefreshAll();
            }
        }


        private ObservableCollection<UserModel> _clients = new();
        /// <summary>
        /// Colección observable de clientes disponibles para seleccionar en el formulario.
        /// </summary>
        public ObservableCollection<UserModel> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }

        /// <summary>
        /// Carga asincrónicamente la lista de clientes desde el servicio de usuarios.
        /// </summary>
        private async void LoadClients()
        {
            Clients.Clear();
            var users = await UserService.GetAllUsersAsync();
            foreach (var user in users)
                if (user.Rol == "Usuario") Clients.Add(user);
        }

        /// <summary>
        /// Obtiene o establece el DNI del cliente asociado a la reserva.
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


        private ObservableCollection<RoomModel> _rooms = new();
        /// <summary>
        /// Colección observable de habitaciones disponibles para seleccionar en el formulario.
        /// </summary>
        public ObservableCollection<RoomModel> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        /// <summary>
        /// Carga asincrónicamente la lista de habitaciones filtradas desde el servicio de habitaciones.
        /// </summary>
        private async void LoadRooms()
        {
            Rooms.Clear();
            var rooms = (await RoomService.GetRoomsFilteredAsync()).Items;
            foreach (var room in rooms)
                Rooms.Add(room);
        }

        /// <summary>
        /// Obtiene o establece el número de habitación seleccionado.
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

        public void ChangeRoomData(String roomNumber)
        {
            var room = _rooms.First(room => room.RoomNumber == roomNumber);
            BasePricePerNight = room.PricePerNight ?? 0;
            Offer = room.Offer ?? 0;
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

        /// <summary>
        /// Obtiene o establece la fecha de fin de la reserva.
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
        /// Obtiene o establece la fecha de pago de la reserva.
        /// </summary>
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

        /// <summary>
        /// Obtiene o establece el porcentaje de oferta aplicado a la reserva.
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
        /// Obtiene el precio total calculado de la reserva.
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
        /// Obtiene el número total de noches calculado de la reserva.
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
        /// Comando para guardar la reserva.
        /// </summary>
        public ICommand SaveCommand { get; }
        
        public ICommand InvoiceCommand { get; }
        
        /// <summary>
        /// Comando para cancelar la reserva.
        /// </summary>
        public ICommand CancelCommand { get; }

        public ICommand ReturnCommand { get; } =
            new RelayCommand(_ =>
                NavigationService.Instance.NavigateTo<BookingView>());

        /// <summary>
        /// Constructor privado que inicializa una nueva instancia del ViewModel.
        /// Configura los comandos y carga los datos iniciales.
        /// </summary>
        private FormBookingViewModel()
        {
            Booking = new BookingModel();

            SaveCommand = new RelayCommand(async _ => await Save());
            CancelCommand = new RelayCommand(async _ => await Cancel());
            InvoiceCommand = new AsyncRelayCommand<BookingModel>(DownloadInvoiceAsync);
            
            LoadClients();
            LoadRooms();
        }

        /// <summary>
        /// Recalcula el precio total y el número de noches basado en las fechas seleccionadas.
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
        /// Actualiza todas las propiedades del ViewModel notificando cambios en la UI.
        /// </summary>
        private void RefreshAll()
        {
            OnPropertyChanged(nameof(Title));
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
            
            OnPropertyChanged(nameof(InvoiceEnable));
        }

        /// <summary>
        /// Guarda la reserva actual (crea una nueva o actualiza una existente).
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
        
        /// <summary>
        /// Cancela la reserva actual después de la confirmación del usuario.
        /// </summary>
        private async Task Cancel()
        {
            if (Booking.Status == "Cancelada")
            {
                MessageBox.Show("Esta reserva ya está cancelada");
                return;
            }

            if (MessageBox.Show("¿Cancelar reserva?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                await BookingService.CancelBookingAsync(Booking.Id);
                await BookingEvents.RaiseBookingChanged();
                NavigationService.Instance.NavigateTo<BookingView>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task DownloadInvoiceAsync(BookingModel booking)
    {
        try
        {
            byte[] pdfBytes = await InvoiceService.DownloadPdfAsync(booking);

            if (pdfBytes.Length > 0)
            {
                string tempFile = await GetTempFileRoute(booking);
            
                File.WriteAllBytes(tempFile, pdfBytes);
            
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al mostrar factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task<String> GetTempFileRoute(BookingModel booking)
    {
        if (booking.InvoiceId != "")
        {
            return Path.Combine(Path.GetTempPath(), booking.InvoiceId);
        }
        else
        {
            BookingModel bookingWithInvoice = await BookingService.GetBookingAsync(booking.Id);
            return Path.Combine(Path.GetTempPath(), bookingWithInvoice.InvoiceId);
        }
    }
    }
}
