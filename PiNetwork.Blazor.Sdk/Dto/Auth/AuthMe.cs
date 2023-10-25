using System.Text.Json.Serialization;

namespace PiNetwork.Blazor.Sdk.Dto.Auth;

public sealed class AuthMeDto
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("credentials")]
    public Credentials Credentials { get; set; }
}