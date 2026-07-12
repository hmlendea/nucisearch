using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using NuciSearch.Services;

namespace NuciSearch.Localisation
{
    public sealed class IpCultureProvider(IGeolocationService geolocationService) : IRequestCultureProvider
    {
        public async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            string ipAddress = string.Empty;

            if (httpContext.Connection.RemoteIpAddress is not null)
            {
                ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
            }

            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
            {
                string firstIp = forwardedFor.ToString().Split(',')[0].Trim();

                if (!string.IsNullOrEmpty(firstIp))
                {
                    ipAddress = firstIp;
                }
            }

            string countryCode = await geolocationService.GetCountryCodeAsync(ipAddress);
            string culture = "en-GB";

            if (countryCode.Equals("RO"))
            {
                culture = "ro-RO";
            }

            return new ProviderCultureResult(culture);
        }
    }
}
