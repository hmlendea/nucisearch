using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using NuciSearch.Components;
using NuciSearch.Localisation;

namespace NuciSearch
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddNuciSearchServices(builder.Configuration);

            WebApplication app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
            }

            CultureInfo[] supportedCultures = [new("en-GB"), new("ro-RO")];

            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-GB");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = [app.Services.GetRequiredService<IpCultureProvider>()];
            });

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();

            app.MapGet("/opensearch.xml", (IStringLocalizer<SharedResources> L) =>
            {
                string xml = $$"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <OpenSearchDescription xmlns="http://a9.com/-/spec/opensearch/1.1/">
                        <ShortName>NuciSearch</ShortName>
                        <Description>{{L["OpenSearch_Description"]}}</Description>
                        <InputEncoding>UTF-8</InputEncoding>
                        <Image width="64" height="64" type="image/png">https://search.nuilandia.ro/assets/logo.png</Image>
                        <Url type="text/html" method="get" template="https://search.nuilandia.ro?q={searchTerms}" />
                    </OpenSearchDescription>
                    """;
                return Results.Content(xml, "application/opensearchdescription+xml", System.Text.Encoding.UTF8);
            });

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
