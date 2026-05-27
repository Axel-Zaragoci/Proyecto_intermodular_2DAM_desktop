using System.IO;
using System.Text;
using desktop_app.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
namespace desktop_app.Services;


public class PaymentsExporter
{
    /// <summary>
    /// Exporta una lista de pagos en csv
    /// Utiliza el paquete CSVHelper para facilitar el proceso
    /// </summary>
    /// 
    /// <param name="payments">
    /// Lista de los pagos a exportar
    /// </param>
    /// <param name="filePath">
    /// Ruta donde guardar el CSV con los pagos
    /// </param>
    public static void ExportToCsv(List<BasePaymentModel> payments, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            Encoding = Encoding.UTF8
        });

        csv.WriteField("Id");
        csv.WriteField("BookingId");
        csv.WriteField("ClientId");
        csv.WriteField("PaymentMethod");
        csv.WriteField("PaymentType");
        csv.WriteField("Status");
        csv.WriteField("PaymentDate");
        csv.WriteField("PricePaid");
        csv.WriteField("Receiver");
        csv.WriteField("StripeSessionId");
        csv.WriteField("StripeSessionIntentId");
        csv.WriteField("RefundReason");
        csv.WriteField("Details");
        csv.NextRecord();

        foreach (var payment in payments)
        {
            csv.WriteField(payment.Id);
            csv.WriteField(payment.BookingId);
            csv.WriteField(payment.ClientId);
            csv.WriteField(payment.PaymentMethod);
            csv.WriteField(payment.PaymentType);
            csv.WriteField(payment.Status);
            csv.WriteField(payment.PaymentDate.ToString("dd/MM/yyyy HH:mm:ss"));
            csv.WriteField(payment.PricePaid);
            csv.WriteField(payment.ReceivedBy.UserId ?? "");
            csv.WriteField(payment.StripeSessionId ?? "");
            csv.WriteField(payment.StripeSessionIntentId ?? "");
            csv.WriteField(payment.RefundReason ?? "");
            
            string details = payment.fullData.Replace("\n", "|").Replace("\r", "").Trim();
            csv.WriteField(details);
            
            csv.NextRecord();
        }
    }
}