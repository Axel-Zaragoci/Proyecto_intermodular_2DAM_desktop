namespace desktop_app.Models;

public class BookingModelDifference
{
    public string PropertyName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }

    public override string ToString()
    {
        return $"{PropertyName}: {OldValue} -> {NewValue}";
    }
    
    /// <summary>
    /// Obtiene las diferencias entre 2 reservas
    /// Utilizado para obtener las diferencias en los logs
    /// </summary>
    /// 
    /// <param name="oldBooking">
    /// Reserva antes de la acción logueada
    /// </param>
    /// <param name="newBooking">
    /// Reserva después de la acción logueada
    /// </param>
    /// 
    /// <returns>
    /// Cadena de texto con las diferencias mostrando el valor anterior y el nuevo
    /// </returns>
    public static List<BookingModelDifference> GetDifferences(BookingModel oldBooking, BookingModel newBooking)
    {
        var differences = new List<BookingModelDifference>();
        
        if (oldBooking.CheckInDate != newBooking.CheckInDate)
            differences.Add(new BookingModelDifference { PropertyName = "Fecha de inicio", OldValue = oldBooking.CheckInDate.ToString(), NewValue = newBooking.CheckInDate.ToString() });
        
        if (oldBooking.CheckOutDate != newBooking.CheckOutDate)
            differences.Add(new BookingModelDifference { PropertyName = "Fecha de fin", OldValue = oldBooking.CheckOutDate.ToString(), NewValue = newBooking.CheckOutDate.ToString() });
        
        if (oldBooking.CreationDate != newBooking.CreationDate)
            differences.Add(new BookingModelDifference { PropertyName = "Fecha de creación de la reserva", OldValue = oldBooking.CreationDate.ToString(), NewValue = newBooking.CreationDate.ToString() });
        
        if (oldBooking.TotalPrice != newBooking.TotalPrice)
            differences.Add(new BookingModelDifference { PropertyName = "Precio total (con IVA y descuento)", OldValue = oldBooking.TotalPrice.ToString(), NewValue = newBooking.TotalPrice.ToString() });
        
        if (oldBooking.PricePerNight != newBooking.PricePerNight)
            differences.Add(new BookingModelDifference { PropertyName = "Precio por noche (sin IVA y con descuento)", OldValue = oldBooking.PricePerNight.ToString(), NewValue = newBooking.PricePerNight.ToString() });
        
        if (oldBooking.Offer != newBooking.Offer)
            differences.Add(new BookingModelDifference { PropertyName = "% de oferta", OldValue = oldBooking.Offer.ToString(), NewValue = newBooking.Offer.ToString() });
        
        if (oldBooking.Status != newBooking.Status)
            differences.Add(new BookingModelDifference { PropertyName = "Estado", OldValue = oldBooking.Status, NewValue = newBooking.Status });
        
        if (oldBooking.Guests != newBooking.Guests)
            differences.Add(new BookingModelDifference { PropertyName = "Huéspedes", OldValue = oldBooking.Guests.ToString(), NewValue = newBooking.Guests.ToString() });
        
        if (oldBooking.TotalNights != newBooking.TotalNights)
            differences.Add(new BookingModelDifference { PropertyName = "Total de noches", OldValue = oldBooking.TotalNights.ToString(), NewValue = newBooking.TotalNights.ToString() });
        
        if (oldBooking.InvoiceId != newBooking.InvoiceId)
            differences.Add(new BookingModelDifference { PropertyName = "Factura", OldValue = oldBooking.InvoiceId, NewValue = newBooking.InvoiceId });
        
        if (oldBooking.ReminderSent24H != newBooking.ReminderSent24H)
            differences.Add(new BookingModelDifference { PropertyName = "Recordatorio 24H previas enviado", OldValue = oldBooking.ReminderSent24H ? "Si" : "No", NewValue = newBooking.ReminderSent24H ? "Si" : "No" });
        
        if (oldBooking.ReminderSent48H != newBooking.ReminderSent48H)
            differences.Add(new BookingModelDifference { PropertyName = "Recordatorio 48H previas enviado", OldValue = oldBooking.ReminderSent48H ? "Si" : "No", NewValue = newBooking.ReminderSent48H ? "Si" : "No" });
        
        if (oldBooking.PaymentStatus != newBooking.PaymentStatus)
            differences.Add(new BookingModelDifference { PropertyName = "Estado de pago", OldValue = oldBooking.PaymentStatus, NewValue = newBooking.PaymentStatus });
        
        if (oldBooking.TotalPaid != newBooking.TotalPaid)
            differences.Add(new BookingModelDifference { PropertyName = "Total pagado", OldValue = oldBooking.TotalPaid.ToString(), NewValue = newBooking.TotalPaid.ToString() });
        
        if (oldBooking.CreatedVia != newBooking.CreatedVia)
            differences.Add(new BookingModelDifference { PropertyName = "Creado desde", OldValue = oldBooking.CreatedVia, NewValue = newBooking.CreatedVia });

        return differences;
    }
}