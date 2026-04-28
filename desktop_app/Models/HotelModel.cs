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
    
    [JsonPropertyName("postalCode")]
    public string postalCode { get; set; }
    
    [JsonPropertyName("city")]
    public string city { get; set; }
    
    [JsonPropertyName("country")]
    public string country { get; set; }
    
    [JsonPropertyName("email")]
    public string email { get; set; }
    
    [JsonPropertyName("phone")]
    public string phone { get; set; }

    public override string ToString()
    {
        return "Hotel " + this.cif + ": \n" +
               "Nombre: " +  this.name + "\n" +
               "Dirección: " + this.address + "\n" +
               "Código postal: " + this.postalCode + "\n" +
               "Ciudad: " + this.city + "\n" +
               "País: " + this.country + "\n" +
               "Email: " + this.email + "\n" +
               "Teléfono: " + this.phone;
    }
}