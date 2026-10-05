# Extensibility Guide

NuciSearch is designed for easy extension. This guide covers the four main extension points: adding search providers, query patterns, cultures, and domain blacklists.

## 1. Architecture for Extensibility

```
SearchService.GetSearchUrl(query, type)
    │
    ├─► type == "images" → GetImagesUrl()
    ├─► type == "maps" → GetMapsUrl()
    ├─► type == "torrents" → GetTorrentsUrl()
    ├─► type == "videos" → GetVideosUrl()
    ├─► type == "text" → GetTextUrl()
    └─► type == "auto" → GetAutoUrl()
                              │
                              ├─► Empty/whitespace → ""
                              ├─► Image keywords → GetImagesUrl()
                              ├─► Wikidata ID (Q123) → GetWikidataUrl()
                              ├─► Wikidata keyword → GetWikidataSearchUrl()
                              ├─► Jira pattern → GetJiraUrl()
                              ├─► Rally pattern → GetRallyUrl()
                              ├─► Currency pattern → GetCurrencyConversionUrl()
                              ├─► IP pattern → GetIpWhoIsUrl()
                              ├─► Provider keywords → Get{Provider}Url()
                              └─► Default → GetBraveUrl() / GetDuckDuckGoUrl()
```

All routing logic is in `SearchService.cs` with **private static methods** for each URL builder. Extension requires modifying this file.

---

## 2. Adding a Search Provider

### 2.1 When to Add a Provider

Add a provider when:
- A site has a dedicated search engine (e.g., `emag.ro`, `github.com`, `stackoverflow.com`)
- Users frequently search that site directly
- The site's search URL structure is stable

### 2.2 Step-by-Step

#### Step 1: Add URL Builder Method

In `SearchService.cs`, add a `private static` method:

```csharp
private static string GetNewProviderUrl(string query)
    => $"https://newprovider.com/search?q={Uri.EscapeDataString(query)}";
```

**Naming convention:** `Get{ProviderName}Url` (PascalCase)

**Parameters:** Single `string query` — the search terms after the keyword

**Return:** Fully-formed URL with query encoded

#### Step 2: Add Keyword Detection

In `GetAutoUrlForMultiWordQuery`, add a check **before** the default case:

```csharp
if (query.StartsWith("newprovider ", StringComparison.OrdinalIgnoreCase))
{
    return GetNewProviderUrl(query["newprovider ".Length..]);
}
```

**Placement:** Add after existing provider checks, before the final `return GetBraveUrl(...)` or `GetDuckDuckGoUrl(...)`.

**Keyword matching:** Use `StartsWith("keyword ", StringComparison.OrdinalIgnoreCase)` for prefix matching.

#### Step 3: Add Tests

In `SearchServiceTests.cs`:

```csharp
[Test]
public void GivenNewProviderKeyword_WhenGettingSearchUrl_ThenReturnsNewProviderUrl()
    => Assert.That(
        searchService.GetSearchUrl("newprovider query", "auto"),
        Is.EqualTo("https://newprovider.com/search?q=query"));
```

Add culture-specific test if needed:

```csharp
[Test]
[SetUICulture("ro-RO")]
public void GivenNewProviderKeyword_WhenGettingSearchUrl_ThenReturnsNewProviderUrl()
    => Assert.That(
        searchService.GetSearchUrl("newprovider query", "auto"),
        Is.EqualTo("https://newprovider.ro/search?q=query"));
```

### 2.3 Provider Examples

| Provider | Keyword | URL Pattern |
|----------|---------|-------------|
| eMAG | `emag` | `https://emag.ro/search/{query}` |
| Altex | `altex` | `https://altex.ro/cauta/{query}` |
| Amazon | `amazon` | `https://amazon.com/s?k={query}` |
| GitHub | `github` | `https://github.com/search?q={query}` |
| Stack Overflow | `stackoverflow` | `https://stackoverflow.com/search?q={query}` |
| Wikipedia | `wiki` | `https://en.wikipedia.org/w/index.php?search={query}` |

### 2.4 Multi-Language Providers

For providers with language-specific domains:

```csharp
private static string GetIkeaUrl(string query, string culture)
{
    string domain = culture switch
    {
        "ro-RO" => "ikea.com/ro/ro",
        "de-DE" => "ikea.com/de/de",
        "fr-FR" => "ikea.com/fr/fr",
        _ => "ikea.com/us/en"
    };
    return $"https://{domain}/search/?q={Uri.EscapeDataString(query)}";
}
```

