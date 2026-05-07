using Newtonsoft.Json;

namespace desktop_app.Models
{
    public class BookingModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; } = "";

        [JsonProperty("room")] 
        public string Room { get; set; }
        
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("checkInDate")]
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        
        [JsonProperty("checkOutDate")]
        public DateTime CheckOutDate { get; set; } = DateTime.Now;

        [JsonProperty("payDate")]
        public DateTime PayDate { get; set; } = DateTime.Now;
        
        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonProperty("pricePerNight")]
        public decimal? PricePerNight { get; set; }

        [JsonProperty("offer")]
        public decimal? Offer { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("guests")]
        public int Guests { get; set; } = 0;

        [JsonProperty("totalNights")]
        public int TotalNights { get; set; }

        [JsonProperty("invoiceId")]
        public string InvoiceId { get; set; } = "";
        
        [JsonProperty("reminder_sent_24h")]
        public bool ReminderSent24H { get; set; } = false;
        
        [JsonProperty("reminder_sent_48h")]
        public bool ReminderSent48H { get; set; } = false;
        
        [JsonProperty("reminder_24h_name")]
        public string ReminderName24H { get; set; } = "";
        
        [JsonProperty("reminder_48h_name")]
        public string ReminderName48H { get; set; } = "";
        
        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; } = "Pendiente";
        
        [JsonProperty("totalPaid")]
        public decimal TotalPaid { get; set; } = 0;
        
        [JsonProperty("createdVia")]
        public string CreatedVia { get; set; } = "Online";
        
        [JsonIgnore]
        public string RoomNumber { get; set; } = "";

        [JsonIgnore]
        public string ClientName { get; set; } = "Por conseguir";

        [JsonIgnore]
        public string ClientDni { get; set; } = "";
        
        [JsonIgnore]
        public bool CanGenerateInvoice => TotalPaid == TotalPrice;
        
        [JsonIgnore]
        public DateTime Hour24H => CheckInDate.AddDays(-1);
        
        [JsonIgnore] 
        public DateTime Hour48H => CheckInDate.AddDays(-2);
        
        public BookingModel Clone()
        {
            return new BookingModel
            {
                Id = Id,
                Room = Room,
                Client = Client,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                PayDate = PayDate,
                TotalPrice = TotalPrice,
                PricePerNight = PricePerNight,
                Offer = Offer,
                Status = Status,
                Guests = Guests,
                TotalNights = TotalNights,
                InvoiceId = InvoiceId,
                ReminderSent24H = ReminderSent24H,
                ReminderSent48H = ReminderSent48H,
                ReminderName24H = ReminderName24H,
                ReminderName48H = ReminderName48H,
                PaymentStatus = PaymentStatus,
                TotalPaid = TotalPaid,
                CreatedVia = CreatedVia
            };
        }

        public override string ToString()
        {
            return "ID: " + this.Id + "\n"
                   + "ID del cliente: " + this.Client + "\n"
                   + "\tDNI del cliente: " + this.ClientDni + "\n"
                   + "\tNombre del cliente: " + this.ClientName + "\n"
                   + "ID de la habitación: " + this.Room + "\n"
                   + "\tNúmero de la habitación: " + this.RoomNumber + "\n"
                   + "Fecha de inicio: " + this.CheckInDate.ToString("d") + "\n"
                   + "Fecha de fin: " + this.CheckOutDate.ToString("d") + "\n"
                   + "Fecha de reserva y pago: " + this.PayDate.ToString("d") + "\n"
                   + "Oferta: " + this.Offer + "%\n"
                   + "Precio total: " + this.TotalPrice + "€\n"
                   + "Precio por noche: " + this.PricePerNight + "€\n"
                   + "Total de noches: " + this.TotalNights + "\n"
                   + "Cantidad de huéspedes: " + this.Guests + "\n"
                   + "ID de factura: " + this.InvoiceId + "\n"
                   + "ReminderSent24H: " + this.ReminderSent24H + "\n"
                   + "ReminderSent48H: " + this.ReminderSent48H + "\n"
                   + "ReminderName24H: " + this.ReminderName24H + "\n"
                   + "ReminderName48H: " + this.ReminderName48H + "\n"
                   + "PaymentStatus: " + this.PaymentStatus + "\n"
                   + "TotalPaid: " + this.TotalPaid + "\n"
                   + "CreatedVia: " + this.CreatedVia + "\n"
                   + "CanGenerateInvoice: " + this.CanGenerateInvoice + "\n"
                   + "Hour24H: " + this.Hour24H + "\n"
                   + "Hour48H: " + this.Hour48H;
        }
    }
}