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
    public string PaymentDate { get; set; }
    
    [JsonProperty ("pricePaid")]
    public string PricePaid { get; set; }
    
    [JsonProperty ("recibedBy")]
    public ReceivedByModel RecibedBy { get; set; }
    
    [JsonProperty ("stripeSessionId")]
    public string? StripeSessionId { get; set; }
    
    [JsonProperty ("stripeSessionIntentId")]
    public string? StripeSessionIntentId { get; set; }
    
    [JsonProperty ("refundReason")]
    public string? RefundReason { get; set; }
}