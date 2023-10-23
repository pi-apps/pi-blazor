using System;
using System.Text.Json.Serialization;

namespace PiNetwork.Blazor.Sdk.Dto.Auth;

public sealed class AuthResultDto
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

public sealed class User
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("credentials")]
    public Credentials Credentials { get; set; }
}

public sealed class Credentials
{
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }

    [JsonPropertyName("valid_until")]
    public ValidUntil ValidUntil { get; set; }
}

public sealed class ValidUntil
{
    [JsonPropertyName("timestamp")]
    public Int64 TimeStamp { get; set; }

    [JsonPropertyName("iso8601")]
    public string Iso8601 { get; set; }
}