using System.Net.Http;
using desktop_app.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace desktop_app.Services;

public class PaymentService
{
    /// <summary>
    /// Método para registrar un pago en efectivo
    /// </summary>
    /// 
    /// <param name="data">
    /// Datos del pago
    /// </param>
    /// <param name="bookingId">
    /// ID de la reserva que se paga
    /// </param>
    /// 
    /// <returns>
    /// Objeto del pago creado en la base de datos
    /// </returns>
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
    
    /// <summary>
    /// Método para registrar un pago por transferencia
    /// </summary>
    /// 
    /// <param name="data">
    /// Datos del pago
    /// </param>
    /// <param name="bookingId">
    /// ID de la reserva que se paga
    /// </param>
    /// 
    /// <returns>
    /// Objeto del pago creado en la base de datos
    /// </returns>
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

    /// <summary>
    /// Método para obtener los pagos de una reserva
    /// </summary>
    /// 
    /// <param name="bookingId">
    /// ID de la reserva
    /// </param>
    /// 
    /// <returns>
    /// Lista de todos los pagos asociados a la reserva
    /// </returns>
    public static async Task<List<BasePaymentModel>> GetBookingPaymentHistory(string bookingId)
    {
        var endpoint = $"booking/{bookingId}/payments";
        var response = await CreateResponse(endpoint, new Object { }, HttpMethod.Get);
        var json = await response.Content.ReadAsStringAsync();
        var payments = ProcessPaymentListJson(json);
        return payments;
    }

    /// <summary>
    /// Método para registrar un pago en efectivo
    /// </summary>
    /// 
    /// <param name="reason">
    /// Razón de la devolución
    /// </param>
    /// <param name="bookingId">
    /// ID de la reserva que se paga
    /// </param>
    /// 
    /// <returns>
    /// Valor booleano:
    ///     - true -> Todos los pagos devueltos
    ///     - false -> Uno o más pagos no han sido devueltos
    /// </returns>
    public static async Task<Boolean> RefundBookingPayments(string reason, string bookingId)
    {
        var endpoint = $"payments/{bookingId}/refund";
        var response = await CreateResponse(endpoint, new { reason = reason }, HttpMethod.Patch);
        var json = await response.Content.ReadAsStringAsync();
        var container = JsonConvert.DeserializeObject<JObject>(json);
        var allCompleted = container["success"]?.ToObject<Boolean>();

        return allCompleted == true;
    }

    /// <summary>
    /// Método para obtener todos los pagos
    /// </summary>
    /// 
    /// <returns>
    /// Lista con todos los pagos
    /// </returns>
    public static async Task<List<BasePaymentModel>> GetPayments()
    {
        var endpoint = $"payments/";
        var response = await CreateResponse(endpoint, new {}, HttpMethod.Get);
        var json = await response.Content.ReadAsStringAsync();
        var payments = ProcessPaymentListJson(json);
        Console.WriteLine(payments.Count);
        return payments;
    }

    /// <summary>
    /// Procesa una lista de objetos en json para obtener una lista de pagos en su modelo correcto
    /// </summary>
    /// 
    /// <param name="json">
    /// Cadena JSON con la lista de pagos
    /// </param>
    /// 
    /// <returns>
    /// Lista de pagos
    /// </returns>
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
    
    /// <summary>
    /// Método que crea la solicitud, obtiene la respuesta y verifica los errores
    /// </summary>
    /// 
    /// <param name="endpoint">
    /// String del endpoint al que se debe comunicar
    /// Como este es el manejador de reservas ya empieza la URL con el acceso al router de reservas de la API
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
    /// Método para registrar un pago en efectivo
    /// </summary>
    /// 
    /// <param name="data">
    /// Datos del pago
    /// </param>
    /// <param name="bookingId">
    /// ID de la reserva que se paga
    /// </param>
    /// 
    /// <returns>
    /// Objeto del pago creado en la base de datos
    /// </returns>
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