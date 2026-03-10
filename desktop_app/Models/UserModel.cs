using System.Text.Json.Serialization;

namespace desktop_app.Models
{
    public class UserModel
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("dni")]
        public string Dni { get; set; }

        [JsonPropertyName("phoneNumber")]
        public long? PhoneNumber { get; set; }

        [JsonIgnore]
        public string PhoneDisplay => string.IsNullOrWhiteSpace(PhoneNumber + "") ? "Desconocido" : PhoneNumber + "";

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("cityName")]
        public string CityName { get; set; }

        [JsonPropertyName("imageRoute")]
        public string ImageRoute { get; set; }

        [JsonPropertyName("rol")]
        public string Rol { get; set; }

        [JsonIgnore]
        public string VipText => VipStatus ? "Sí" : "No";

        [JsonPropertyName("vipStatus")]
        public bool VipStatus { get; set; }
        
        [JsonIgnore]
        public string FullNameWithDni => $"{FirstName} {LastName} - {Dni}";
    }
}