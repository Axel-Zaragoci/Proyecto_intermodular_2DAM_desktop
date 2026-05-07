using Newtonsoft.Json;

namespace desktop_app.Models;

public class CashPaymentModel : BasePaymentModel
{
    [JsonProperty ("amountReceived")]
    public decimal AmountReceived { get; set; }
    
    [JsonProperty ("changeGiven")]
    public decimal ChangeGiven { get; set; }

    public override string getPaymentData()
    {
        return $"Cantidad recibida: {AmountReceived}\nCambio entregado: {ChangeGiven}";
    }
}