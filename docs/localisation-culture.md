# Localisation & Culture

NuciSearch supports two cultures: `en-GB` (default) and `ro-RO` (Romanian). Culture selection is automatic based on the user's IP geolocation.

## 1. Culture Detection Pipeline

```
HTTP Request
    ↓
IpCultureProvider.DetermineProviderCultureResult(HttpContext)
    ↓
Extract IP from:
  1. httpContext.Connection.RemoteIpAddress
  2. X-Forwarded-For header (first IP if present)
    ↓
GeolocationService.GetCountryCodeAsync(ipAddress)
    ↓
Map country code → culture:
  "RO" → "ro-RO"
  *    → "en-GB"
    ↓
Return ProviderCultureResult(culture)
    ↓
ASP.NET Core RequestLocalizationMiddleware applies culture
    ↓
CultureInfo.CurrentUICulture set for the request
    ↓
SearchService reads CultureInfo.CurrentUICulture for culture-aware URLs
```

## 2. IpCultureProvider

```csharp
public sealed class IpCultureProvider(IGeolocationService geolocationService) : IRequestCultureProvider
{
    public async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        string ipAddress = string.Empty;

        // 1. Direct connection IP
        if (httpContext.Connection.RemoteIpAddress is not null)
        {
            ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
        }

        // 2. X-Forwarded-For (for reverse proxy scenarios)
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
        {
            string firstIp = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(firstIp))
            {
                ipAddress = firstIp;
            }
        }

        // 3. Geolocation lookup
        string countryCode = await geolocationService.GetCountryCodeAsync(ipAddress);
        string culture = "en-GB";

        if (string.Equals(countryCode, "RO"))
        {
            culture = "ro-RO";
        }

        return new ProviderCultureResult(culture);
    }
}
```

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| `X-Forwarded-For` takes precedence | In reverse proxy deployments (nginx, Cloudflare), `RemoteIpAddress` is the proxy's IP. The first IP in `X-Forwarded-For` is the original client. |
| Single culture per request | `ProviderCultureResult` sets both `Culture` and `UICulture` to the same value. No separate formatting culture needed. |
| Default to `en-GB` | English is the fallback for all non-Romanian countries. |
| No cookie/header override | Culture is purely IP-based. No user preference persistence. |

## 3. GeolocationService

```csharp
public async Task<string> GetCountryCodeAsync(string ipAddress)
{
    // 1. Private/loopback IPs → RO (developer convenience)
    if (IsPrivateOrLoopback(ipAddress))
        return "RO";

    // 2. Check 24-hour in-memory cache
    string? cachedCode = cache.Get<string>(ipAddress);
    if (cachedCode is not null)
        return cachedCode;

    // 3. Call ipwho.is API
    HttpClient client = httpClientFactory.CreateClient("Geolocation");
    IpWhoIsResponse? response = await client.GetFromJsonAsync<IpWhoIsResponse>(
        $"https://ipwho.is/{Uri.EscapeDataString(ipAddress)}?fields=country_code");

    string countryCode = "GB"; // Default fallback

    if (response is not null && !string.IsNullOrEmpty(response.CountryCode))
    {
        countryCode = response.CountryCode;
    }

    // 4. Cache for 24 hours
    cache.Set(ipAddress, countryCode, TimeSpan.FromHours(24));

    return countryCode;
}
```

### Private IP Handling

```csharp
private static bool IsPrivateOrLoopback(string ipAddress)
{
    if (string.IsNullOrEmpty(ipAddress)) return true;
    if (ipAddress == "::1" || ipAddress == "127.0.0.1") return true;
    if (ipAddress.StartsWith("192.168.")) return true;
    if (ipAddress.StartsWith("10.")) return true;
    if (ipAddress.StartsWith("172.")) return true; // Note: includes 172.16-172.31
    return false;
}
```

**Note:** The `172.` check is broad — it includes `172.0.0.0/8` rather than just `172.16.0.0/12`. This is intentional for simplicity; private IPs in the `172.0.0.0/8` range are rare in practice.

### Cache Strategy

- **Backend:** `IMemoryCache` (in-process, no distributed cache)
- **TTL:** 24 hours
- **Key:** Raw IP address string
- **Eviction:** Automatic on TTL expiry or memory pressure

**Implication:** In a horizontally scaled deployment, each instance has its own cache. The same IP may hit different instances and get different results until caches populate. This is acceptable for culture detection (eventual consistency).

### Error Handling

- **API failure** → Returns `"GB"` (English), logs error with `NuciSearchOperation.GetCountryCode` and `NuciSearchLogInfoKey.IpAddress`
- **Empty response** → Returns `"GB"`
- **Invalid JSON** → Returns `"GB"`

### External Dependency

**ipwho.is** — Free IP geolocation API
- Endpoint: `https://ipwho.is/{ip}?fields=country_code`
- Rate limits: Generous for typical self-hosted usage
- No API key required
- Response: `{ "country_code": "RO" }`

## 4. Culture-Aware URL Builders

Three URL builders in `SearchService` read `CultureInfo.CurrentUICulture.Name`:

### Firefox Extensions

```csharp
private static string GetFirefoxExtensionsUrl(string query)
{
    if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
    {
        return "https://addons.mozilla.org/ro/firefox/search/?q=" + Uri.EscapeDataString(query);
    }
    return "https://addons.mozilla.org/en-GB/firefox/search/?q=" + Uri.EscapeDataString(query);
}
```

### Google Maps

```csharp
private static string GetGoogleMapsUrl(string query)
{
    if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
    {
        return $"https://google.ro/maps/search/{Uri.EscapeDataString(query)}";
    }
    return $"https://google.co.uk/maps/search/{Uri.EscapeDataString(query)}";
}
```

