# Testing Strategy

NuciSearch uses NUnit for unit testing with Moq for mocking. The test suite covers the core search routing logic, geolocation, and edge cases.

## 1. Test Project Structure

```
NuciSearch.UnitTests/
├── NuciSearch.UnitTests.csproj
└── Services/
    └── SearchServiceTests.cs
```

### Dependencies

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="NUnit" Version="4.3.2" />
<PackageReference Include="NUnit3TestAdapter" Version="5.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="NuciLog.Core" Version="3.0.0" />
<PackageReference Include="NuciText.Obfuscation" Version="1.1.1" />
```

## 2. Test Organisation

### SearchServiceTests.cs

The single test file contains **192 tests** organised into logical sections:

| Section | Test Count | Coverage |
|---------|------------|----------|
| Empty / whitespace | 2 | Empty query handling |
| Search types | 7 | `images`, `maps`, `torrents`, `videos`, `text`, `auto` single-word |
| Image keywords | 18 | All image intent keywords |
| WikiData | 4 | Wikidata ID and keyword routing |
| Jira | 5 | Jira issue key patterns |
| Rally | 4 | Rally issue key patterns |
| Currency conversion | 10 | Various currency formats and normalisations |
| IP address | 6 | IP query patterns |
| Keyword redirects | ~80 | All provider keywords |
| Domain blacklists | ~25 | Game/topic domain exclusions |
| Deobfuscation | 4 | NuciText.Obfuscation integration |
| Query normalisation | 3 | Whitespace, zero-width chars |
| Text search fallback | 1 | Multi-word no-keyword fallback |

### Test Naming Convention

```
Given[Precondition]_When[Action]_Then[ExpectedOutcome]
```

Examples:
- `GivenEmptyQuery_WhenGettingSearchUrl_ThenReturnsEmptyString`
- `GivenImagesSearchType_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl`
- `GivenCurrencyQueryWithLei_WhenGettingSearchUrl_ThenNormalisesLeiToRon`

## 3. Test Setup

```csharp
[SetUp]
public void SetUp()
{
    Mock<ILogger> loggerMock = new();
    searchService = new SearchService(loggerMock.Object);
}
```

- **Logger mocking:** `ILogger` is mocked to verify logging calls
- **No DI container:** Tests instantiate `SearchService` directly
- **Culture control:** `[SetUICulture("ro-RO")]` and `[SetUICulture("en-GB")]` attributes set `CultureInfo.CurrentUICulture` for culture-aware tests

## 4. Key Test Patterns

### 4.1 Search Type Tests

```csharp
[Test]
public void GivenImagesSearchType_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
    => Assert.That(
        searchService.GetSearchUrl("cats", "images"),
        Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));

[Test]
[SetUICulture("ro-RO")]
public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleRoMapsUrl()
    => Assert.That(
        searchService.GetSearchUrl("london", "maps"),
        Is.EqualTo("https://google.ro/maps/search/london"));
```

### 4.2 Pattern Matching Tests

```csharp
[Test]
public void GivenJiraAapQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
    => Assert.That(
        searchService.GetSearchUrl("AAP-123", "auto"),
        Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));

[Test]
public void GivenLowercaseJiraQuery_WhenGettingSearchUrl_ThenReturnsJiraUrlUppercased()
    => Assert.That(
        searchService.GetSearchUrl("aap-123", "auto"),
        Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));
```

### 4.3 Currency Normalisation Tests

```csharp
[Test]
public void GivenCurrencyQueryWithLei_WhenGettingSearchUrl_ThenNormalisesLeiToRon()
    => Assert.That(
        searchService.GetSearchUrl("100 lei in euro", "auto"),
        Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));

[Test]
public void GivenCurrencyQueryWithRomanianInPreposition_WhenGettingSearchUrl_ThenNormalisesIn()
    => Assert.That(
        searchService.GetSearchUrl("100 RON \u00een EUR", "auto"),
        Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));
```

### 4.4 Keyword Redirect Tests

```csharp
[Test]
public void GivenEmagKeyword_WhenGettingSearchUrl_ThenReturnsEmagUrl()
    => Assert.That(
        searchService.GetSearchUrl("emag laptop", "auto"),
        Is.EqualTo("https://emag.ro/search/laptop"));

[Test]
[SetUICulture("ro-RO")]
public void GivenIkeaKeyword_WhenGettingSearchUrl_ThenReturnsIkeaRoUrl()
    => Assert.That(
        searchService.GetSearchUrl("ikea scaun", "auto"),
        Is.EqualTo("https://ikea.com/ro/ro/search/?q=scaun"));
```

### 4.5 Domain Blacklist Tests

```csharp
[Test]
public void GivenMinecraftQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
    => Assert.That(
        searchService.GetSearchUrl("minecraft building", "text"),
        Does.Contain("fandom"));

[Test]
public void GivenGtaQuery_WhenGettingTextSearchUrl_ThenIncludesRequestedDomainBlacklists()
    => Assert.That(
        searchService.GetSearchUrl("gta mission guide", "text"),
        Does.Contain("grandtheftwiki.com")
            .And.Contain("gta.fandom.com")
            .And.Contain("wikigta.org")
            .And.Contain("gtastarsandstripes.miraheze.org")
            .And.Contain("neoseeker.com")
            .And.Contain("rockstargames.fandom.com")
            .And.Contain("gtaboom.com")
            .And.Contain("sportskeeda.com")
            .And.Contain("gta5wiki.com"));
