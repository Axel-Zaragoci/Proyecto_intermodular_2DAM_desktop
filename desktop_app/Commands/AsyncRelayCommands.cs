using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace desktop_app.Commands
{
    /// <summary>
    /// Implementación de ICommand para operaciones asíncronas (llamadas a API, I/O).
    /// Incluye protección anti-doble-click.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool>? _canExecute;
        private bool _isRunning;

        /// <summary>
        /// Crea un nuevo comando asíncrono.
        /// </summary>
        /// <param name="executeAsync">Método async a ejecutar.</param>
        /// <param name="canExecute">Condición opcional para habilitar el comando.</param>
        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento que notifica cuando cambia la capacidad de ejecución.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determina si el comando puede ejecutarse.
        /// </summary>
        /// <param name="parameter">Parámetro desde la vista.</param>
        /// <returns>True si puede ejecutarse, false si está corriendo o la condición falla.</returns>
        public bool CanExecute(object? parameter)
        {
        /// Si está corriendo, deshabilitamos el botón para evitar doble click
            if (_isRunning) return false;
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// Ejecuta la operación asíncrona.
        /// </summary>
        /// <param name="parameter">Parámetro desde la vista.</param>
        public async void Execute(object? parameter)
        {
            _isRunning = true;
            RaiseCanExecuteChanged();

            try
            {
                await _executeAsync();
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Notifica a la UI que debe reevaluar CanExecute.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Versión genérica de AsyncRelayCommand que acepta un parámetro tipado.
    /// </summary>
    /// <typeparam name="T">Tipo del parámetro.</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _executeAsync;
        private readonly Func<T, bool>? _canExecute;
        private bool _isRunning;

        /// <summary>
        /// Crea un nuevo comando asíncrono con parámetro tipado.
        /// </summary>
        /// <param name="executeAsync">Método async que recibe el parámetro.</param>
        /// <param name="canExecute">Condición opcional para habilitar el comando.</param>
        public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento que notifica cuando cambia la capacidad de ejecución.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determina si el comando puede ejecutarse con el parámetro dado.
        /// </summary>
        /// <param name="parameter">Parámetro que se intentará castear a T.</param>
        /// <returns>True si puede ejecutarse, false si el tipo no coincide o la condición falla.</returns>
        public bool CanExecute(object? parameter)
        {
            if (_isRunning) return false;
            if (parameter is not T param) return false;
            return _canExecute?.Invoke(param) ?? true;
        }

        /// <summary>
        /// Ejecuta la operación asíncrona con el parámetro tipado.
        /// </summary>
        /// <param name="parameter">Parámetro que se casteará a T.</param>
        public async void Execute(object? parameter)
        {
            if (parameter is not T param) return;

            _isRunning = true;
            RaiseCanExecuteChanged();

            try
            {
                await _executeAsync(param);
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Notifica a la UI que debe reevaluar CanExecute.
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
