using System.Threading.Tasks;

namespace NuciSearch.Services
{
    /// <summary>
    /// Resolves an IP address to a country code.
    /// </summary>
    public interface IGeolocationService
    {
        /// <summary>
        /// Resolves the country code for an IP address.
        /// </summary>
        /// <param name="ipAddress">The client IP address.</param>
        /// <returns>The resolved country code.</returns>
        Task<string> GetCountryCodeAsync(string ipAddress);
    }
}
