using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using Microsoft.Win32;

namespace desktop_app.ViewModels;

public class PaymentsViewModel : ViewModelBase
{
    private ObservableCollection<BasePaymentModel> _payments = new();
    public ObservableCollection<BasePaymentModel> Payments
    {
        get => _payments;
        set => SetProperty(ref _payments, value);
    }
    
    private ObservableCollection<UserModel> Users { get; set; }
    
    private ICollectionView _paymentsView;
    public ICollectionView PaymentsView
    {
        get => _paymentsView;
        set => SetProperty(ref _paymentsView, value);
    }

    private string _filterBookingId = "";
    public string FilterBookingId
    {
        get => _filterBookingId;
        set {if (SetProperty(ref _filterBookingId, value)) PaymentsView?.Refresh();}
    }

    private string _filterClientName = "";
    public string FilterClientName
    {
        get  => _filterClientName;
        set {if (SetProperty(ref _filterClientName, value)) PaymentsView?.Refresh();}
    }

    public ObservableCollection<string> Methods { get; } = new ObservableCollection<string>() { "Todos", "Tarjeta", "Efectivo", "Transferencia" };
    private string _selectedMethod = "Todos";
    public string SelectedMethod
    {
        get => _selectedMethod;
        set {if (SetProperty(ref _selectedMethod, value)) PaymentsView?.Refresh();}
    }
    
    private DateTime _selectedDate = DateTime.Now;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set {if (SetProperty(ref _selectedDate, value)) PaymentsView?.Refresh();}
    }

    public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
    private string _selectedDateFilter = "Antes de";
    public string SelectedDateFilter
    {
        get => _selectedDateFilter;
        set {if (SetProperty(ref _selectedDateFilter, value)) PaymentsView?.Refresh();}
    }


    public PaymentsViewModel()
    {
        Payments = new ObservableCollection<BasePaymentModel>();
        PaymentsView = CollectionViewSource.GetDefaultView(Payments);
        PaymentsView.Filter = FilterPayments;
        _ = LoadData();
        ReloadCommand = new AsyncRelayCommand(LoadData);
        ShowPaymentCommand = new RelayCommand<BasePaymentModel>(ShowPayment);
        ExportToCsvCommand = new RelayCommand(ExportToCsv);
    }


    private async Task LoadData()
    {
        List<BasePaymentModel> paymentsList = await PaymentService.GetPayments();
        
        List<UserModel> userList = await UserService.GetAllUsersAsync();
        Users = new ObservableCollection<UserModel>(userList);
        
        foreach (var payment in paymentsList)
        {
            var client = userList.Find(u => u.Id == payment.ClientId);
            payment.ClientName = client == null ? "No encontrado" : $"{client.FirstName} {client.LastName}";

            if (payment.ReceivedBy.UserId == payment.ClientId)
            {
                payment.ReceiverName = "";
            }
            else
            {
                var receiver = userList.Find(u => u.Id == payment.ReceivedBy.UserId);
                payment.ReceiverName = receiver == null ? "No encontrado" : $"{receiver.FirstName} {receiver.LastName}";
            }
            
            payment.BookingId = payment.BookingId.Substring(0, 7);
        }
        
        Payments.Clear();
        foreach (var payment in paymentsList) {Payments.Add(payment);}

        PaymentsView.Refresh();
        OnPropertyChanged(nameof(PaymentsView));
    }
    
    private bool FilterPayments(object obj)
    {
        if (obj is not BasePaymentModel payment) return false;

        bool Match(string? value, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;
            return (value ?? "").IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool idMatch = Match(payment.BookingId, FilterBookingId);
        bool nameMatch = Match(payment.ClientName, FilterClientName);
        string paymentMethod = SelectedMethod == "Todos" ? "" : SelectedMethod;
        bool typeMatch = Match(payment.PaymentMethod, paymentMethod);

        bool dateMatch = true;
        DateTime paymentDate = payment.PaymentDate;
        switch (_selectedDateFilter)
        {
            case "Fecha exacta":
                dateMatch = paymentDate.Date == SelectedDate.Date;
                break;
            case "Antes de":
                dateMatch = paymentDate.Date <= SelectedDate.Date;
                break;
            case "Después de":
                dateMatch = paymentDate.Date >= SelectedDate.Date;
                break;
        }

        bool result = idMatch && nameMatch && typeMatch && dateMatch;
        return result;
    }

    private void ExportToCsv(object obj)
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archviso CSV (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"Pagos_{DateTime.Now:yyyyMMdd_HHmmss}",
                Title = "Exportar pagos a CSV",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                PaymentsExporter.ExportToCsv(Payments.ToList(), saveFileDialog.FileName);

                MessageBox.Show($"Pagos exportados correctamente a:\n{saveFileDialog.FileName}", "Exportación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar pagos:\n{ex.Message}", "Error al exportar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    } 
    

    private void ShowPayment(BasePaymentModel payment)
    {
        MessageBox.Show(payment.ToString(), $"Datos del pago {payment.Id}",  MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    public ICommand ReloadCommand { get; }
    public ICommand ShowPaymentCommand { get; }
    public ICommand ExportToCsvCommand { get; }
}