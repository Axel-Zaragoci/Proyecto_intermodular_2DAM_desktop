using System.Net.Http;
using System.Net.Http.Headers;

namespace desktop_app.Services
{
    /// <summary>
    /// Handler HTTP encargado de agregar automáticamente el encabezado de autorización a las solicitudes salientes.
    /// </summary>
    /// <remarks>
    /// Este handler obtiene el token de acceso almacenado en <see cref="TokenStore.AccessToken"/> y lo mete en un encabezado Authorization con esquema Bearer, que es como esta configurada la verificacion en la API.
    /// </remarks>
    public class AuthHeaderHandler : DelegatingHandler
    {
        /// <summary>
        /// Intercepta la solicitud HTTP antes de ser enviada.
        /// </summary>
        /// <param name="request">
        /// Solicitud HTTP que será enviada al servidor.
        /// </param>
        /// <param name="ct">
        /// Token de cancelación para abortar la operación si es necesario.
        /// </param>
        /// <returns>
        /// Una tarea que representa la respuesta HTTP del servidor.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = TokenStore.AccessToken;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, ct);
        }
    }
}