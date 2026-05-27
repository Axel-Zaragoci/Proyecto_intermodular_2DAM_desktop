using System.Windows;
using System.Windows.Input;
using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;

namespace desktop_app.ViewModels.Invoices;

public class FormHotelViewModel : ViewModelBase
{
    /// <summary>
    /// Propiedad que almacena el objeto del hotel
    /// </summary>
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
    
    /// <summary>
    /// Comandos para guardar los datos del hotel y navegar a la vista anterior
    /// </summary>
    public ICommand ReturnCommand { get; } = new RelayCommand(_ => NavigationService.Instance.NavigateTo<InvoicesView>());
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Constructor
    /// Se encarga de iniciar el comando de guardado y de cargar los datos del hotel
    /// </summary>
    public FormHotelViewModel()
    {
        _ = LoadHotel();
        SaveCommand = new AsyncRelayCommand(SaveHotel);
    }

    /// <summary>
    /// Función que guarda los datos del hotel
    /// FLUJO:
    /// - Obtiene el servicio del hotel
    /// - Manda los datos del hotel a la API para actualizar
    /// - Muestra mensaje de confirmación
    /// - Navega a la vista anterior
    /// </summary>
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

    /// <summary>
    /// Función que carga los datos del hotel
    /// </summary>
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