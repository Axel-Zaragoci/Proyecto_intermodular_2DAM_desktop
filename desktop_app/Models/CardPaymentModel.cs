using Newtonsoft.Json;

namespace desktop_app.Models;

public class CardPaymentModel : BasePaymentModel
{
    [JsonProperty ("type")]
    public string Type { get; set; }
    
    [JsonProperty ("lastDigits")]
    public int LastDigits { get; set; }
    
    [JsonProperty ("holder")]
    public string Holder { get; set; }
}