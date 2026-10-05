# Architecture Deep Dive

Implementation-level architectural analysis of NuciSearch. This document complements the root [ARCHITECTURE.md](../ARCHITECTURE.md) by explaining *why* decisions were made, not just *what* exists.

## 1. Stateless URL Construction

### Design Decision

`SearchService` is a **stateless, static-method-heavy** class registered as a singleton. All URL builders are `private static` methods.

### Rationale

- **No per-request state** — URL construction is a pure function of `(query, searchType, culture)`. No instance fields needed.
- **Thread safety by construction** — Static methods with no shared mutable state are inherently thread-safe. Blazor Server's circuit model can invoke `GetSearchUrl` from any circuit without locks.
- **Testability** — Static methods are trivially testable without DI setup. The only injected dependency is `ILogger` (for the singleton instance), which is mocked in tests.
- **Culture dependency** — `CultureInfo.CurrentUICulture` is read at call time, not at construction. This means the same singleton instance correctly produces culture-specific URLs for different requests (each request sets its culture via `IpCultureProvider` before the service is called).

### Consequence

The singleton registration in `ServiceCollectionExtensions.cs` is technically unnecessary for correctness (the class has no mutable state), but it follows the DI convention and allows future extension (e.g., injecting configuration for provider URLs).

## 2. Pattern-Based Routing Hierarchy

### Decision

`GetAutoUrl` implements a **priority-ordered cascade** of pattern checks:

```
1. Image keywords → DuckDuckGo Images
2. Jira issue key → Jira
3. Rally issue key → Rally
4. Wikidata ID → Wikidata
5. Currency conversion → DuckDuckGo (normalised)
6. IP address query → DuckDuckGo
7. Multi-word with provider keyword → Provider-specific URL
8. Fallback → Text search (Brave/DuckDuckGo)
```

### Why This Order

- **Specificity first** — A query like `Q20717572` must match Wikidata before falling through to text search. A query like `AAP-123` must match Jira before keyword matching.
- **Image keywords before everything** — "cats image" should go to image search, not text search, even if "cats" could match a provider keyword.
- **IP address before multi-word** — "my ip address" is a 3-word query that would otherwise enter the multi-word keyword cascade. The IP pattern check prevents this.
- **Multi-word keyword matching last** — This is the most complex and least specific check, so it runs after all pattern-based checks.

### Causal Chain

```
User types "emag laptop"
  → NormaliseQuery (FormKC, strip zero-width, collapse whitespace)
  → Deobfuscate (NuciText.Obfuscation)
  → GetAutoUrl
    → Not image keyword
    → Not Jira/Rally/Wikidata/Currency/IP
    → words.Count() >= 2 → GetAutoUrlForMultiWordQuery
      → ContainsKeyword(words, "emag") → true
      → StripKeyword(words, "emag") → "laptop"
      → GetEmagUrl("laptop") → "https://emag.ro/search/laptop"
```

## 3. Keyword Normalisation for Diacritics

### Problem

Users type "Cărturești" (with diacritics) but the keyword is "carturesti" (without). Similarly, "mömax" vs "momax" vs "moemax".

### Solution

`NormaliseKeyword` performs:
1. **FormD decomposition** — Splits composed characters into base + combining marks
2. **Strip NonSpacingMark** — Removes combining diacritical marks (e.g., ă → a + U+0306, strip U+0306)
3. **FormC recomposition** — Recomposes any remaining composed characters
4. **"oe" → "o" replacement** — Handles the Romanian "ș" → "s" and "ț" → "t" edge case where FormD decomposition of "ș" yields "s" + U+0325 (combining breve below), but "oe" ligature handling is needed for certain provider names

### Why Not Just `ToLowerInvariant`?

`ToLowerInvariant` does not strip diacritics. "Cărturești".ToLowerInvariant() = "cărturești" ≠ "carturesti". The FormD/NonSpacingMark/FormC pipeline is the standard .NET approach for accent-insensitive comparison.

## 4. Domain Blacklist Architecture

### Decision

`ApplyDomainBlacklist` appends `-site:domain` exclusions to text search queries based on game/topic keyword patterns.

### Rationale

- **Quality control** — Certain domains (Fandom wikis, Neoseeker, etc.) produce low-quality or outdated results for specific game queries. Excluding them improves result relevance.
- **Composability** — Multiple blacklists can apply simultaneously (e.g., "gta mission guide" triggers 9 domain exclusions).
- **Text search only** — Blacklists only apply to `GetTextSearch` (Brave/DuckDuckGo), not to provider-specific URLs. This is because provider URLs are already targeted and don't need domain filtering.

