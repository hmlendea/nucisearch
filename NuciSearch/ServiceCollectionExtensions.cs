using Microsoft.Extensions.DependencyInjection;
using NuciSearch.Services;

namespace NuciSearch
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddNuciSearchServices(this IServiceCollection services)
        {
            services.AddSingleton<ISearchService, SearchService>();

            return services;
        }
    }
}
