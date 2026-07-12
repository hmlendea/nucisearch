using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NuciLog.Core;
using NuciSearch.Logging;

namespace NuciSearch.Services
{
    public sealed class GeolocationService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger logger) : IGeolocationService
    {
        public async Task<string> GetCountryCodeAsync(string ipAddress)
        {
            if (IsPrivateOrLoopback(ipAddress))
            {
                return "RO";
            }

            string? cachedCode = cache.Get<string>(ipAddress);

            if (cachedCode is not null)
            {
                return cachedCode;
            }

            try
            {
                HttpClient client = httpClientFactory.CreateClient("Geolocation");
                IpWhoIsResponse? response = await client.GetFromJsonAsync<IpWhoIsResponse>(
                    $"https://ipwho.is/{Uri.EscapeDataString(ipAddress)}?fields=country_code");

                string countryCode = "GB";

                if (response is not null && !string.IsNullOrEmpty(response.CountryCode))
                {
                    countryCode = response.CountryCode;
                }

                cache.Set(ipAddress, countryCode, TimeSpan.FromHours(24));

                return countryCode;
            }
            catch (Exception exception)
            {
                logger.Error(NuciSearchOperation.GetCountryCode, OperationStatus.Failure, exception,
                    [new(NuciSearchLogInfoKey.IpAddress, ipAddress)]);

                return "GB";
            }
        }

        private static bool IsPrivateOrLoopback(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return true;
            }

            if (ipAddress.Equals("::1") || ipAddress.Equals("127.0.0.1"))
            {
                return true;
            }

            if (ipAddress.StartsWith("192.168.", StringComparison.Ordinal))
            {
                return true;
            }

            if (ipAddress.StartsWith("10.", StringComparison.Ordinal))
            {
                return true;
            }

            if (ipAddress.StartsWith("172.", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}