### Pattern Matching

Each blacklist uses a `Regex` pattern compiled at class-initialisation time (static field). The patterns match game/topic keywords in the query:

| Pattern | Triggers | Domains Excluded |
|---------|----------|-----------------|
| `fandomKeywordsPattern` | "minecraft", "terraria", "osrs", "factorio", "warhammer", "wh40k", "40k", "skyrim", "baldur" | `fandom.com` |
| `arcenservKeywordsPattern` | "terraria" | `arcenserv.info` |
| `fextralifeKeywordsPattern` | "bg3", "borderlands", "skyrim", "baldur" | `wiki.fextralife.com` |
| `gtaKeywordsPattern` | "gta", "grand theft auto" | 9 domains |
| `aSongOfIceAndFireWikiKeywordsPattern` | "asoiaf", "song of ice and fire", "game of thrones", "house of the dragon" | 6 domains |

## 5. Random Instance Selection

### Decision

Reddit and Wikipedia searches use `Random.Shared.Next()` to select from a list of instances.

### Rationale

- **Load distribution** — No single instance bears all traffic.
- **Resilience** — If one instance is down, the next request may select a working one.
- **No health checking** — The app does not verify instance availability before selection. This is a deliberate trade-off: health checks would add latency and complexity for a self-hosted app where the operator can monitor instances.

### Why `Random.Shared`?

`Random.Shared` is a thread-safe singleton introduced in .NET 6. It avoids the allocation cost of creating a new `Random` instance per call, and avoids the thread-safety issues of a shared `Random` instance.

## 6. Blazor Server Render Mode

### Decision

`Home.razor` uses `InteractiveServerRenderMode(prerender: false)`.

### Rationale

- **No prerendering** — The search form has no server-side state to prerender. Prerendering would waste a circuit slot for a page that immediately redirects.
- **Interactive server** — The form needs server-side interactivity for `@onsubmit` handling and `NavigationManager` access.
- **`prerender: false`** — Prevents the "double-render" issue where the page would render once on the server (without user input) and again on the client.

### Circuit Lifecycle

```
1. Browser requests /
2. Server renders Home.razor (no prerender)
3. Blazor circuit established (SignalR)
4. User types query, clicks Search
5. HandleSearch() runs on server
6. SearchService.GetSearchUrl() called
7. NavigationManager.NavigateTo(targetUrl, ForceLoad: true)
8. Browser navigates to external URL
9. Circuit can be disposed
```

## 7. OpenSearch Endpoint

### Decision

The OpenSearch XML is generated dynamically via `MapGet("/opensearch.xml")` rather than served as a static file.

### Rationale

- **Localisation** — The `<Description>` element uses `IStringLocalizer<SharedResources>` to provide the description in the user's culture.
- **Single source of truth** — The URL template (`https://search.nuilandia.ro?q={searchTerms}`) is defined in one place.
- **Static file also exists** — `wwwroot/opensearch.xml` is a static fallback for environments where the dynamic endpoint is not available.

## 8. Configuration Binding

### Decision

`NuciLoggerSettings` is bound from configuration via `configuration.Bind()` and registered as a singleton.

### Rationale

- **NuciLog integration** — The `NuciLogger` (registered as `ILogger`) reads its settings from the bound `NuciLoggerSettings` instance.
- **Operator control** — Log file path and file output enablement are configurable without code changes.
- **Singleton** — Settings are immutable after startup; a singleton is appropriate.

## 9. Exception Handling Strategy

### Decision

- **Development** — No exception handler middleware; exceptions propagate to the Blazor error UI.
- **Production** — `UseExceptionHandler("/Error")` catches unhandled exceptions and redirects to the error page.
- **Status code pages** — `UseStatusCodePagesWithReExecute("/not-found")` handles 404s.

### Rationale

- **Development** — Full stack traces are visible in the browser console and the Blazor error UI.
- **Production** — Users see a generic error page with a Request ID for correlation. No stack traces are exposed.
- **Request ID** — `Error.razor` reads `Activity.Current?.Id ?? HttpContext?.TraceIdentifier` to provide a correlation ID.

## 10. Anti-Forgery

### Decision

`app.UseAntiforgery()` is applied globally.

### Rationale

- **Blazor Server requirement** — Interactive server components require anti-forgery tokens for form submissions.
- **No explicit token rendering** — Blazor's `@onsubmit` with `@onsubmit:preventDefault` handles token validation automatically.