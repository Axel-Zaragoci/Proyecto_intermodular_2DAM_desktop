using System.ComponentModel;
using System.Windows.Controls;
using desktop_app.Models;
using desktop_app.Views;

namespace desktop_app.Services
{
    /// <summary>
    /// Servicio para la navegación entre vistas
    /// Implementa la interfaz <see cref="INotifyPropertyChanged"/> para notificar cambios en las propiedades a la vista
    /// </summary>
    /// 
    /// <remarks>Utiliza una única instancia para evitar tener múltiples colas de vistas</remarks>
    public class NavigationService : INotifyPropertyChanged
    {
        /// <summary>
        /// Atributo privado que almacena una instancia de esta clase si todavía no ha sido instanciada
        /// </summary>
        private static NavigationService? _instance;


        /// <summary>
        /// Propiedad pública de la clase para acceder a la instancia
        /// </summary>
        public static NavigationService Instance => _instance ??= new NavigationService();
        
        /// <summary>
        /// Pila de vistas visitadas por el usuario
        /// Para indicar que UserView es la primera vista, la añado de base al stack
        /// </summary>
        private List<UserControl> _stackViews = [new BookingView()];

        /// <summary>
        /// Acción que se ejecuta cuando se necesita resetear el scroll al inicio
        /// La MainWindow se suscribirá a esta acción
        /// </summary>
        public Action? ScrollToTopRequested;

        /// <summary>
        /// Propiedad para obtener o modificar la vista actual
        /// Para obtener la vista actual, devuelve la vista en la primera posición de la pila
        /// Para cambiar de vista, inserta la vista al inicio de la pila
        /// </summary>
        public UserControl CurrentView
        {
            get => _stackViews[0];
            protected set
            {
                if (_stackViews[0] == value) return;
                StackViews.Insert(0, value);
            }
        }


        /// <summary>
        /// Propiedad para obtener la pila de vistas y modificarla
        /// </summary>
        public List<UserControl> StackViews
        {
            get => _stackViews;
        }


        /// <summary>
        /// Función para cambiar la vista actual
        /// Si ya se ha creado anteriormente la vista a la que se busca ir, cambia a esa vista
        /// En caso de no haberse creado anteriormente, crea una nueva
        /// Envía un aviso de cambio de la vista actual
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// Vista a la que se quiere navegar
        /// Debe ser descendiente de UserControl y tener un constructor
        /// </typeparam>
        public void NavigateTo<T> () where T : UserControl, new()
        {
            CurrentView = StackViews.Find(e => e is T) ?? new T();
            OnPropertyChanged(nameof(CurrentView));
            ScrollToTopRequested?.Invoke();
        }

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
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void NavigateTo(Func<UserControl> factory)
        {
            var view = factory?.Invoke();
            if (view == null) return;

            CurrentView = view;
            OnPropertyChanged(nameof(CurrentView));
            ScrollToTopRequested?.Invoke();
        }
    }
}
