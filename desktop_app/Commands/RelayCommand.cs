using System.Windows.Input;

namespace desktop_app.Commands
{
    /// <summary>
    /// Implementación del patrón Commnad para WPF
    /// Permite enlazar acciones y condiciones de ejecución desde el ViewModel
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Inicializa una nueva instancia de RelayCommand con una acción de ejecución y una condición de permiso
        /// </summary>
        /// 
        /// <param name="execute">
        /// Acción a ejecutar al invocar el comando
        /// </param>
        /// <param name="canExecute">
        /// Condición para determinar si se puede ejecutar
        /// </param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }


        /// <summary>
        /// Inicializa una nueva instancia de <see cref="RelayCommand"/> que siempre puede ejecutarse
        /// </summary>
        /// 
        /// <param name="execute">
        /// Acción que se ejecutará cuando se invoque el comando
        /// </param>
        public RelayCommand(Action<object> execute) : this(execute, null) { }


        /// <summary>
        /// Evento que notifica a la interfaz cuando cambia la capacidad de ejecución del comando
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        
        /// <summary>
        /// Determina si el comando puede ejecutarse en su estado actual
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parámetro pasado desde la vista
        /// </param>
        /// 
        /// <returns>
        /// true si el comando puede ejecutarse
        /// false si el comando NO puede ejecutarse 
        /// </returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }


        /// <summary>
        /// Ejecuta la acción asociada al comando
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parámetro pasado desde la vista
        /// </param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// Funcionalidad de RelayCommand con un parámetro
    /// </summary>
    /// <typeparam name="T">
    /// Modelo o clase del objeto que se quiere pasar a la función que se ejecuta con el comando
    /// </typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        /// <summary>
        /// Inicializa una nueva instancia de RelayCommand con una acción de ejecución y una condición de permiso con un parámetro de tipo T
        /// </summary>
        /// 
        /// <param name="execute">
        /// Acción a ejecutar al invocar el comando
        /// </param>
        /// <param name="canExecute">
        /// Condición para determinar si se puede ejecutar
        /// </param>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="RelayCommand"/> con un parámetro de tipo T que siempre puede ejecutarse
        /// </summary>
        /// 
        /// <param name="execute">
        /// Acción que se ejecutará cuando se invoque el comando
        /// </param>
        public RelayCommand(Action<T> execute) : this(execute, null) { }
        
        /// <summary>
        /// Evento que notifica a la interfaz cuando cambia la capacidad de ejecución del comando
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Determina si el comando puede ejecutarse en su estado actual
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parámetro pasado desde la vista que debe ser de tipo T
        /// </param>
        /// 
        /// <returns>
        /// true si el comando puede ejecutarse
        /// false si el comando NO puede ejecutarse 
        /// </returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter is not T param) return false;
            return _canExecute == null || _canExecute(param);
        }

        /// <summary>
        /// Ejecuta la acción asociada al comando
        /// </summary>
        /// 
        /// <param name="parameter">
        /// Parámetro pasado desde la vista que debe ser de tipo T
        /// </param>
        public void Execute(object? parameter)
        {
            if (parameter is not T param) return;

            _execute(param);
        }
    }
}