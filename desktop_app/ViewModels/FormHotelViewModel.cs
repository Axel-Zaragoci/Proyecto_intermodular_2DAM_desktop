using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels;

public class FormHotelViewModel : ViewModelBase
{
    private HotelModel _hotel;

    public HotelModel Hotel
    {
        get => _hotel;
        set
        {
            _hotel = value;
            OnPropertyChanged(nameof(Hotel));
        }
    }
    
    public ICommand ReturnCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<InvoicesView>());
    
    public ICommand SaveCommand { get; }

    public FormHotelViewModel()
    {
        _ = LoadHotel();
        SaveCommand = new AsyncRelayCommand(SaveHotel);
    }

    private async Task SaveHotel()
    {
        try
        {
            HotelService service = new HotelService();
            await service.UpdateHotelAsync(Hotel);
            MessageBox.Show("Hotel actualizado", "Guardado", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.Instance.NavigateTo<InvoicesView>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            MessageBox.Show("Error actualizando los datos del hotel", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadHotel()
    {
        try
        {
            HotelService service = new HotelService();
            Hotel = await service.GetHotelAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            MessageBox.Show("Error cargando el hotel", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}