Then in the keyword check:

```csharp
if (query.StartsWith("ikea ", StringComparison.OrdinalIgnoreCase))
{
    string culture = CultureInfo.CurrentUICulture.Name;
    return GetIkeaUrl(query["ikea ".Length..], culture);
}
```

---

## 3. Adding a Query Pattern

### 3.1 When to Add a Pattern

Add a pattern when:
- Users search for structured identifiers (Jira keys, DOIs, ISBNs, tracking numbers)
- The pattern can be detected with a regex
- The target URL is deterministic from the pattern

### 3.2 Step-by-Step

#### Step 1: Add Regex Pattern

Add as `private static readonly Regex` field at the top of `SearchService.cs`:

```csharp
private static readonly Regex DoiRegex = new(
    @"^doi:\s*(10\.\d{4,9}/[-._;()/:A-Z0-9]+)$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);
```

**Naming:** `{PatternName}Regex` (PascalCase)

**Options:** Always use `RegexOptions.Compiled` for performance. Add `RegexOptions.IgnoreCase` if case-insensitive.

#### Step 2: Add Detection Logic

In `GetAutoUrl`, add check **before** provider keywords (patterns have priority):

```csharp
Match doiMatch = DoiRegex.Match(query);
if (doiMatch.Success)
{
    string doi = doiMatch.Groups[1].Value;
    return $"https://doi.org/{Uri.EscapeDataString(doi)}";
}
```

**Placement:** After IP pattern, before provider keywords.

**Extraction:** Use capture groups to extract the identifier.

#### Step 3: Add Tests

```csharp
[Test]
public void GivenDoiQuery_WhenGettingSearchUrl_ThenReturnsDoiUrl()
    => Assert.That(
        searchService.GetSearchUrl("DOI:10.1000/xyz123", "auto"),
        Is.EqualTo("https://doi.org/10.1000/xyz123"));

[Test]
public void GivenLowercaseDoiQuery_WhenGettingSearchUrl_ThenReturnsDoiUrl()
    => Assert.That(
        searchService.GetSearchUrl("doi:10.1000/xyz123", "auto"),
        Is.EqualTo("https://doi.org/10.1000/xyz123"));
```

### 3.3 Pattern Examples

| Pattern | Regex | Target URL |
|---------|-------|------------|
| Jira | `^[A-Z]{2,10}-\d+$` | `https://worldpay.atlassian.net/browse/{key}` |
| Rally | `^(US|DE|TC|TA)\d+$` | `https://rally1.rallydev.com/#/detail/{key}` |
| Wikidata | `^Q\d+$` | `https://www.wikidata.org/wiki/{id}` |
| ISBN | `^(?:ISBN[- ]?1[03]?[- ]?)?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$)[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$` | `https://isbnsearch.org/isbn/{isbn}` |
| DOI | `^doi:\s*(10\.\d{4,9}/.+)$` | `https://doi.org/{doi}` |
| Tracking | `^\d{12,}$` | `https://tools.usps.com/go/TrackConfirm_action?tLabels={num}` |

---

## 4. Adding a Culture

### 4.1 When to Add a Culture

Add a culture when:
- You want localised UI for a new language/region
- Search engines have country-specific domains (Google Maps, Wikipedia, Firefox, IKEA)
- The geolocation service should map a country code to this culture

### 4.2 Step-by-Step

#### Step 1: Add Resource File

Create `NuciSearch/Resources/SharedResources.{culture}.resx`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="SearchPlaceholder" xml:space="preserve">
    <value>Căutați pe web...</value>
  </data>
  <data name="SearchButton" xml:space="preserve">
    <value>Caută</value>
  </data>
  <!-- Copy all keys from SharedResources.resx -->
