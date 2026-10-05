# API Reference

NuciSearch exposes functionality through service interfaces, HTTP endpoints, and OpenSearch. This document provides a complete reference for developers integrating with or extending NuciSearch.

## 1. Service Interfaces

### 1.1 ISearchService

```csharp
public interface ISearchService
{
    string GetSearchUrl(string query, string searchType);
}
```

**Location:** `NuciSearch/Services/ISearchService.cs`

**Implementation:** `SearchService`

#### GetSearchUrl

```csharp
string GetSearchUrl(string query, string searchType)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `query` | `string` | User's search query (can be empty, obfuscated, or contain special patterns) |
| `searchType` | `string` | Search mode: `"auto"`, `"text"`, `"images"`, `"maps"`, `"torrents"`, `"videos"` |

| Return | Type | Description |
|--------|------|-------------|
| `string` | Fully-formed search engine URL with encoded query |

#### Search Types

| Type | Description | Default Engine |
|------|-------------|----------------|
| `auto` | Smart routing based on query patterns | Brave / DuckDuckGo |
| `text` | General web search | Brave / DuckDuckGo |
| `images` | Image search | DuckDuckGo Images |
| `maps` | Map search | Google Maps (culture-aware) |
| `torrents` | Torrent search | 1337x |
| `videos` | Video search | DuckDuckGo Videos |

#### Routing Logic (Auto Mode)

The `auto` mode applies pattern matching in this order:

1. **Empty/whitespace** → Empty string
2. **Image keywords** (`image`, `img`, `photo`, `picture`, `pic`, `wallpaper`, `background`, `icon`, `logo`, `screenshot`, `art`, `drawing`, `illustration`, `render`, `vector`, `svg`, `gif`, `meme`) → DuckDuckGo Images
3. **Wikidata ID** (`Q` + digits) → Wikidata entity page
4. **Wikidata keyword** (`wikidata` + query) → Wikidata search
5. **Jira key** (`PROJECT-123` pattern) → Atlassian Jira
6. **Rally key** (`US12345`, `DE12345`, `TC12345`, `TA12345`) → Rally
7. **Currency conversion** (`100 USD in EUR`, `100 lei in euro`) → DuckDuckGo with normalised currency codes
8. **IP address** (`ip 8.8.8.8`, `ipv4 1.1.1.1`, `ipv6 ::1`) → ipwho.is
9. **Provider keywords** (`emag`, `altex`, `amazon`, `github`, `stackoverflow`, etc.) → Provider-specific URL
10. **Default** → Brave Search (randomly alternates with DuckDuckGo)

#### Example Usage

```csharp
ISearchService searchService = serviceProvider.GetRequiredService<ISearchService>();

// Auto mode - smart routing
string url1 = searchService.GetSearchUrl("cats", "auto");
// → "https://search.brave.com/search?q=cats" (or DuckDuckGo)

// Explicit image search
string url2 = searchService.GetSearchUrl("cats", "images");
// → "https://duckduckgo.com/?iax=images&ia=images&q=cats"

// Jira issue
string url3 = searchService.GetSearchUrl("AAP-123", "auto");
// → "https://worldpay.atlassian.net/browse/AAP-123"

// Currency conversion
string url4 = searchService.GetSearchUrl("100 USD in EUR", "auto");
// → "https://duckduckgo.com/?q=100%20USD%20in%20EUR"

// Provider keyword
string url5 = searchService.GetSearchUrl("emag laptop", "auto");
// → "https://emag.ro/search/laptop"
```

---

### 1.2 IGeolocationService

```csharp
public interface IGeolocationService
{
    Task<string> GetCountryCodeAsync(string ipAddress);
}
```

**Location:** `NuciSearch/Services/IGeolocationService.cs`

**Implementation:** `GeolocationService`

#### GetCountryCodeAsync

```csharp
Task<string> GetCountryCodeAsync(string ipAddress)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `ipAddress` | `string` | IPv4 or IPv6 address string |

| Return | Type | Description |
|--------|------|-------------|
| `Task<string>` | Country code: `"RO"` (Romania/private), `"GB"` (UK/default), or ISO 3166-1 alpha-2 code from ipwho.is |

#### Return Values

| Input | Output | Reason |
|-------|--------|--------|
| `null`, `""`, `"127.0.0.1"`, `"::1"` | `"RO"` | Private/loopback IP |
| `"192.168.x.x"`, `"10.x.x.x"`, `"172.x.x.x"` | `"RO"` | Private IP ranges |
| Public IP in Romania | `"RO"` | ipwho.is returns RO |
| Public IP elsewhere | `"GB"` | Default fallback for non-RO |
| API error | `"GB"` | Fallback on failure |

