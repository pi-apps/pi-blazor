using System.Text.Json.Serialization;

namespace PiNetwork.Blazor.Sdk.Dto.Auth
{
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
    }
}