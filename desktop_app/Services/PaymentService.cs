using System.Net.Http;
using desktop_app.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace desktop_app.Services;

public class PaymentService
{
    public static async Task<CashPaymentModel> CreatePayment(CashPaymentModel data, string bookingId)
    {
        var endpoint = $"booking/{bookingId}/payments";

        object payload = new
        {
            paymentMethod = "Efectivo",
            pricePaid = data.PricePaid,
            amountReceived = data.AmountReceived
        };
        
        var response = await CreateResponse(endpoint, payload, HttpMethod.Post);
        var content = await response.Content.ReadAsStringAsync();
        var createdPayment = JsonConvert.DeserializeObject<CashPaymentModel>(content);
        
        return createdPayment;
    }
    
    public static async Task<TransferPaymentModel> CreatePayment(TransferPaymentModel data, string bookingId)
    {
        var endpoint = $"booking/{bookingId}/payments";

        object payload = new
        {
            paymentMethod = "Transferencia",
            pricePaid = data.PricePaid,
            reference = data.Reference,
            transferDate = data.TransferDate,
            holder = data.Holder,
            originAccount = data.OriginAccount
        };
        
        var response = await CreateResponse(endpoint, payload, HttpMethod.Post);
        var content = await response.Content.ReadAsStringAsync();
        var createdPayment = JsonConvert.DeserializeObject<TransferPaymentModel>(content);
        
        return createdPayment;
    }

    public static async Task<List<BasePaymentModel>> GetBookingPaymentHistory(string bookingId)
    {
        var endpoint = $"booking/{bookingId}/payments";
        var response = await CreateResponse(endpoint, new Object { }, HttpMethod.Get);
        var json = await response.Content.ReadAsStringAsync();
        var payments = ProcessPaymentListJson(json);
        return payments;
    }

    public static async Task<Boolean> RefundBookingPayments(string reason, string bookingId)
    {
        var endpoint = $"payments/{bookingId}/refund";
        var response = await CreateResponse(endpoint, new { reason = reason }, HttpMethod.Get);
        var json = await response.Content.ReadAsStringAsync();
        var container = JsonConvert.DeserializeObject<JObject>(json);
        var allCompleted = container["allSuccess"]?.ToObject<Boolean>();

        return allCompleted != null;
    }

    public static async Task<List<BasePaymentModel>> GetPayments()
    {
        var endpoint = $"payments/";
        var response = await CreateResponse(endpoint, new {}, HttpMethod.Get);
        var json = await response.Content.ReadAsStringAsync();
        var payments = ProcessPaymentListJson(json);
        Console.WriteLine(payments.Count);
        return payments;
    }

    private static List<BasePaymentModel> ProcessPaymentListJson(string json)
    {
        var rawPayments = JsonConvert.DeserializeObject<List<JObject>>(json);
        var payments = new List<BasePaymentModel>();

        foreach (var raw in rawPayments)
        {
            var method = raw["paymentMethod"]?.ToString();

            BasePaymentModel payment = method switch
            {
                "Tarjeta" => raw.ToObject<CardPaymentModel>(),
                "Efectivo" => raw.ToObject<CashPaymentModel>(),
                "Transferencia" => raw.ToObject<TransferPaymentModel>(),
                _ => raw.ToObject<BasePaymentModel>()
            };
            
            payments.Add(payment);
        }
        return payments;
    }
    
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
    
    private static Task HandleError(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("Error en la API de pagos: " + error);
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