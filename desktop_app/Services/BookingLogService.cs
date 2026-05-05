using System.Net.Http;
using desktop_app.Models;
using Newtonsoft.Json;

namespace desktop_app.Services;

public class BookingLogService
{
    public static async Task<List<BookingLogModel>> GetBookingLogs(string bookingId)
    {
        var response = await CreateResponse($"booking/{bookingId}/audit", new Object(), HttpMethod.Get);
        var content = await response.Content.ReadAsStringAsync();
    
        var logs = JsonConvert.DeserializeObject<List<BookingLogModel>>(content);
    
        return logs ?? new List<BookingLogModel>();
    }

    public static async Task<List<BookingLogModel>> GetDeletedBookingsLogs(string type)
    {
        var response = await CreateResponse($"audit/bookings/{type}", new Object(), HttpMethod.Get);
        var contet = await response.Content.ReadAsStringAsync();
        
        var logs = JsonConvert.DeserializeObject<List<BookingLogModel>>(contet);
        
        return logs ?? new List<BookingLogModel>();
    }

    public static async Task<List<BookingLogModel>> GetLogsByDocumentId(string id)
    {
        var response = await CreateResponse($"audit/{id}", new Object(), HttpMethod.Get);
        var content = await response.Content.ReadAsStringAsync();
        
        var logs = JsonConvert.DeserializeObject<List<BookingLogModel>>(content);
        
        return logs ?? new List<BookingLogModel>();
    }
    
    /// <summary>
    /// Método que crea la solicitud, obtiene la respuesta y verifica los errores
    /// </summary>
    /// 
    /// <param name="endpoint">
    /// String del endpoint al que se debe comunicar
    /// </param>
    /// <param name="payload">
    /// Objeto con los datos que se deben de enviar en el body de la solicitud a la API
    /// </param>
    /// <param name="method">
    /// Método de la solicitud HTTP
    /// </param>
    /// 
    /// <returns>
    /// Devuelve la respuesta del servidor en caso de que no haya error
    /// </returns>
    private static async Task<HttpResponseMessage> CreateResponse(string endpoint, object payload, HttpMethod method)
    {
        string url = $"{ApiService.BaseUrl}{endpoint}";
        
        var request = new HttpRequestMessage(method, url);
        
        var json = JsonConvert.SerializeObject(payload);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await ApiService._httpClient.SendAsync(request);

        await HandleError(response);
        
        return response;
    }

    /// <summary>
    /// Manejador de errores en la comunicación con el servidor
    /// Recibe una respuesta y verifica si tiene errores
    /// En caso de haberlos los maneja
    /// </summary>
    /// 
    /// <param name="response">
    /// Recibe la respuesta de la API 
    /// </param>
    /// 
    /// <returns>
    /// Indica como completada la tarea en caso de que no haya error en la respuesta
    /// </returns>
    /// 
    /// <exception cref="Exception">
    /// Lanza excepciones con los errores personalizados que provienen de la API en caso de error
    /// </exception>
    private static Task HandleError(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("Error en la API de booking logs: " + error);
            var value = JsonConvert.DeserializeObject<Dictionary<string, string>>(error);
            if (value != null && value.ContainsKey("error"))
            {
                var errors = value["error"];
                string errString = String.Join("\n", errors.Split(", "));
                throw new Exception(errString);
            }
            throw new Exception(error);
        }
        return Task.CompletedTask;
    }
}