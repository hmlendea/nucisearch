# Geolocation Service

NuciSearch resolves client IP addresses to country codes to determine the UI culture. The GeolocationService handles IP validation, caching, external API integration, and error handling.

## 1. Service Overview

```csharp
public sealed class GeolocationService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger logger) : IGeolocationService
```

### Dependencies

| Dependency | Purpose |
|------------|---------|
| `IHttpClientFactory` | Creates HTTP client for ipwho.is API calls (configured as "Geolocation") |
| `IMemoryCache` | 24-hour in-memory cache for IP→country mappings |
| `ILogger` | Error logging via NuciLog |

### Registration

```csharp
// ServiceCollectionExtensions.cs
services.AddHttpClient("Geolocation");
services.AddMemoryCache();
services.AddSingleton<IGeolocationService, GeolocationService>();
```

## 2. Public API

```csharp
public async Task<string> GetCountryCodeAsync(string ipAddress)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `ipAddress` | `string` | Raw IP address string (IPv4 or IPv6) |

### Returns

| Return Value | Description |
|--------------|-------------|
| `"RO"` | Private/loopback IP (developer convenience) |
| `"RO"` | Romania (user is in Romania) |
| `"GB"` | United Kingdom (default fallback for all other countries) |
| Any other `country_code` | From ipwho.is API response |

### Error Handling

- **API failure** → Returns `"GB"`, logs error with `NuciSearchOperation.GetCountryCode` and `NuciSearchLogInfoKey.IpAddress`
- **Empty/malformed response** → Returns `"GB"`
- **Invalid JSON** → Returns `"GB"`

## 3. Processing Pipeline

```
GetCountryCodeAsync(ipAddress)
    ↓
IsPrivateOrLoopback(ipAddress)
    ↓
true → return "RO"
    ↓
false → cache.Get(ipAddress)
    ↓
hit → return cached country code
    ↓
miss → call ipwho.is API
    ↓
parse response → country_code
    ↓
cache.Set(ipAddress, countryCode, 24h)
    ↓
return countryCode
```

### Step 1: Private IP Check

```csharp
private static bool IsPrivateOrLoopback(string ipAddress)
{
    if (string.IsNullOrEmpty(ipAddress)) return true;
    if (ipAddress == "::1" || ipAddress == "127.0.0.1") return true;
    if (ipAddress.StartsWith("192.168.")) return true;
    if (ipAddress.StartsWith("10.")) return true;
    if (ipAddress.StartsWith("172.")) return true;
    return false;
}
```

**Note:** Private IP ranges:
- `127.0.0.1/8` (loopback)
- `10.0.0.0/8` (private)
- `172.16.0.0/12` (private, but code checks `172.` broadly)
- `192.168.0.0/16` (private)
- `::1` (loopback IPv6)
- `fe80::/10` (link-local IPv6) — **NOT checked** (would need additional logic)

### Step 2: Cache Lookup

- **Backend:** `IMemoryCache` (in-process)
- **Key:** Raw IP address string
- **TTL:** 24 hours
- **Eviction:** Automatic on TTL expiry or memory pressure

### Step 3: ipwho.is API Call

```csharp
HttpClient client = httpClientFactory.CreateClient("Geolocation");
IpWhoIsResponse? response = await client.GetFromJsonAsync<IpWhoIsResponse>(
    $"https://ipwho.is/{Uri.EscapeDataString(ipAddress)}?fields=country_code");
```

**Endpoint:** `https://ipwho.is/{ip}?fields=country_code`

**Response format:**
```json
{
  "country_code": "RO",
  "country_name": "Romania",
  "ip": "8.8.8.8",
  "latitude": 44.87,
  "longitude": 23.62,
  "timezone": "Europe/Bucharest",
  "currency": "RON"
}
```

**Note:** The `fields=country_code` query parameter limits the response to just the country code, reducing bandwidth.

### Step 4: Cache Storage

```csharp
cache.Set(ipAddress, countryCode, TimeSpan.FromHours(24));
```

- **Key:** IP address string
- **Value:** Country code string
- **Expiration:** 24 hours from storage

## 4. Default Country Code

```csharp
string countryCode = "GB"; // Default fallback
```

**Rationale:** United Kingdom is the default because:
- English is the primary language of the application
- Most non-Romanian traffic is English-speaking
- Fallback prevents `null` references

## 5. External Dependency: ipwho.is

### API Details

| Property | Value |
|----------|-------|
| **Base URL** | `https://ipwho.is` |
| **Endpoint** | `/{ip}?fields=country_code` |
| **Rate limits** | Generous for self-hosted usage |
| **API key** | Not required |
| **Cost** | Free |
| **Reliability** | High (CDN-backed) |

### Failure Scenarios

| Scenario | Behaviour |
|----------|-----------|
| **Network error** | Returns `"GB"`, logs error |
| **HTTP 429 (rate limit)** | Returns `"GB"`, logs error |
| **HTTP 500 (server error)** | Returns `"GB"`, logs error |
| **Timeout** | Returns `"GB"`, logs error |
| **Invalid JSON** | Returns `"GB"`, logs error |
| **Missing country_code field** | Returns `"GB"` |

### Error Logging

```csharp
logger.Error(
    NuciSearchOperation.GetCountryCode,
    OperationStatus.Failure,
    exception,
    [new(NuciSearchLogInfoKey.IpAddress, ipAddress)]);
```

### Caching Under Failure

