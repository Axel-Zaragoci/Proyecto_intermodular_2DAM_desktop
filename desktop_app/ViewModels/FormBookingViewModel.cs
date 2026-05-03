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

        
        
        
        

        private FormBookingViewModel()
        {
            BookingId = "";
            CurrentView = new UpdateView();
            NavigateToDetailsCommand = new RelayCommand(NavigateToDetails);
            NavigateToPaymentCommand = new RelayCommand(NavigateToPayment);
            DownloadInvoiceCommand = new AsyncRelayCommand(DownloadInvoiceAsync);
            CancelBookingCommand = new AsyncRelayCommand(Cancel);
        }

        
        
        private async Task Cancel()
        {
            try
            {
                var booking = await BookingService.GetBookingAsync(BookingId);
                if (booking.Status == "Cancelada")
                {
                    MessageBox.Show("Esta reserva ya está cancelada");
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
        
        private async Task DownloadInvoiceAsync()
        {
            try
            {
                var booking =  await BookingService.GetBookingAsync(BookingId);
                
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
        
        public void NavigateToDetails(object obj)
        {
            CurrentView = new UpdateView();
            UpdateBookingFormViewModel.Instance.BookingId = BookingId;
        }

        public void NavigateToPayment(object obj)
        {
            CurrentView = new BasePaymentsView();
            BasePaymentsViewModel.Instance.BookingId = BookingId;
            BasePaymentsViewModel.Instance.NavigateToFormView("Efectivo");
        }
        
        public ICommand ReturnCommand { get; } =
            new RelayCommand(_ =>
                NavigationService.Instance.NavigateTo<BookingView>());

        public ICommand NavigateToDetailsCommand { get; }
        
        public ICommand NavigateToPaymentCommand { get; }
        
        public ICommand DownloadInvoiceCommand { get; }
        
        public ICommand CancelBookingCommand { get; }
        
        
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
