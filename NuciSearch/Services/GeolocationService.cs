using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NuciLog.Core;
using NuciSearch.Logging;

namespace NuciSearch.Services
{
    public sealed class GeolocationService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger logger) : IGeolocationService
    {
        private static string DefaultCountryCode => "GB";

        private static string LocalCountryCode => "RO";

        public async Task<string> GetCountryCodeAsync(string ipAddress)
        {
            if (IsPrivateOrLoopback(ipAddress))
            {
                return LocalCountryCode;
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

                string countryCode = DefaultCountryCode;

                if (response is not null && !string.IsNullOrEmpty(response.CountryCode))
                {
                    countryCode = response.CountryCode;
                }

                cache.Set(ipAddress, countryCode, TimeSpan.FromHours(24));

                return countryCode;
            }
            catch (Exception exception)
            {
                logger.Error(
                    NuciSearchOperation.GetCountryCode,
                    OperationStatus.Failure,
                    exception,
                    [new(NuciSearchLogInfoKey.IpAddress, ipAddress)]);

                return DefaultCountryCode;
            }
        }

        private static bool IsPrivateOrLoopback(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress? parsedAddress))
            {
                return true;
            }

            if (IPAddress.IsLoopback(parsedAddress))
            {
                return true;
            }

            byte[] addressBytes = parsedAddress.GetAddressBytes();

            if (addressBytes.Length == 4 && addressBytes[0] == 10)
            {
                return true;
            }

            if (addressBytes.Length == 4 && addressBytes[0] == 192 && addressBytes[1] == 168)
            {
                return true;
            }

            if (addressBytes.Length == 4 && addressBytes[0] == 172 &&
                addressBytes[1] >= 16 && addressBytes[1] <= 31)
            {
                return true;
            }

            return false;
        }
    }
}
