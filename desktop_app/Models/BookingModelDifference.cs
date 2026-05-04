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

    public static List<BookingModelDifference> GetDifferences(BookingModel oldBooking, BookingModel newBooking)
    {
        var differences = new List<BookingModelDifference>();
        
        if (oldBooking.Id != newBooking.Id)
            differences.Add(new BookingModelDifference { PropertyName = "Id", OldValue = oldBooking.Id, NewValue = newBooking.Id });
        
        if (oldBooking.Room != newBooking.Room)
            differences.Add(new BookingModelDifference { PropertyName = "Room", OldValue = oldBooking.Room, NewValue = newBooking.Room });
        
        if (oldBooking.Client != newBooking.Client)
            differences.Add(new BookingModelDifference { PropertyName = "Client", OldValue = oldBooking.Client, NewValue = newBooking.Client });
        
        if (oldBooking.CheckInDate != newBooking.CheckInDate)
            differences.Add(new BookingModelDifference { PropertyName = "CheckInDate", OldValue = oldBooking.CheckInDate.ToString(), NewValue = newBooking.CheckInDate.ToString() });
        
        if (oldBooking.CheckOutDate != newBooking.CheckOutDate)
            differences.Add(new BookingModelDifference { PropertyName = "CheckOutDate", OldValue = oldBooking.CheckOutDate.ToString(), NewValue = newBooking.CheckOutDate.ToString() });
        
        if (oldBooking.PayDate != newBooking.PayDate)
            differences.Add(new BookingModelDifference { PropertyName = "PayDate", OldValue = oldBooking.PayDate.ToString(), NewValue = newBooking.PayDate.ToString() });
        
        if (oldBooking.TotalPrice != newBooking.TotalPrice)
            differences.Add(new BookingModelDifference { PropertyName = "TotalPrice", OldValue = oldBooking.TotalPrice.ToString(), NewValue = newBooking.TotalPrice.ToString() });
        
        if (oldBooking.PricePerNight != newBooking.PricePerNight)
            differences.Add(new BookingModelDifference { PropertyName = "PricePerNight", OldValue = oldBooking.PricePerNight.ToString(), NewValue = newBooking.PricePerNight.ToString() });
        
        if (oldBooking.Offer != newBooking.Offer)
            differences.Add(new BookingModelDifference { PropertyName = "Offer", OldValue = oldBooking.Offer.ToString(), NewValue = newBooking.Offer.ToString() });
        
        if (oldBooking.Status != newBooking.Status)
            differences.Add(new BookingModelDifference { PropertyName = "Status", OldValue = oldBooking.Status, NewValue = newBooking.Status });
        
        if (oldBooking.Guests != newBooking.Guests)
            differences.Add(new BookingModelDifference { PropertyName = "Guests", OldValue = oldBooking.Guests.ToString(), NewValue = newBooking.Guests.ToString() });
        
        if (oldBooking.TotalNights != newBooking.TotalNights)
            differences.Add(new BookingModelDifference { PropertyName = "TotalNights", OldValue = oldBooking.TotalNights.ToString(), NewValue = newBooking.TotalNights.ToString() });
        
        if (oldBooking.InvoiceId != newBooking.InvoiceId)
            differences.Add(new BookingModelDifference { PropertyName = "InvoiceId", OldValue = oldBooking.InvoiceId, NewValue = newBooking.InvoiceId });
        
        if (oldBooking.ReminderSent24H != newBooking.ReminderSent24H)
            differences.Add(new BookingModelDifference { PropertyName = "ReminderSent24H", OldValue = oldBooking.ReminderSent24H.ToString(), NewValue = newBooking.ReminderSent24H.ToString() });
        
        if (oldBooking.ReminderSent48H != newBooking.ReminderSent48H)
            differences.Add(new BookingModelDifference { PropertyName = "ReminderSent48H", OldValue = oldBooking.ReminderSent48H.ToString(), NewValue = newBooking.ReminderSent48H.ToString() });
        
        if (oldBooking.ReminderName24H != newBooking.ReminderName24H)
            differences.Add(new BookingModelDifference { PropertyName = "ReminderName24H", OldValue = oldBooking.ReminderName24H, NewValue = newBooking.ReminderName24H });
        
        if (oldBooking.ReminderName48H != newBooking.ReminderName48H)
            differences.Add(new BookingModelDifference { PropertyName = "ReminderName48H", OldValue = oldBooking.ReminderName48H, NewValue = newBooking.ReminderName48H });
        
        if (oldBooking.PaymentStatus != newBooking.PaymentStatus)
            differences.Add(new BookingModelDifference { PropertyName = "PaymentStatus", OldValue = oldBooking.PaymentStatus, NewValue = newBooking.PaymentStatus });
        
        if (oldBooking.TotalPaid != newBooking.TotalPaid)
            differences.Add(new BookingModelDifference { PropertyName = "TotalPaid", OldValue = oldBooking.TotalPaid.ToString(), NewValue = newBooking.TotalPaid.ToString() });
        
        if (oldBooking.CreatedVia != newBooking.CreatedVia)
            differences.Add(new BookingModelDifference { PropertyName = "CreatedVia", OldValue = oldBooking.CreatedVia, NewValue = newBooking.CreatedVia });
        
        if (oldBooking.RoomNumber != newBooking.RoomNumber)
            differences.Add(new BookingModelDifference { PropertyName = "RoomNumber", OldValue = oldBooking.RoomNumber, NewValue = newBooking.RoomNumber });
        
        if (oldBooking.ClientName != newBooking.ClientName)
            differences.Add(new BookingModelDifference { PropertyName = "ClientName", OldValue = oldBooking.ClientName, NewValue = newBooking.ClientName });
        
        if (oldBooking.ClientDni != newBooking.ClientDni)
            differences.Add(new BookingModelDifference { PropertyName = "ClientDni", OldValue = oldBooking.ClientDni, NewValue = newBooking.ClientDni });
        
        return differences;
    }
}