using Newtonsoft.Json;

namespace desktop_app.Models;

public class TransferPaymentModel : BasePaymentModel
{
    [JsonProperty ("reference")]
    public string Reference { get; set; }
    
    [JsonProperty ("date")]
    public DateTime Date { get; set; }
    
    [JsonProperty ("holder")]
    public string Holder { get; set; }
    
    [JsonProperty ("originAccount")]
    public string OriginAccount { get; set; }
}