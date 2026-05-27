using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using desktop_app.Models;
using Newtonsoft.Json;

namespace desktop_app.Services;

public class InvoiceService
{
    /// <summary>
    /// Solicita la factura a la API
    /// </summary>
    /// 
    /// <param name="booking">
    /// Objeto de reserva de la que se solicita la facutra
    /// </param>
    /// 
    /// <returns>
    /// Array de bytes correspondientes al archivo pdf de la factura
    /// </returns>
    /// <exception cref="Exception"></exception>
    public static async Task<byte[]> DownloadPdfAsync(BookingModel booking)
    {
        var response = await CreateResponse($"{booking.Id}/invoice", new Object(), HttpMethod.Get);

        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (contentType != "application/pdf")
        {
            throw new Exception($"Archivo PDF inválido. Tipo: {contentType}");
        }
        
        byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
        return pdfBytes;
    }

    /// <summary>
    /// Notifica a la API que envíe un email con la factura 
    /// </summary>
    /// 
    /// <param name="booking">
    /// Objeto de la reserva cuya factura debe ser enviada
    /// </param>
    public static async Task SendPdfAsync(BookingModel booking)
    {
        var response = await CreateResponse($"{booking.Id}/sendInvoice", new Object(), HttpMethod.Get);
        
        MessageBox.Show("Se ha enviado correctamente la factura", "Envío correcto", MessageBoxButton.OK, MessageBoxImage.None);
    }
    
    /// <summary>
    /// Método que crea la solicitud, obtiene la respuesta y verifica los errores
    /// </summary>
    /// 
    /// <param name="endpoint">
    /// String del endpoint al que se debe comunicar
    /// Como las facturas se relacionan con las reservas, la url empieza automática con el router de los mismos
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
        string url = $"{ApiService.BaseUrl}booking/{endpoint}";

        var request = new HttpRequestMessage(method, url);

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
    private static Task HandleError (HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("Error en la API de booking: " + error);
            var value = JsonConvert.DeserializeObject<Dictionary<string, string>>(error);
            if (value != null)
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