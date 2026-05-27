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
    private List<BasePaymentModel> _allPayments = new();
    public ObservableCollection<BasePaymentModel> Payments { get; }
    
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set {if(SetProperty(ref _currentPage, value)) ApplyPagination();}
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set { if (SetProperty(ref _pageSize, value)) ApplyPagination(); }
    }
        
    private int _totalItems = 0;
        
    public int TotalPages => (int)Math.Ceiling((double)_totalItems / PageSize);
        
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
        
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand GoToPageCommand { get; }
        
    public ObservableCollection<int> PageSizeOptions { get; } = new ObservableCollection<int> { 5, 10, 15, 20, 50 };
    
    
    
    private ObservableCollection<UserModel> Users { get; set; }

    private string _filterBookingId = "";
    public string FilterBookingId
    {
        get => _filterBookingId;
        set {
            if (SetProperty(ref _filterBookingId, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();   
            }
        }
    }

    private string _filterClientName = "";
    public string FilterClientName
    {
        get  => _filterClientName;
        set
        {
            if (SetProperty(ref _filterClientName, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            }
        }
    }

    public ObservableCollection<string> Methods { get; } = new ObservableCollection<string>() { "Todos", "Tarjeta", "Efectivo", "Transferencia" };
    private string _selectedMethod = "Todos";
    public string SelectedMethod
    {
        get => _selectedMethod;
        set {
            if (SetProperty(ref _selectedMethod, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            }
        }
    }
    
    private DateTime _selectedDate = DateTime.Now;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            }
        }
    }

    public ObservableCollection<string> DateFilterTypes { get; } = new ObservableCollection<string>() { "Fecha exacta" ,"Antes de", "Después de" };
    private string _selectedDateFilter = "Antes de";
    public string SelectedDateFilter
    {
        get => _selectedDateFilter;
        set
        {
            if (SetProperty(ref _selectedDateFilter, value))
            {
                CurrentPage = 1;
                ApplyFiltersAndPagination();
            }
        }
    }


    public PaymentsViewModel()
    {
        _ = LoadData();
        
        Payments = new ObservableCollection<BasePaymentModel>();
        
        FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => HasPreviousPage);
        PreviousPageCommand  = new RelayCommand(_ => GoToPreviousPage(), _ => HasPreviousPage);
        NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => HasNextPage);
        LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => HasNextPage);
        GoToPageCommand = new RelayCommand<string>(GoToPage, CanGoToPage);
        
        ReloadCommand = new AsyncRelayCommand(LoadData);
        ShowPaymentCommand = new RelayCommand<BasePaymentModel>(ShowPayment);
        ExportToCsvCommand = new RelayCommand(ExportToCsv);
    }

    private void GoToFirstPage() => CurrentPage = 1;
    private void GoToPreviousPage() => CurrentPage--;
    private void GoToNextPage() => CurrentPage++;
    private void GoToLastPage() => CurrentPage = TotalPages;
        
    private void GoToPage(string? pageNumber)
    {
        if (int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }
        
    private bool CanGoToPage(string? pageNumber)
    {
        return int.TryParse(pageNumber, out int page) && page >= 1 && page <= TotalPages;
    }

    private List<BasePaymentModel> GetFilteredPayments()
    {
        if (_allPayments == null || !_allPayments.Any()) return new List<BasePaymentModel>();

        return _allPayments.Where(payment => FilterPayments(payment)).ToList();
    }
    
    private void ApplyFiltersAndPagination()
    {
        var filtered = GetFilteredPayments();
        _totalItems = filtered.Count;
        ApplyPagination();
    }

    private void ApplyPagination()
    {
        var filtered = GetFilteredPayments();
        var paginated = filtered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
            
        Payments.Clear();
        foreach (var booking in paginated)
        {
            Payments.Add(booking);
        }
            
        CommandManager.InvalidateRequerySuggested();

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(CurrentPage));
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
        
        _allPayments.Clear();
        foreach (var payment in paymentsList) {_allPayments.Add(payment);}

        CurrentPage = 1;
        ApplyFiltersAndPagination();
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