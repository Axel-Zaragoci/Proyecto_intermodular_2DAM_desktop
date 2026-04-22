using desktop_app.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace desktop_app.Services;

public class HotelService
{
    public static async Task<HotelModel> GetHotelAsync()
    {
        return (
            await (
                await CreateResponse("", new Object(), HttpMethod.Get)).Content.ReadFromJsonAsync<HotelModel>()
        ) ?? new HotelModel();
    }

    public static async Task<HotelModel?> UpdateHotelAsync(HotelModel hotel)
    {
        var payload = new
        {
            name = hotel.name,
            cif = hotel.cif,
            address = hotel.address,
            city = hotel.city,
            country = hotel.country,
            email = hotel.email,
            phone = hotel.phone
        };

        var response = await CreateResponse("", payload, HttpMethod.Patch);
        
        HotelModel updatedHotel = await response.Content.ReadFromJsonAsync<HotelModel>();
        return updatedHotel;
    }
    
    private static async Task<HttpResponseMessage> CreateResponse(string endpoint, object payload, HttpMethod method)
    {
        string url = $"{ApiService.BaseUrl}booking/{endpoint}";
            
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(payload)
        };

        var response = await ApiService._httpClient.SendAsync(request);

        await HandleError(response);
            
        return response;
    }
    
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