</root>
```

**Naming:** `SharedResources.{culture}.resx` (e.g., `SharedResources.de-DE.resx`)

**Content:** Translate all strings from `SharedResources.resx`

#### Step 2: Update IpCultureProvider

In `IpCultureProvider.cs`, add country code mapping:

```csharp
public async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
{
    // ... existing IP detection ...

    string culture = countryCode switch
    {
        "RO" => "ro-RO",
        "DE" or "AT" or "CH" => "de-DE",
        "FR" => "fr-FR",
        "ES" => "es-ES",
        "IT" => "it-IT",
        _ => "en-GB"
    };

    return new ProviderCultureResult(culture);
}
```

**Mapping:** Map ISO 3166-1 alpha-2 country codes to .NET culture names.

#### Step 3: Add Culture-Aware URL Branches

In relevant URL builders, add culture-specific branches:

```csharp
private static string GetMapsUrl(string query)
{
    string culture = CultureInfo.CurrentUICulture.Name;
    return culture switch
    {
        "ro-RO" => $"https://google.ro/maps/search/{Uri.EscapeDataString(query)}",
        "de-DE" => $"https://google.de/maps/search/{Uri.EscapeDataString(query)}",
        "fr-FR" => $"https://google.fr/maps/search/{Uri.EscapeDataString(query)}",
        "es-ES" => $"https://google.es/maps/search/{Uri.EscapeDataString(query)}",
        "it-IT" => $"https://google.it/maps/search/{Uri.EscapeDataString(query)}",
        _ => $"https://google.com/maps/search/{Uri.EscapeDataString(query)}"
    };
}
```

**Affected URL builders:**
- `GetMapsUrl` — Google Maps domains
- `GetFirefoxAddonUrl` — `addons.mozilla.org/{lang}`
- `GetIkeaUrl` — Country-specific IKEA domains
- `GetWikipediaUrl` — Language-specific Wikipedia
- `GetRedditUrl` — Could add regional subreddits

#### Step 4: Add Tests

```csharp
[Test]
[SetUICulture("de-DE")]
public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGermanMapsUrl()
    => Assert.That(
        searchService.GetSearchUrl("berlin", "maps"),
        Is.EqualTo("https://google.de/maps/search/berlin"));

[Test]
[SetUICulture("fr-FR")]
public void GivenFirefoxKeyword_WhenGettingSearchUrl_ThenReturnsFrenchFirefoxUrl()
    => Assert.That(
        searchService.GetSearchUrl("firefox ublock", "auto"),
        Is.EqualTo("https://addons.mozilla.org/fr/firefox/search/?q=ublock"));
```

---

## 5. Adding a Domain Blacklist

### 5.1 When to Add a Blacklist

Add a blacklist when:
- A topic has low-quality/spam domains that pollute results
- Users frequently search for that topic
- The domains are well-known and stable

### 5.2 Step-by-Step

#### Step 1: Add Regex Pattern

In `SearchService.cs`, add detection regex:

```csharp
private static readonly Regex NewGameRegex = new(
    @"\bnewgame\b",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);
```

#### Step 2: Add Blacklist Logic

In `ApplyDomainBlacklist` method, add a new condition:

```csharp
private static string ApplyDomainBlacklist(string url, string query)
{
    // ... existing checks ...

    if (NewGameRegex.IsMatch(query))
    {
        return $"{url}&as_sitesearch=-newgame.fandom.com -newgame.wikia.com";
    }

    return url;
}
```

**Format:** `&as_sitesearch=-domain1.com -domain2.com` (Google/DuckDuckGo site exclusion)

**Multiple domains:** Space-separated with `-` prefix.

#### Step 3: Add Tests

```csharp
[Test]
public void GivenNewGameQuery_WhenGettingTextSearchUrl_ThenIncludesNewGameBlacklist()
    => Assert.That(
        searchService.GetSearchUrl("newgame guide", "text"),
        Does.Contain("newgame.fandom.com")
            .And.Contain("newgame.wikia.com"));
