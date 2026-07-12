using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciSearch.Services;

namespace NuciSearch
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddNuciSearchServices(this IServiceCollection services, IConfiguration configuration)
        {
            NuciLoggerSettings loggingSettings = new();
            configuration.Bind(nameof(NuciLoggerSettings), loggingSettings);
            services.AddSingleton(loggingSettings);

            services.AddSingleton<ILogger, NuciLogger>();
            services.AddSingleton<ISearchService, SearchService>();

            return services;
        }
    }
}
