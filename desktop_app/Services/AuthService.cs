using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace desktop_app.Services
{
    /// <summary>
    /// Servicio de autenticación encargado de gestionar el inicio de sesión del usuario.
    /// </summary>
    public static class AuthService
    {
        /// <summary>
        /// Realiza la llamada al endpoint de autenticación para iniciar sesión.
        /// </summary>
        /// <remarks>
        /// Envía las credenciales del usuario (email y contraseña) al backend.
        /// Si la autenticación es correcta:
        /// <list type="bullet">
        /// <item>Obtiene el token de acceso.</item>
        /// <item>Guarda el token en <see cref="TokenStore.AccessToken"/>.</item>
        /// <item>Devuelve el rol del usuario autenticado.</item>
        /// </list>
        /// En caso de error, lanza una excepción descriptiva.
        /// </remarks>
        /// <param name="email">
        /// Dirección de correo electrónico del usuario.
        /// </param>
        /// <param name="password">
        /// Contraseña del usuario.
        /// </param>
        /// <returns>
        /// Una tarea asíncrona que devuelve el rol del usuario autenticado.
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await ApiService._httpClient.PostAsync("/auth/login", content);

            if (!resp.IsSuccessStatusCode) throw new Exception("Login fallido.");

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            if (!doc.RootElement.TryGetProperty("token", out var tokenEl)) throw new Exception("Campo token vacío.");

            if (!doc.RootElement.TryGetProperty("rol", out var rolEl)) throw new Exception("Campo rol vacío.");

            TokenStore.AccessToken = tokenEl.GetString();

            return rolEl.GetString();
        }
    }
}
