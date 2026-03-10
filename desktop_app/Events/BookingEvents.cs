namespace desktop_app.Events;

public static class BookingEvents
{
    /// <summary>
    /// Evento para indicar que se ha modificado los bookings
    /// </summary>
    public static event Func<Task>? OnBookingChanged;

    /// <summary>
    /// Función para iniciar el evento de volver a cargar reservas
    /// </summary>
    public static async Task RaiseBookingChanged()
    {
        if (OnBookingChanged != null) await OnBookingChanged.Invoke();
    }
}