#### Example Usage

```csharp
IGeolocationService geoService = serviceProvider.GetRequiredService<IGeolocationService>();

string countryCode = await geoService.GetCountryCodeAsync("8.8.8.8");
// → "US" (Google DNS)

string countryCode2 = await geoService.GetCountryCodeAsync("127.0.0.1");
// → "RO" (private IP)
```

---

## 2. Data Transfer Objects

### 2.1 IpWhoIsResponse

```csharp
public sealed class IpWhoIsResponse
{
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
```

**Location:** `NuciSearch/Services/IpWhoIsResponse.cs`

**Usage:** Internal DTO for deserialising ipwho.is API response. Only `CountryCode` is used by `GeolocationService`.

---

## 3. HTTP Endpoints

### 3.1 OpenSearch Description

```
GET /opensearch.xml
```

**Response:** `application/opensearchdescription+xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OpenSearchDescription xmlns="http://a9.com/-/spec/opensearch/1.1/">
  <ShortName>NuciSearch</ShortName>
  <Description>Search wrapper for multiple engines</Description>
  <InputEncoding>UTF-8</InputEncoding>
  <Image height="16" width="16" type="image/x-icon">/assets/favicon/favicon.ico</Image>
  <Url type="text/html" method="GET" template="https://search.nuilandia.ro/?q={searchTerms}&type=auto"/>
  <Url type="application/opensearchdescription+xml" rel="self" template="https://search.nuilandia.ro/opensearch.xml"/>
</OpenSearchDescription>
```

**Usage:** Allows browsers to add NuciSearch as a search engine.

### 3.2 Search Page (Blazor Server)

```
GET /
```

**Response:** `text/html` — Blazor Server application shell.

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | `string` | Search query |
| `type` | `string` | Search type: `auto`, `text`, `images`, `maps`, `torrents`, `videos` |

**Example:**
```
https://search.nuilandia.ro/?q=cats&type=images
```

### 3.3 Blazor Circuit (WebSocket)

```
/_NuciSearch.WebSocket
```

**Protocol:** WebSocket (Blazor Server circuit)

**Note:** Internal endpoint for Blazor Server real-time communication. Not for direct API consumption.

---

## 4. Extension Points

### 4.1 Adding a Search Provider

To add a new search provider (e.g., `newsite`):

1. **Add URL builder method** in `SearchService.cs`:
   ```csharp
   private static string GetNewSiteUrl(string query)
       => $"https://newsite.com/search?q={Uri.EscapeDataString(query)}";
   ```

2. **Add keyword check** in `GetAutoUrlForMultiWordQuery`:
   ```csharp
   if (query.StartsWith("newsite ", StringComparison.OrdinalIgnoreCase))
   {
       return GetNewSiteUrl(query["newsite ".Length..]);
   }
   ```

3. **Add tests** in `SearchServiceTests.cs`:
   ```csharp
   [Test]
   public void GivenNewSiteKeyword_WhenGettingSearchUrl_ThenReturnsNewSiteUrl()
       => Assert.That(
           searchService.GetSearchUrl("newsite query", "auto"),
           Is.EqualTo("https://newsite.com/search?q=query"));
   ```

### 4.2 Adding a Query Pattern

To add a new pattern (e.g., `DOI:10.1000/xyz`):

1. **Add regex pattern** as static readonly field:
   ```csharp
   private static readonly Regex DoiRegex = new(@"^doi:\s*\d+\.\d+/.+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
   ```

2. **Add check** in `GetAutoUrl` (before provider keywords):
   ```csharp
   if (DoiRegex.IsMatch(query))
   {
       string doi = query["doi:".Length..].Trim();
       return $"https://doi.org/{Uri.EscapeDataString(doi)}";
   }
   ```

3. **Add tests** for the pattern.

### 4.3 Adding a Culture

To add a new culture (e.g., `de-DE`):

1. **Add resource file:** `Resources/SharedResources.de-DE.resx`
2. **Update `IpCultureProvider.cs`** to map country code to culture:
   ```csharp
   if (countryCode == "DE" || countryCode == "AT" || countryCode == "CH")
   {
       return new ProviderCultureResult("de-DE");
   }
   ```
3. **Add culture-specific URL branches** in URL builders:
   ```csharp
   if (culture == "de-DE")
   {
       return $"https://google.de/maps/search/{Uri.EscapeDataString(query)}";
   }
   ```

### 4.4 Adding a Domain Blacklist

To add a domain blacklist for a topic (e.g., `newgame`):