```

### 5.3 Existing Blacklists

| Topic | Regex | Excluded Domains |
|-------|-------|------------------|
| Minecraft | `\bminecraft\b` | `minecraft.fandom.com` |
| GTA | `\bgta\b` | 10 domains including `gta.fandom.com`, `gtastarsandstripes.miraheze.org` |
| Roblox | `\broblox\b` | `roblox.fandom.com` |
| Terraria | `\bterraria\b` | `terraria.fandom.com` |
| Stardew Valley | `\bstardew\b` | `stardewvalley.fandom.com` |
| Elden Ring | `\beld[ -]?ring\b` | `eldenring.fandom.com` |
| Baldur's Gate 3 | `\bbg3\b\|\bbaldur[']?s?[ -]?gate\b` | `baldursgate3.fandom.com` |
| League of Legends | `\blol\b\|\bleague[ -]?of[ -]?legends\b` | `leagueoflegends.fandom.com` |
| Valorant | `\bvalorant\b` | `valorant.fandom.com` |
| CS2/CS:GO | `\bcs2\b\|\bcsgo\b\|\bcounter[ -]?strike\b` | `csgostash.com`, `cs.money` |
| Dota 2 | `\bdota\b` | `dota2.fandom.com` |
| WoW | `\bwow\b\|\bworld[ -]?of[ -]?warcraft\b` | `wowhead.com` (kept), `wow.fandom.com` |

---

## 6. Advanced Extensions

### 6.1 Custom Search Type

To add a new search type (e.g., `"news"`):

1. **Add case in `GetSearchUrl`:**
   ```csharp
   case "news":
       return GetNewsUrl(query);
   ```

2. **Add URL builder:**
   ```csharp
   private static string GetNewsUrl(string query)
       => $"https://news.google.com/search?q={Uri.EscapeDataString(query)}";
   ```

3. **Update OpenSearch XML** (`wwwroot/opensearch.xml`):
   ```xml
   <Url type="text/html" method="GET" template="https://search.nuilandia.ro/?q={searchTerms}&type=news"/>
   ```

4. **Add UI option** in `Home.razor` dropdown.

### 6.2 Custom Geolocation Provider

To replace ipwho.is:

1. **Create new DTO** for the provider's response
2. **Modify `GeolocationService`** to call new API
3. **Update `IpWhoIsResponse`** or create new DTO
4. **Keep same interface** `IGeolocationService` — no consumer changes needed

### 6.3 Custom Obfuscation

NuciSearch uses `NuciText.Obfuscation` for query deobfuscation. To customise:

```csharp
// In SearchService constructor
NuciTextObfuscatorOptions options = new()
{
    UseApproximateReplacements = true,
    // Custom options
};
```

The obfuscation is automatic — any query that was obfuscated with the same seed will be deobfuscated.

---

## 7. Testing Extensions

### 7.1 Test Checklist

For each extension, verify:

- [ ] Unit tests pass
- [ ] Culture-specific tests pass (if applicable)
- [ ] Edge cases covered (empty query, special chars, case variations)
- [ ] No regression in existing tests (`dotnet test`)

### 7.2 Running Tests

```bash
# All tests
dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj

# Specific extension tests
dotnet test --filter "FullyQualifiedName~NewProvider"
dotnet test --filter "FullyQualifiedName~Doi"
dotnet test --filter "FullyQualifiedName~de-DE"
```

---

## 8. Contribution Guidelines

### 8.1 Code Style

- Follow existing patterns in `SearchService.cs`
- Use `private static` methods for URL builders
- Use `Uri.EscapeDataString` for query encoding
- Use `StringComparison.OrdinalIgnoreCase` for keyword matching
- Add `RegexOptions.Compiled` to all regexes

### 8.2 Pull Request Requirements

- [ ] Tests for new functionality
- [ ] No breaking changes to existing tests
- [ ] Updated documentation (this file or relevant docs)
- [ ] Code follows existing style

### 8.3 Review Focus

Reviewers should check:
- URL encoding correctness
- Regex performance (compiled, no catastrophic backtracking)
- Culture handling consistency
- Test coverage for new paths

---

## 9. Extension Points Summary

| Extension Point | File | Method | Test File |
|-----------------|------|--------|-----------|
| Search Provider | `SearchService.cs` | `Get{Provider}Url` + keyword in `GetAutoUrlForMultiWordQuery` | `SearchServiceTests.cs` |
| Query Pattern | `SearchService.cs` | Regex field + check in `GetAutoUrl` | `SearchServiceTests.cs` |
| Culture | `SharedResources.{culture}.resx`, `IpCultureProvider.cs`, URL builders | Culture mapping + URL branches | `SearchServiceTests.cs` with `[SetUICulture]` |
| Domain Blacklist | `SearchService.cs` | Regex field + check in `ApplyDomainBlacklist` | `SearchServiceTests.cs` |
| Search Type | `SearchService.cs` | Case in `GetSearchUrl` + `Get{Type}Url` | `SearchServiceTests.cs` |
| Geolocation Provider | `GeolocationService.cs` | `GetCountryCodeAsync` implementation | Integration tests |

---

## 10. Future Extension Ideas

- **Plugin architecture** — Load providers from external assemblies
- **User preferences** — Per-user default search engine, blocked domains
- **Search history** — Local storage of recent searches
- **Keyboard shortcuts** — `!bang` syntax for direct provider access
- **API endpoint** — REST API for programmatic search URL generation
- **Browser extension** — Direct integration with browser search