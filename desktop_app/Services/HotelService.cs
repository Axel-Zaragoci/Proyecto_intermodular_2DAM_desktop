using desktop_app.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace desktop_app.Services;

public class HotelService
{
    public async Task<HotelModel> GetHotelAsync()
    {
        var response = await CreateResponse("", new Object(), HttpMethod.Get);
        var content = response.Content;
        Console.WriteLine(content.ReadAsStringAsync().Result);
        return await content.ReadFromJsonAsync<HotelModel>();
    }

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