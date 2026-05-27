using desktop_app.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace desktop_app.Services;

public class HotelService
{
    /// <summary>
    /// Obtiene los datos del hotel
    /// </summary>
    /// 
    /// <returns>
    /// Promesa de los datos del hotel
    /// </returns>
    public async Task<HotelModel> GetHotelAsync()
    {
        var response = await CreateResponse("", new Object(), HttpMethod.Get);
        var content = response.Content;
        Console.WriteLine(content.ReadAsStringAsync().Result);
        return await content.ReadFromJsonAsync<HotelModel>();
    }

    /// <summary>
    /// Modifica los datos del hotel
    /// </summary>
    /// 
    /// <param name="hotel">
    /// Objeto del hotel con los nuevos datos
    /// </param>
    /// 
    /// <returns>
    /// Objeto del hotel con los datos actualizados o null
    /// </returns>
    public async Task<HotelModel?> UpdateHotelAsync(HotelModel hotel)
    {
        var payload = new
        {
            name = hotel.name,
            cif = hotel.cif,
            address = hotel.address,
            postalCode = hotel.postalCode,
            city = hotel.city,
            country = hotel.country,
            email = hotel.email,
            phone = hotel.phone
        };

        var response = await CreateResponse("", payload, HttpMethod.Patch);
        
        HotelModel updatedHotel = await response.Content.ReadFromJsonAsync<HotelModel>();
        return updatedHotel;
    }
    
    /// <summary>
    /// Método que crea la solicitud, obtiene la respuesta y verifica los errores
    /// </summary>
    /// 
    /// <param name="endpoint">
    /// String del endpoint al que se debe comunicar
    /// Como este es el manejador de hotel ya empieza la URL con el acceso al router de hotel de la API
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
        string url = $"{ApiService.BaseUrl}hotel/{endpoint}";
            
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(payload)
        };

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
            Console.WriteLine("Error en la API de hotel: " + error);
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