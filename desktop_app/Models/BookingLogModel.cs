using Newtonsoft.Json;

namespace desktop_app.Models;

public class BookingLogModel
{
    [JsonProperty ("collections")]
    public string Collections { get; set; }
    
    [JsonProperty("action")]
    public string Action { get; set; }
    
    [JsonProperty ("documentId")]
    public string DocumentId { get; set; }
    
    [JsonProperty ("userId")]
    public string UserId { get; set; }
    
    [JsonProperty ("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty ("oldBooking")]
    public BookingModel OldBooking { get; set; }
    
    [JsonProperty ("newBooking")]
    public BookingModel NewBooking { get; set; }
}