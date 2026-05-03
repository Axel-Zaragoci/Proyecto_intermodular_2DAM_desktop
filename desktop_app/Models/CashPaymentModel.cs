using Newtonsoft.Json;

namespace desktop_app.Models;

public class CashPaymentModel : BasePaymentModel
{
    [JsonProperty ("amountReceived")]
    public decimal AmountReceived { get; set; }
    
    [JsonProperty ("changeGiven")]
    public decimal ChangeGiven { get; set; }
}