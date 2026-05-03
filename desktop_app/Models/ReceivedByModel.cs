using Newtonsoft.Json;

namespace desktop_app.Models;

public class ReceivedByModel
{
    [JsonProperty ("type")]
    public string Type { get; set; }
    
    [JsonProperty ("id")]
    public string UserId { get; set; }
}