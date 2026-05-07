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

    public virtual string getPaymentData()
    {
        return "";
    }
}