### IKEA

```csharp
private static string GetIkeaUrl(string query)
{
    if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
    {
        return $"https://ikea.com/ro/ro/search/?q={Uri.EscapeDataString(query)}";
    }
    return $"https://ikea.com/gb/en/search/?q={Uri.EscapeDataString(query)}";
}
```

### Wikipedia (Random Instance Selection)

```csharp
private static string GetWikiPediaUrl(string query)
{
    string encodedQuery = Uri.EscapeDataString(query);
    string langCode = "en";

    if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
    {
        langCode = "ro";
    }

    string[] instances = [
        $"https://{langCode}.wikipedia.org/w/index.php?search={encodedQuery}",
        $"https://wikiless.tiekoetter.com/w/index.php?search={encodedQuery}&lang={langCode}",
    ];

    return instances[Random.Shared.Next(instances.Length)];
}
```

## 5. Localisation Resources

### Resource Files

| File | Culture | Purpose |
|------|---------|---------|
| `SharedResources.resx` | Default (`en-GB`) | Base English strings |
| `SharedResources.ro-RO.resx` | `ro-RO` | Romanian translations |

### Resource Keys

| Key | English | Romanian |
|-----|---------|----------|
| `Home_Tab_Auto` | Auto | Auto |
| `Home_Tab_Text` | Text | Text |
| `Home_Tab_Images` | Images | Imagini |
| `Home_Tab_Torrents` | Torrents | Torrenturi |
| `Home_Tab_Videos` | Videos | Video |
| `Home_Tab_Locations` | Locations | Locații |
| `Home_SearchPlaceholder` | Search for something... | Caută ceva... |
| `Home_SearchButton` | Search | Caută |
| `NotFound_Title` | Not Found | Negăsit |
| `NotFound_Message` | Sorry, the content you are looking for does not exist. | Îmi pare rău, conținutul pe care îl cauți nu există. |
| `Error_Title` | Error. | Eroare. |
| `Error_Message` | An error occurred while processing your request. | A apărut o eroare în timpul procesării cererii tale. |
| `ReconnectModal_Rejoining` | Rejoining the server... | Reconectare la server... |
| `ReconnectModal_RejoinFailedPrefix` | Rejoin failed... trying again in | Reconectare eșuată... încerc din nou în |
| `ReconnectModal_RejoinFailedSuffix` | seconds. | secunde. |
| `ReconnectModal_RejoinFailed` | Failed to rejoin. Please retry or reload the page. | Reconectarea a eșuat. Te rog reîncearcă sau reîncarcă pagina. |
| `ReconnectModal_Retry` | Retry | Reîncearcă |
| `ReconnectModal_Paused` | The session has been paused by the server. | Sesiunea a fost pusă pe pauză de server. |
| `ReconnectModal_ResumeFailed` | Failed to resume the session. Please retry or reload the page. | Reluarea sesiunii a eșuat. Te rog reîncearcă sau reîncarcă pagina. |
| `ReconnectModal_Resume` | Resume | Reluare |
| `MainLayout_UnhandledError` | An unhandled error has occurred. | A apărut o eroare neașteptată. |
| `MainLayout_Reload` | Reload | Reîncarcă |
| `OpenSearch_Description` | Search the web with NuciSearch | Caută pe web cu NuciSearch |

### Usage in Components

```razor
@inject IStringLocalizer<SharedResources> L

<label>@L["Home_Tab_Auto"]</label>
<input placeholder="@L["Home_SearchPlaceholder"]" />
<button>@L["Home_SearchButton"]</button>
```

### Usage in Program.cs (OpenSearch)

```csharp
app.MapGet("/opensearch.xml", (IStringLocalizer<SharedResources> L) =>
{
    string xml = $$"""
        <Description>{{L["OpenSearch_Description"]}}</Description>
        ...
        """;
    return Results.Content(xml, "application/opensearchdescription+xml", Encoding.UTF8);
});
```

## 6. Supported Cultures Configuration

```csharp
CultureInfo[] supportedCultures = [new("en-GB"), new("ro-RO")];

app.UseRequestLocalization(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [app.Services.GetRequiredService<IpCultureProvider>()];
});
```

- **Default:** `en-GB`
- **Supported:** `en-GB`, `ro-RO`
- **Provider:** Only `IpCultureProvider` (no cookie, query string, or header providers)

## 7. Adding a New Culture

To add a new culture (e.g., `de-DE`):

1. **Add resource file:** `SharedResources.de-DE.resx` with translations
2. **Update `supportedCultures`:** Add `new("de-DE")` to the array
3. **Update `IpCultureProvider`:** Add mapping from country code to culture
4. **Update culture-aware URL builders:** Add branches for the new culture where needed
5. **Test:** Verify all localised strings and culture-specific URLs work

## 8. Testing Culture Behaviour

Tests use `[SetUICulture("ro-RO")]` and `[SetUICulture("en-GB")]` attributes:

```csharp
[Test]
[SetUICulture("ro-RO")]
public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleRoMapsUrl()
    => Assert.That(
        searchService.GetSearchUrl("london", "maps"),
        Is.EqualTo("https://google.ro/maps/search/london"));

[Test]
[SetUICulture("en-GB")]
public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleCoUkMapsUrl()
    => Assert.That(
        searchService.GetSearchUrl("london", "maps"),
        Is.EqualTo("https://google.co.uk/maps/search/london"));
```

The `SetUICulture` attribute sets `CultureInfo.CurrentUICulture` for the test duration.