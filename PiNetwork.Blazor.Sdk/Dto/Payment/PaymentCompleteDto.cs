using System.Text.Json.Serialization;

namespace PiNetwork.Blazor.Sdk.Dto.Payment
{
    public sealed class PaymentCompleteDto
    {
        [JsonPropertyName("txid")]
        public string Txid { get; set; }
    }
}