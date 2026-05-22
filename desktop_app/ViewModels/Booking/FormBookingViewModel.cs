using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Events;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;
using desktop_app.Views.BookingViews;

namespace desktop_app.ViewModels.Booking
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
        public string Title => string.IsNullOrEmpty(BookingId) ? "Crear reserva" : "Actualizar reserva";
        
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
                OnPropertyChanged(nameof(BookingId));
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Vista mostrada en la página
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
        /// Constructor del viewmodel
        ///
        /// Carga una vista
        /// Inicia los comandos
        /// </summary>
        private FormBookingViewModel()
        {
            BookingId = "";
            CurrentView = new UpdateView();
            NavigateToDetailsCommand = new RelayCommand(NavigateToDetails);
            NavigateToPaymentCommand = new RelayCommand(NavigateToPayment);
            NavigateToLogsCommand = new RelayCommand(NavigateToLogs);
            DownloadInvoiceCommand = new AsyncRelayCommand(DownloadInvoiceAsync);
            CancelBookingCommand = new AsyncRelayCommand(Cancel);
            CheckInCommand = new AsyncRelayCommand(CheckIn);
            CheckOutCommand = new AsyncRelayCommand(CheckOut);
        }

        /// <summary>
        /// Función para cancelar la reserva
        /// Obtiene la reserva y verifica que no esté ya cancelada, en proceso o finalizada
        /// Pide confirmación para la cancelación
        /// Indica a la api que cancele la reserva, avisa a la vista de reservas que ha habido un cambio y navega a dicha vista
        /// </summary>
        private async Task Cancel()
        {
            try
            {
                var booking = await BookingService.GetBookingAsync(BookingId);
                if (booking.Status == "Cancelada" || booking.Status == "Check-in" || booking.Status == "Check-out")
                {
                    MessageBox.Show("Esta reserva ya está cancelada, en proceso o ya finalizada");
                    return;
                }

                if (MessageBox.Show("¿Cancelar reserva?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                
                await BookingService.CancelBookingAsync(booking.Id);
                await BookingEvents.RaiseBookingChanged();
                NavigationService.Instance.NavigateTo<BookingView>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Función para realizar el check-in
        /// Pide confirmación para el check-in
        /// Indica a la api que realice el check-in, avisa a la vista de reservas que ha habido un cambio y navega a dicha vista
        /// </summary>
        private async Task CheckIn()
        {
            try
            {
                if (MessageBox.Show("¿Realizar check-in de la reserva?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                
                await BookingService.CheckInBookingAsync(_bookingId);
                await BookingEvents.RaiseBookingChanged();
                NavigationService.Instance.NavigateTo<BookingView>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Función para realizar el check-out
        /// Pide confirmación para el check-out
        /// Indica a la api que realice el check-out, avisa a la vista de reservas que ha habido un cambio y navega a dicha vista
        /// </summary>
        private async Task CheckOut()
        {
            try
            {
                if (MessageBox.Show("¿Realizar check-out de la reserva?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                
                await BookingService.CheckOutBookingAsync(_bookingId);
                await BookingEvents.RaiseBookingChanged();
                NavigationService.Instance.NavigateTo<BookingView>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Función que descarga la factura
        /// Obtiene la reserva completa
        /// Verifica que ya haya finalizado
        /// Obtiene la factura del servicio
        /// Lo guarda en la ruta de archivos temporales
        /// Abre la app por defecto para los pdfs y la muestra
        /// </summary>
        private async Task DownloadInvoiceAsync()
        {
            try
            {
                var booking =  await BookingService.GetBookingAsync(BookingId);

                if (booking.CheckOutDate.Date < DateTime.Now.Date)
                {
                    MessageBox.Show("Solo se pueden generar facturas de reservas finalizadas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
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
        
        /// <summary>
        /// Muestra la vista de detalles y actualizar la reserva
        /// Le pasa el ID de la reserva
        /// </summary>
        /// 
        /// <param name="obj">
        /// Parametro necesario para el evento
        /// </param>
        public void NavigateToDetails(object obj)
        {
            CurrentView = new UpdateView();
            UpdateBookingFormViewModel.Instance.BookingId = BookingId;
        }

        /// <summary>
        /// Navega a la vista de registrar pago
        /// Le pasa el ID de la reserva
        /// </summary>
        /// 
        /// <param name="obj">
        /// Parametro necesario para el evento
        /// </param>
        public void NavigateToPayment(object obj)
        {
            CurrentView = new BasePaymentsView();
            BasePaymentsViewModel.Instance.BookingId = BookingId;
            BasePaymentsViewModel.Instance.NavigateToFormView("Efectivo");
        }
        
        
        /// <summary>
        /// Navega a la vista de cronología
        /// Le pasa el ID de la reserva
        /// </summary>
        /// 
        /// <param name="obj">
        /// Parametro necesario para el evento
        /// </param>
        public void NavigateToLogs(object obj)
        {
            CurrentView = new LogsView();
            BookingLogHistoryViewModel.Instance.BookingId = BookingId;
        }
        
        /// <summary>
        /// Comando que navega de vuelta a la vista de reservas
        /// </summary>
        public ICommand ReturnCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<BookingView>());

        /// <summary>
        /// Comandos para navegar y las acciones
        /// </summary>
        public ICommand NavigateToDetailsCommand { get; }
        public ICommand NavigateToPaymentCommand { get; }
        public ICommand NavigateToLogsCommand { get; }
        public ICommand DownloadInvoiceCommand { get; }
        public ICommand CancelBookingCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand CheckOutCommand { get; }
        
        /// <summary>
        /// Obtiene la ruta para la factura
        /// Junta el directorio de archivos temporales y el nobmre de fichero de la factura
        /// </summary>
        /// 
        /// <param name="booking">
        /// Objeto de la reserva cuya factura se quiere guardar
        /// </param>
        /// 
        /// <returns>
        /// Cadena de texto de la ruta en la que guardar el fichero
        /// </returns>
        private async Task<String> GetTempFileRoute(BookingModel booking)
        {
            if (booking.InvoiceId != "")
            {
                return Path.Combine(Path.GetTempPath(), booking.InvoiceId);
            }
            var bookingWithInvoice = await BookingService.GetBookingAsync(booking.Id);
            return Path.Combine(Path.GetTempPath(), bookingWithInvoice.InvoiceId);
        }
    }
}