```

### 4.6 Deobfuscation Tests

```csharp
[Test]
public void GivenObfuscatedQuery_WhenGettingSearchUrl_ThenDeobfuscatesBeforeSearching()
{
    INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
    NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
    string obfuscatedQuery = obfuscator.Obfuscate("cats", options);

    string result = searchService.GetSearchUrl(obfuscatedQuery, "auto");

    Assert.That(
        result,
        Does.StartWith("https://search.brave.com/search?q=")
            .Or.StartWith("https://duckduckgo.com/?q="));
}
```

### 4.7 Query Normalisation Tests

```csharp
[Test]
public void GivenQueryWithZeroWidthCharacters_WhenGettingSearchUrl_ThenStripsZeroWidthCharacters()
{
    string result1 = searchService.GetSearchUrl("emag\u200B laptop", "auto");
    string result2 = searchService.GetSearchUrl("emag laptop", "auto");

    Assert.That(result1, Is.EqualTo(result2));
}
```

## 5. Running Tests

### Command Line

```bash
# Run all tests
dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj

# Run with coverage
dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj --collect:"XPlat Code Coverage"

# Run specific test
dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj --filter "FullyQualifiedName~GivenEmagKeyword"

# Run tests for specific culture
dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj --filter "FullyQualifiedName~ro-RO"
```

### Test Output

```
Test Run Successful.
Total tests: 192
     Passed: 192
     Failed: 0
     Skipped: 0
```

## 6. Coverage Areas

### Covered

| Area | Coverage |
|------|----------|
| SearchService.GetSearchUrl | 100% of public API |
| All search types | All 6 types tested |
| All pattern matching | Jira, Rally, Wikidata, Currency, IP, Image keywords |
| All provider keywords | ~50 providers tested |
| Domain blacklists | All 12 blacklist patterns tested |
| Culture-aware URLs | Maps, Firefox, IKEA, Wikipedia |
| Deobfuscation | Integration with NuciText.Obfuscation |
| Query normalisation | Whitespace, zero-width, FormKC |
| Logging | Verified via mock |

### Not Covered (by design)

| Area | Reason |
|------|--------|
| GeolocationService | Requires HTTP mocking; integration test territory |
| IpCultureProvider | Requires HttpContext mocking; integration test territory |
| Blazor Components | UI tests require browser automation (Playwright) |
| Program.cs | Application startup; integration test territory |
| OpenSearch endpoint | HTTP endpoint test; integration test territory |

## 7. Adding New Tests

### For New Provider Keywords

```csharp
[Test]
public void GivenNewProviderKeyword_WhenGettingSearchUrl_ThenReturnsNewProviderUrl()
    => Assert.That(
        searchService.GetSearchUrl("newprovider query", "auto"),
        Is.EqualTo("https://newprovider.com/search?q=query"));
```

### For New Pattern Matching

```csharp
[Test]
public void GivenNewPattern_WhenGettingSearchUrl_ThenRoutesCorrectly()
    => Assert.That(
        searchService.GetSearchUrl("PATTERN-123", "auto"),
        Is.EqualTo("https://expected.url/PATTERN-123"));
```

### For New Domain Blacklist

```csharp
[Test]
public void GivenNewGameQuery_WhenGettingTextSearchUrl_ThenIncludesNewBlacklist()
    => Assert.That(
        searchService.GetSearchUrl("newgame guide", "text"),
        Does.Contain("newgame.fandom.com"));
```

### For Culture-Aware Behaviour

```csharp
[Test]
[SetUICulture("de-DE")]
public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGermanMapsUrl()
    => Assert.That(
        searchService.GetSearchUrl("berlin", "maps"),
        Is.EqualTo("https://google.de/maps/search/berlin"));
```

## 8. Test Data

### Test Values

| Category | Values Used |
|----------|-------------|
| Queries | `"cats"`, `"emag laptop"`, `"AAP-123"`, `"Q42"`, `"100 USD in EUR"` |
| IPs | `"127.0.0.1"`, `"::1"`, `"192.168.1.1"`, `"8.8.8.8"` |
| Cultures | `"en-GB"`, `"ro-RO"` |
| Search types | `"auto"`, `"text"`, `"images"`, `"maps"`, `"torrents"`, `"videos"` |

### Obfuscation Test Data

```csharp
INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
string obfuscatedQuery = obfuscator.Obfuscate("cats", options);
```

The seed `123456789` ensures deterministic obfuscation for reproducible tests.

## 9. CI/CD Integration

### GitHub Actions (dotnet.yml)

```yaml
- name: Test
  run: dotnet test NuciSearch.UnitTests/NuciSearch.UnitTests.csproj --no-restore --verbosity normal
```

### Test Results

- Tests run on every push and PR
- Must pass for merge
- Coverage reported via Codecov (if configured)

## 10. Debugging Failed Tests

### Common Failure Patterns

| Failure | Likely Cause |
|---------|--------------|
| `Expected: "https://emag.ro/search/laptop"` but was `"https://emag.ro/search/laptop%20"` | Trailing space in query not trimmed |
| `Expected: "https://google.ro/maps/search/london"` but was `"https://google.co.uk/maps/search/london"` | Culture not set correctly |
| `Expected: "https://duckduckgo.com/?q=100%20RON%20in%20EUR"` but was `"https://duckduckgo.com/?q=100%20lei%20in%20EUR"` | Currency normalisation not applied |
| `Does.Contain("fandom")` failed | Blacklist pattern not matching |

### Debugging Tips

1. **Run single test with verbose output:**
   ```bash
   dotnet test --filter "FullyQualifiedName=GivenEmagKeyword" --logger "console;verbosity=detailed"
   ```

2. **Add temporary logging to SearchService:**
   ```csharp
   logger.Info(NuciSearchOperation.Search, OperationStatus.Started,
       [new(NuciSearchLogInfoKey.Query, query)]);
   ```

3. **Check regex patterns** in `SearchService.cs` static fields

4. **Verify culture** with `[SetUICulture]` attribute on test method