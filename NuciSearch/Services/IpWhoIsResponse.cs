using System.Text.Json.Serialization;

namespace NuciSearch.Services
{
    internal sealed class IpWhoIsResponse
    {
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;
    }
}
