using desktop_app.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace desktop_app.Services
{
    /// <summary>
    /// Servicio para operaciones de reseñas contra la API REST.
    /// </summary>
    public static class ReviewService
    {
        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        /// <summary>
        /// Obtiene todas las reseñas de una habitación.
        /// </summary>
        /// <param name="roomId">ID de la habitación.</param>
        /// <returns>Lista de reseñas, o lista vacía si hay error.</returns>
        public static async Task<List<ReviewModel>> GetReviewsByRoomAsync(string roomId)
        {
            try
            {
                var url = ApiService.BaseUrl + $"review/room/{Uri.EscapeDataString(roomId)}";
                var resp = await ApiService._httpClient.GetAsync(url);

                if (!resp.IsSuccessStatusCode) return new List<ReviewModel>();

                var json = await resp.Content.ReadAsStringAsync();
                var reviews = JsonSerializer.Deserialize<List<ReviewModel>>(json, _jsonOptions);
                return reviews ?? new List<ReviewModel>();
            }
            catch
            {
                return new List<ReviewModel>();
            }
        }

        /// <summary>
        /// Crea una nueva reseña.
        /// </summary>
        /// <param name="review">Datos de la reseña.</param>
        /// <returns>La reseña creada, o null si hay error.</returns>
        public static async Task<ReviewModel?> CreateReviewAsync(ReviewModel review)
        {
            try
            {
                var url = ApiService.BaseUrl + "review";
                var payload = new
                {
                    user = review.User,
                    room = review.Room,
                    booking = review.Booking,
                    rating = review.Rating,
                    description = review.Description
                };

                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await ApiService._httpClient.PostAsync(url, content);
                if (!resp.IsSuccessStatusCode) return null;

                var body = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ReviewModel>(body, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina una reseña por su ID.
        /// </summary>
        /// <param name="reviewId">ID de la reseña a eliminar.</param>
        /// <returns>True si se eliminó correctamente.</returns>
        public static async Task<bool> DeleteReviewAsync(string reviewId)
        {
            try
            {
                var url = ApiService.BaseUrl + $"review/{Uri.EscapeDataString(reviewId)}";
                var resp = await ApiService._httpClient.DeleteAsync(url);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
