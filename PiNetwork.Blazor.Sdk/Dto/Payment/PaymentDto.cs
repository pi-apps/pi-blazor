using System.Text.Json.Serialization;

namespace PiNetwork.Blazor.Sdk.Dto.Payment;

public sealed class PaymentDto
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }

    [JsonPropertyName("user_uid")]
    public string UserUid { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("memo")]
    public string Memo { get; set; }

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

    [JsonPropertyName("from_address")]
    public string FromAddress { get; set; }

    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; }

    [JsonPropertyName("direction")]
    public string Direction { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("network")]
    public string Network { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("transaction")]
    public Transaction Transaction { get; set; }
}

public sealed class Metadata
{
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }
}

public sealed class Status
{
    [JsonPropertyName("developer_approved")]
    public bool DeveloperApproved { get; set; }

    [JsonPropertyName("transaction_verified")]
    public bool TransactionVerified { get; set; }

    [JsonPropertyName("developer_completed")]
    public bool DeveloperCompleted { get; set; }

    [JsonPropertyName("cancelled")]
    public bool Cancelled { get; set; }

    [JsonPropertyName("user_cancelled")]
    public bool UserCancelled { get; set; }
}

public sealed class Transaction
{
    [JsonPropertyName("txid")]
    public string Txid { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("_link")]
    public string Link { get; set; }
}