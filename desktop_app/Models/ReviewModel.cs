using System.Text.Json.Serialization;

namespace desktop_app.Models
{
    /// <summary>
    /// Representa una reseña de habitación.
    /// </summary>
    public class ReviewModel
    {
        /// <summary>Identificador único de la reseña.</summary>
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        /// <summary>ID del usuario que hizo la reseña.</summary>
        [JsonPropertyName("user")]
        public object? User { get; set; }

        /// <summary>ID de la habitación reseñada.</summary>
        [JsonPropertyName("room")]
        public object? Room { get; set; }

        /// <summary>ID de la reserva asociada.</summary>
        [JsonPropertyName("booking")]
        public object? Booking { get; set; }

        /// <summary>Valoración (1-5).</summary>
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        /// <summary>Descripción de la reseña.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        /// <summary>Fecha de creación.</summary>
        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>Nombre del usuario (si viene populado).</summary>
        public string UserName 
        {
            get
            {
                if (User is System.Text.Json.JsonElement je)
                {
                    string first = je.TryGetProperty("firstName", out var f) ? f.GetString() ?? "" : "";
                    string last = je.TryGetProperty("lastName", out var l) ? l.GetString() ?? "" : "";
                    string full = $"{first} {last}".Trim();
                    return string.IsNullOrEmpty(full) ? "Usuario" : full;
                }
                return "Usuario";
            }
        }
    }
}
