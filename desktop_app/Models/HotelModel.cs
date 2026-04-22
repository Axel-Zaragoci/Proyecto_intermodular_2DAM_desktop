using System.Text.Json.Serialization;

namespace desktop_app.Models;

public class HotelModel
{
    [JsonPropertyName("name")]
    public string name { get; set; }
    
    [JsonPropertyName("cif")]
    public string cif { get; set; }
    
    [JsonPropertyName("address")]
    public string address { get; set; }
    
    [JsonPropertyName("city")]
    public string city { get; set; }
    
    [JsonPropertyName("country")]
    public string country { get; set; }
    
    [JsonPropertyName("email")]
    public string email { get; set; }
    
    [JsonPropertyName("phone")]
    public string phone { get; set; }
}