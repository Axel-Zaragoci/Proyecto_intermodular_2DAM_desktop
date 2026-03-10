using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace desktop_app.ViewModels
{


    /// <summary>
    /// Clase base para los ViewModels que implementa la interfaz <see cref="INotifyPropertyChanged"/> para notificar cambios en las propiedades a la vista
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {


        /// <summary>
        /// Evento que se dispara cuando cambia el valor de una propiedad
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifica que una propiedad ha cambiado
        /// </summary>
        /// 
        /// <param name="propertyName">
        /// Nombre de la propiedad cuyo valor ha cambiado
        /// </param>
 

        // Este método se llama cada vez que cambias una propiedad
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper: asigna valor y notifica solo si cambió
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}