Even on failure, the cache is **not** updated. The `"GB"` default is returned, and the next request for the same IP will retry the API call.

## 6. IP Address Sources

### Primary Source: `httpContext.Connection.RemoteIpAddress`

```csharp
if (httpContext.Connection.RemoteIpAddress is not null)
{
    ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
}
```

**This is the IP address of the direct TCP connection.** In a typical self-hosted deployment (Kestrel behind nothing), this is the client's real IP.

### Fallback Source: `X-Forwarded-For` Header

```csharp
if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
{
    string firstIp = forwardedFor.ToString().Split(',')[0].Trim();
    if (!string.IsNullOrEmpty(firstIp))
    {
        ipAddress = firstIp;
    }
}
```

**Use case:** When NuciSearch is behind a reverse proxy (nginx, Cloudflare, etc.), `RemoteIpAddress` is the proxy's IP. The `X-Forwarded-For` header contains the original client IP.

**Note:** The code takes only the **first** IP in the comma-separated list. This is the most common case, but doesn't handle `X-Forwarded-For` spoofing or multiple proxy hops.

### Not Implemented: `X-Real-IP`

The code does **not** check `X-Real-IP`. If needed, this could be added as an additional fallback.

## 7. Caching Behaviour in Production

### Single Instance

```
Request 1: IP 8.8.8.8 → API call → "US" → cache["8.8.8.8"] = "US"
Request 2: IP 8.8.8.8 → cache hit → "US"
```

### Multiple Instances (Horizontal Scaling)

```
Instance A: IP 8.8.8.8 → API call → "US" → cache["8.8.8.8"] = "US"
Instance B: IP 8.8.8.8 → API call → "US" → cache["8.8.8.8"] = "US"
```

**Each instance has its own cache.** The first request for a given IP on any instance will hit the API. Subsequent requests (on any instance) will have a cache miss until the 24-hour TTL expires.

**Mitigation:** For true horizontal scaling with consistent geolocation, a distributed cache (Redis, SQL Server) would be needed. This is a known limitation of the current `IMemoryCache` approach.

## 8. Testing Geolocation

### Unit Tests

```csharp
[Test]
public void GivenPrivateIp_WhenGettingCountryCode_ThenReturnsRo()
    => Assert.That(geolocationService.GetCountryCodeAsync("127.0.0.1"), Is.EqualTo("RO"));

[Test]
public void GivenLoopbackIpv6_WhenGettingCountryCode_ThenReturnsRo()
    => Assert.That(geolocationService.GetCountryCodeAsync("::1"), Is.EqualTo("RO"));

[Test]
public void Given192_168_Ip_WhenGettingCountryCode_ThenReturnsRo()
    => Assert.That(geolocationService.GetCountryCodeAsync("192.168.1.1"), Is.EqualTo("RO"));

[Test]
public void Given10_Ip_WhenGettingCountryCode_ThenReturnsRo()
    => Assert.That(geolocationService.GetCountryCodeAsync("10.0.0.1"), Is.EqualTo("RO"));

[Test]
public void Given172_Ip_WhenGettingCountryCode_ThenReturnsRo()
    => Assert.That(geolocationService.GetCountryCodeAsync("172.16.0.1"), Is.EqualTo("RO"));
```

### Integration Tests (requires mock HTTP client)

```csharp
[Test]
public async Task GivenPublicIp_WhenGettingCountryCode_ThenReturnsCountryCode()
{
    // Arrange - mock httpClientFactory to return mock HttpClient
    // that returns a valid IpWhoIsResponse

    // Act
    var result = await geolocationService.GetCountryCodeAsync("8.8.8.8");

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Is.EqualTo("US")); // Google DNS is in US
}
```

## 9. Configuration

### appsettings.json

```json
{
  "NuciLoggerSettings": {
    "logFilePath": "logs/nucisearch.log",
    "isFileOutputEnabled": false
  }
}
```

### Environment Variables (if needed)

NuciLog respects the `ASPNETCORE_ENVIRONMENT` variable for development/production mode, but the GeolocationService itself has no environment-specific configuration.

## 10. Monitoring and Alerts

### Log Patterns to Monitor

```
ERROR GetCountryCode Failure IpAddress=8.8.8.8
```

### Metrics

| Metric | Description |
|--------|-------------|
| `geolocation_cache_hits` | Number of cache hits (via custom metric) |
| `geolocation_cache_misses` | Number of cache misses (API calls) |
| `geolocation_api_errors` | Number of failed API calls |
| `geolocation_default_fallback` | Number of times "GB" was returned |

### Alert Conditions

- `geolocation_api_errors` > 10 per minute → investigate ipwho.is availability
- `geolocation_default_fallback` > 50% of requests → investigate IP detection or network configuration

## 10. Security Considerations

### IP Privacy

- Only the IP address is logged (no personal data beyond geolocation)
- No IP addresses are persisted beyond the 24-hour in-memory cache
- The cache is in-process and lost on application restart

### X-Forwarded-For Handling

- The code trusts the first IP in `X-Forwarded-For` without validation
- In production behind a reverse proxy, ensure the proxy is trusted and adds `X-Forwarded-For` correctly
- Consider adding IP validation (e.g., check that the IP is in a known proxy range) for enhanced security

### Denial of Service

- The 24-hour cache limits API calls to unique IPs
- An attacker could exhaust cache entries, causing more API calls
- The ipwho.is API has generous rate limits for typical usage