1. **Add regex pattern:**
   ```csharp
   private static readonly Regex NewGameRegex = new(@"\bnewgame\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
   ```

2. **Add check** in `ApplyDomainBlacklist`:
   ```csharp
   if (NewGameRegex.IsMatch(query))
   {
       return $"{url}&as_sitesearch=-newgame.fandom.com";
   }
   ```

---

## 5. Configuration Reference

### 5.1 NuciLoggerSettings

```json
{
  "NuciLoggerSettings": {
    "logFilePath": "logs/nucisearch.log",
    "isFileOutputEnabled": false
  }
}
```

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `logFilePath` | `string` | `logs/nucisearch.log` | Path to log file (relative to app directory) |
| `isFileOutputEnabled` | `bool` | `false` | Enable file logging |

### 5.2 HttpClient: Geolocation

```csharp
// Program.cs
builder.Services.AddHttpClient("Geolocation");
```

**Configuration:** No additional config. Uses default `HttpClient` settings.

**Timeout:** Default 100 seconds. Can be configured:
```csharp
builder.Services.AddHttpClient("Geolocation", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

---

## 6. Logging Reference

### 6.1 Operations (NuciSearchOperation)

| Operation | Description |
|-----------|-------------|
| `Search` | Search query processing |
| `GetCountryCode` | Geolocation API call |
| `OpenSearch` | OpenSearch XML endpoint |

### 6.2 Log Info Keys (NuciSearchLogInfoKey)

| Key | Type | Description |
|-----|------|-------------|
| `Query` | `string` | Search query |
| `SearchType` | `string` | Search type (auto, text, images, etc.) |
| `IpAddress` | `string` | Client IP address |
| `CountryCode` | `string` | Resolved country code |
| `Culture` | `string` | Selected UI culture |

### 6.3 Example Log Entry

```json
{
  "Timestamp": "2024-01-15T10:30:45.123Z",
  "Level": "Information",
  "Operation": "Search",
  "Status": "Success",
  "Properties": {
    "Query": "emag laptop",
    "SearchType": "auto",
    "Culture": "ro-RO"
  }
}
```

---

## 7. Error Handling

### 7.1 SearchService

- **Never throws** — Returns empty string for empty query, valid URL otherwise
- **Invalid searchType** — Treated as `"auto"`
- **Obfuscation failure** — Falls back to original query
- **Logging** — Errors logged with `OperationStatus.Failure`

### 7.2 GeolocationService

- **Never throws** — Returns `"GB"` on any error
- **Private IPs** — Returns `"RO"` immediately (no API call)
- **Cache errors** — Ignored, proceeds to API call
- **API errors** — Logged, returns `"GB"`

### 7.3 Global Error Handling

```csharp
// Program.cs
app.UseExceptionHandler("/Error");
```

**Error Page:** `Components/Pages/Error.razor` — Shows generic error message, no stack traces in production.

---

## 8. Version Compatibility

| NuciSearch | .NET | NuciLog | NuciText.Obfuscation |
|------------|------|---------|---------------------|
| 1.x | 10.0 | 3.0.0 | 1.1.1 |

**Breaking Changes:** None in 1.x series. Major version bump for breaking changes.

---

## 9. Client Integration Examples

### 9.1 Browser Search Engine

Add via OpenSearch:
1. Visit `https://search.nuilandia.ro/`
2. Browser detects OpenSearch
3. Add as search engine

### 9.2 Programmatic Search

```bash
# Get search URL for query
curl "https://search.nuilandia.ro/?q=cats&type=auto" -L -s -o /dev/null -w "%{url_effective}\n"
```

### 9.3 Custom Client (C#)

```csharp
using System.Net.Http;
using System.Text.Json;

public class NuciSearchClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public NuciSearchClient(HttpClient http, string baseUrl = "https://search.nuilandia.ro")
    {
        _http = http;
        _baseUrl = baseUrl;
    }

    public async Task<string> GetSearchUrlAsync(string query, string type = "auto")
    {
        var response = await _http.GetAsync($"{_baseUrl}/?q={Uri.EscapeDataString(query)}&type={type}");
        return response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
    }
}
```

---

## 10. Internal APIs (Not for External Use)

### 10.1 IpCultureProvider

```csharp
public class IpCultureProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext);
}
```

**Purpose:** ASP.NET Core localisation provider. Determines culture from client IP via `IGeolocationService`.

**Not for external consumption** — Part of ASP.NET Core localisation pipeline.

### 10.2 SearchService Static Methods

All URL builders are `private static` methods in `SearchService`. Not accessible externally.

**Extension:** Add new providers by modifying `SearchService.cs` directly (see [Extensibility](extensibility.md)).