using Newtonsoft.Json;

namespace desktop_app.Models;

public class TransferPaymentModel : BasePaymentModel
{
    [JsonProperty ("reference")]
    public string Reference { get; set; }
    
    [JsonProperty ("date")]
    public DateTime TransferDate { get; set; }
    
    [JsonProperty ("holder")]
    public string Holder { get; set; }
    
    [JsonProperty ("originAccount")]
    public string OriginAccount { get; set; }

    public override string getPaymentData()
    {
        return $"Identificador de tranferencia: {Reference}\nFecha de transferencia: {TransferDate.ToString("dd-MM-yyy")}\nPropietario de la cuenta: {Holder}\nNúmero de cuenta: {OriginAccount}";
    }
}