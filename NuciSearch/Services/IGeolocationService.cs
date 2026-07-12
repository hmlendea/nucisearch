using System.Threading.Tasks;

namespace NuciSearch.Services
{
    public interface IGeolocationService
    {
        Task<string> GetCountryCodeAsync(string ipAddress);
    }
}
