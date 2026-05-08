using System.Windows.Media.TextFormatting;
using Newtonsoft.Json;

namespace desktop_app.Models;

public class BasePaymentModel
{
    [JsonProperty ("_id")]
    public string Id { get; set; }
    
    [JsonProperty ("bookingId")]
    public string BookingId { get; set; }
    
    [JsonProperty ("clientId")]
    public string ClientId { get; set; }
    
    [JsonProperty ("paymentMethod")]
    public string PaymentMethod { get; set; }
    
    [JsonProperty ("paymentType")]
    public string PaymentType { get; set; }
    
    [JsonProperty ("status")]
    public string Status { get; set; }
    
    [JsonProperty ("paymentDate")]
    public DateTime PaymentDate { get; set; }
    
    [JsonProperty ("pricePaid")]
    public decimal PricePaid { get; set; }
    
    [JsonProperty ("receivedBy")]
    public ReceivedByModel ReceivedBy { get; set; }
    
    [JsonProperty ("stripeSessionId")]
    public string? StripeSessionId { get; set; }
    
    [JsonProperty ("stripeSessionIntentId")]
    public string? StripeSessionIntentId { get; set; }
    
    [JsonProperty ("refundReason")]
    public string? RefundReason { get; set; }

    [JsonIgnore] 
    public string fullData => this.getPaymentData();
    
    [JsonIgnore]
    public string ClientName { get; set; }
    
    [JsonIgnore]
    public string ReceiverName { get; set; }

    public virtual string getPaymentData()
    {
        return "";
    }

    public override string ToString()
    {
        return $"Pago: {Id}\nID de la reserva: {BookingId}\nID del cliente: {ClientId}\nMétodo de pago: {PaymentMethod}\nTipo de pago: {PaymentType}\nEstado del pago: {Status}\nFecha de pago: {PaymentDate.ToString("dd/MM/yyyy hh:mm:ss")}\nPrecio pagado: {PricePaid}\nID del recibidor: {ReceivedBy.UserId}\n{getPaymentData()}";
    }
}