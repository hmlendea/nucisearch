# Search Routing Logic

Complete specification of how NuciSearch routes a user query to a target URL. This is the core business logic of the application.

## 1. Entry Point

```csharp
string GetSearchUrl(string rawQuery, string searchType)
```

**Parameters:**
- `rawQuery` — The raw user input (may contain obfuscation, diacritics, extra whitespace)
- `searchType` — One of: `auto`, `text`, `images`, `torrents`, `videos`, `maps`

**Returns:** A fully-formed URL string, or `string.Empty` if the query is empty/whitespace.

## 2. Pre-Processing Pipeline

### Step 1: Empty Check

```csharp
if (string.IsNullOrWhiteSpace(rawQuery))
    return string.Empty;
```

### Step 2: Normalisation

`NormaliseQuery(rawQuery)` performs:
1. **FormKC normalisation** — Compatibility decomposition followed by canonical composition. Converts ligatures (e.g., "ﬃ" → "ffi") and compatibility characters to their canonical forms.
2. **Zero-width character removal** — Strips `U+200B` (zero-width space), `U+200C` (zero-width non-joiner), `U+200D` (zero-width joiner), and similar invisible characters that could be used to bypass pattern matching.
3. **Whitespace collapsing** — Replaces runs of whitespace (spaces, tabs, newlines) with a single space.
4. **Trimming** — Removes leading/trailing whitespace.

### Step 3: Deobfuscation

```csharp
query = NuciTextObfuscator.Deobfuscate(query);
```

Uses `NuciText.Obfuscation` to reverse text obfuscation (e.g., character substitution, transposition). This allows users to search for obfuscated terms and still get correct routing.

## 3. Search Type Routing

After pre-processing, the query is routed based on `searchType`:

| Search Type | Handler | Description |
|-------------|---------|-------------|
| `images` | `GetImagesUrl` | Always → DuckDuckGo Images |
| `torrents` | `GetTorrentsUrl` | Always → Yandex with " Torrent" suffix |
| `videos` | `GetVideosUrl` | Always → yewtu.be (Invidious instance) |
| `maps` | `GetMapsUrl` | Culture-aware → Google Maps (ro/google.co.uk) |
| `text` | `GetTextSearch` | Brave or DuckDuckGo with domain blacklists |
| `auto` | `GetAutoUrl` | Pattern-based intelligent routing |

### Case Insensitivity

`searchType` is compared case-insensitively via `string.Equals(..., StringComparison.OrdinalIgnoreCase)`.

## 4. Auto Mode Routing

`GetAutoUrl` is the most complex routing path. It applies a priority-ordered cascade of checks:

### 4.1 Image Keywords

```csharp
if (imageSearchKeywordsPattern.IsMatch(query))
    return GetDuckDuckGoImagesUrl(query);
```

**Pattern:** Matches common image-related keywords: `logo`, `portrait`, `image`, `wallpaper`, `background`, `picture`, `photo`, `photos`, `photograph`, `photographs`, `illustration`, `illustrations`, `icon`, `icons`, `clipart`, `thumbnail`, `thumbnails`.

**Case insensitive.** If the query contains any of these keywords, it routes to DuckDuckGo Images regardless of other patterns.

### 4.2 Jira Issue Keys

```csharp
if (jiraPattern.IsMatch(query))
    return GetJiraUrl(query);
```

**Pattern:** Matches `[A-Z]{2,10}-\d+` (2-10 uppercase letters, hyphen, digits).

**URL:** `https://worldpay.atlassian.net/browse/{QUERY_UPPERCASE}`

**Note:** The query is uppercased before URL construction. Lowercase Jira keys like `aap-123` are normalised to `AAP-123`.

### 4.3 Rally Issue Keys

```csharp
if (rallyPattern.IsMatch(query))
    return GetRallyUrl(query);
```

**Pattern:** Matches `^(DE|F|US)\d{6,8}$` — Rally's issue ID formats:
- `DE` prefix: 6-8 digits (e.g., `DE123456`, `DE12345678`)
- `F` prefix: 6-7 digits (e.g., `F123456`, `F1234567`)
- `US` prefix: 8 digits (e.g., `US12345678`, `US123456`)

**URL:** `https://rally1.rallydev.com/#/search?keywords={QUERY}`

### 4.4 Wikidata IDs

```csharp
if (wikiDataPattern.IsMatch(query))
    return GetWikiDataUrl(query);
```

**Pattern:** Matches `^Q\d+$` — Wikidata entity IDs (e.g., `Q42`, `Q20717572`).

**URL:** `https://wikidata.org/wiki/{QUERY_UPPERCASE}`

**Note:** Lowercase `q42` is normalised to `Q42`.

### 4.5 Currency Conversion

```csharp
if (currencyPattern.IsMatch(query))
    return BuildCurrencySearchUrl(query);
```

**Pattern:** Matches currency conversion queries like `100 USD in EUR`, `50 dollars to RON`, `100 lei in euro`.

**Normalisation in `BuildCurrencySearchUrl`:**
- `în` → `in` (Romanian preposition)
- `lei`/`leu` → `RON`
- `euros`/`euro` → `EUR`
- `dollars`/`dollar`/`dolari`/`dolar` → `USD`
- `lira`/`liră`/`lire` → `GBP`
- Any remaining 3-letter code → uppercased

**URL:** `https://duckduckgo.com/?q={NORMALISED_QUERY}`

### 4.6 IP Address Queries

```csharp
if (ipAddressQueryPattern.IsMatch(query))
    return $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}";
```

**Pattern:** Matches queries like `my ip`, `my ip address`, `current ip`, `current ip address` (case insensitive).

**URL:** DuckDuckGo with the original query (not normalised).

### 4.7 Multi-Word Keyword Routing

If the query has 2+ words and none of the above patterns matched, `GetAutoUrlForMultiWordQuery` is called. This method checks for provider keywords:

```csharp
if (words.Count() >= 2)
    return GetAutoUrlForMultiWordQuery(query, words);
```

#### Keyword Matching

`ContainsKeyword(words, keyword)` checks if any word in the query matches the keyword using `KeywordsAreEqual`, which:
1. Normalises both words via `NormaliseKeyword` (diacritic stripping)
2. Compares case-insensitively

#### Keyword Stripping

`StripKeyword(words, keyword)` removes the matching keyword from the word list and returns the remaining words joined by spaces.

#### Provider Keyword Cascade

The method checks keywords in alphabetical order. For each match:
1. Strip the keyword from the query
2. Call the corresponding URL builder with the remaining query

**Special cases:**
- **fdroid/f-droid** — Both variants are stripped
- **firefox extension(s)** — Both singular and plural forms are stripped
- **mc wiki / minecraft wiki** — Both forms are stripped
- **mc head(s) / minecraft head(s)** — All variants are stripped
- **mc schematic(s) / minecraft schematic(s)** — All variants are stripped
- **nexusmods / nexus mods** — Both forms are stripped
- **play store / playstore** — Both forms are stripped
- **spy-shop / spyshop / spy-shop.ro / spyshop.ro** — All variants are stripped
- **uesp / elder scrolls wiki / eso wiki / morrowind wiki / oblivion wiki / skyrim wiki / tes wiki / the elder scrolls wiki** — All variants are stripped

### 4.8 Fallback

If no keyword matches in the multi-word cascade, the query falls through to `GetTextSearch`.

## 5. Text Search

```csharp
private static string GetTextSearch(string query)
{
    string encodedQuery = Uri.EscapeDataString(ApplyQueryCustomisations(query));
    string[] searchEngines = [
        $"https://search.brave.com/search?q={encodedQuery}",
        $"https://duckduckgo.com/?q={encodedQuery}",
    ];
    return searchEngines[Random.Shared.Next(searchEngines.Length)];
}
```

### Domain Blacklist Application

`ApplyQueryCustomisations(query)` calls `ApplyDomainBlacklist(query)`, which appends `-site:domain` exclusions based on game/topic keyword patterns.

### Random Engine Selection

Brave Search and DuckDuckGo are selected randomly to distribute load and provide resilience.

## 6. Culture-Aware Routing

Three URL builders are culture-aware:

| Builder | Culture | URL Pattern |
|---------|---------|-------------|
| `GetFirefoxExtensionsUrl` | `ro-RO` | `https://addons.mozilla.org/ro/firefox/search/?q={query}` |
| `GetFirefoxExtensionsUrl` | other | `https://addons.mozilla.org/en-GB/firefox/search/?q={query}` |
| `GetGoogleMapsUrl` | `ro-RO` | `https://google.ro/maps/search/{query}` |
| `GetGoogleMapsUrl` | other | `https://google.co.uk/maps/search/{query}` |
| `GetIkeaUrl` | `ro-RO` | `https://ikea.com/ro/ro/search/?q={query}` |
| `GetIkeaUrl` | other | `https://ikea.com/gb/en/search/?q={query}` |
| `GetWikiPediaUrl` | `ro-RO` | `https://ro.wikipedia.org/...` or wikiless instance |
| `GetWikiPediaUrl` | other | `https://en.wikipedia.org/...` or wikiless instance |

Culture is determined by `CultureInfo.CurrentUICulture.Name`, which is set by `IpCultureProvider` based on the user's IP address.

## 7. URL Encoding

All query parameters are encoded via `Uri.EscapeDataString()`, which:
- Encodes spaces as `%20`
- Encodes special characters (`+`, `&`, `=`, etc.)
- Does **not** encode `/` (safe for URLs)

## 8. Complete Provider List

### E-commerce (Romanian)
| Keyword | URL Builder | URL Pattern |
|---------|-------------|-------------|
| aliexpress | `GetAliExpressUrl` | `https://www.aliexpress.com/w/wholesale-{query}.html?spm=a2g0o.detail.search.0` |
| altex | `GetAltexUrl` | `https://altex.ro/cauta/?q={query}` |
| animax | `GetAnimaxUrl` | `https://animax.ro/search?q={query}` |
| auchan | `GetAuchanUrl` | `https://auchan.ro/{query}` |
| carturesti | `GetCarturestiUrl` | `https://carturesti.ro/product/search/{query}` |
| decathlon | `GetDecathlonUrl` | `https://decathlon.ro/search?Ntt={query}` |
| dedeman | `GetDedemanUrl` | `https://dedeman.ro/ro/catalogsearch/result/v2?q={query}` |
| dex / dexonline | `GetDexOnlineUrl` | `https://dexonline.ro/definitie/{query}` |
| digi24 | `GetDigi24Url` | `https://digi24.ro/cautare?q={query}` |
| emag | `GetEmagUrl` | `https://emag.ro/search/{query}` |
| evomag | `GetEvomagUrl` | `https://evomag.ro/?sn.q={query}` |
| flanco | `GetFlancoUrl` | `https://flanco.ro/catalogsearch/result/?q={query}` |
| flip.ro | `GetFlipRoUrl` | `https://flip.ro/magazin/?search={query}` |
| g2a | `GetG2aUrl` | `https://g2a.com/search?query={query}` |
| hornbach | `GetHornbachUrl` | `https://hornbach.ro/s/{query}` |
| ikea | `GetIkeaUrl` | Culture-aware (see above) |
| jysk | `GetJyskUrl` | `https://jysk.ro/search?query={query}` |
| leroy merlin | `GetLeroyMerlinUrl` | `https://leroymerlin.ro/produse/search/{query}` |
| lidl | `GetLidlUrl` | `https://lidl.ro/q/search?q={query}` |
| moemax / mömax | `GetMoemaxUrl` | `https://moemax.ro/s/?s={query}` |
| pcgarage | `GetPcGarageUrl` | `https://pcgarage.ro/cauta/{query}` |
| sinsay | `GetSinsayUrl` | `https://sinsay.com/ro/ro/?query={query}` |
| spy-shop / spyshop | `GetSpyShopUrl` | `https://spy-shop.ro/catalogsearch/result/?q={query}&o=relevance` |

### Tech & Software
| Keyword | URL Builder | URL Pattern |
|---------|-------------|-------------|
| appstore / app store / apple store | `GetAppStoreUrl` | `https://apple.com/uk/search/{query}?src=globalnav` |
| arch wiki | `GetArchWikiUrl` | `https://wiki.archlinux.com/index.php?search={query}` |
| fdroid / f-droid | `GetFdroidUrl` | `https://search.f-droid.org/?q={query}` |
| firefox extension(s) | `GetFirefoxExtensionsUrl` | Culture-aware |
| flathub | `GetFlatHubUrl` | `https://flathub.org/apps/search/{query}` |
| github | `GetGitHubUrl` | Text search with `site:github.com` |
| mc head(s) / minecraft head(s) | `GetMinecraftHeadsUrl` | `https://minecraft-heads.com/custom-heads/search?searchterm={query}` |
| mc schematic(s) / minecraft schematic(s) | `GetPlanetMinecraftSchematicsUrl` | `https://planetminecraft.com/projects/?keywords={query}` |
| mc wiki / minecraft wiki | `GetMinecraftWikiUrl` | `https://minecraft.wiki/?search={query}` |
| namemc | `GetNameMcUrl` | `https://namemc.com/search?q={query}` |
| nexusmods / nexus mods | `GetNexusModsUrl` | `https://nexusmods.com/search?keyword={query}` |
| nuget | `GetNuGetUrl` | `https://nuget.org/packages?q={query}` |
| play store / playstore | `GetPlayStoreUrl` | `https://play.google.com/store/search?q={query}` |
| plex | `GetPlexUrl` | `https://app.plex.tv/desktop/#!/search?pivot=top&query={query}` |
| protondb | `GetProtonDbUrl` | `https://protondb.com/search?q={query}` |
| spigot | `GetSpigotUrl` | `https://spigotmc.org/search/294718421/?q={query}&o=relevance` |
| steamdb | `GetSteamDbUrl` | `https://steamdb.info/search/?a=all&q={query}` |

### Social & Media
| Keyword | URL Builder | URL Pattern |
|---------|-------------|-------------|
| facebook | `GetFacebookUrl` | Text search with `site:facebook.com` |
| instagram | `GetInstagramUrl` | `https://instagram.com/popular/{query}` |
| linkedin | `GetLinkedinUrl` | `https://linkedin.com/search/results/all/?keywords={query}` |
| netflix | `GetNetflixUrl` | `https://netflix.com/search?q={query}` |
| odysee | `GetOdyseeUrl` | `https://odysee.com/$/search?q={query}` |
| pinterest | `GetPinterestUrl` | `https://pinterest.com/search/pins/?q={query}` |
| reddit | `GetRedditUrl` | Random redlib instance |
| trip advisor | `GetTripadvisorUrl` | `https://tripadvisor.com/Search?q={query}` |
| tvdb / thetvdb | `GetTvdbUrl` | `https://thetvdb.com/search?query={query}` |
| vinted | `GetVintedUrl` | `https://vinted.com/catalog?search_text={query}` |
| wikipedia | `GetWikiPediaUrl` | Culture-aware, random instance |
| youtube | `GetYouTubeUrl` | `https://yewtu.be/search?q={query}` |

### Gaming Wikis & Databases
| Keyword | URL Builder | URL Pattern |
|---------|-------------|-------------|
| boobpedia | `GetBoobpediaUrl` | `https://boobpedia.com/wiki/index.php?title=Special%3ASearch&search={query}&go=Go` |
| gog | `GetGogUrl` | `https://gog.com/en/games?query={query}` |
| imdb | `GetImdbUrl` | `https://libremdb.iket.me/find?q={query}` |
| microwiki | `GetMicroWikiUrl` | `https://micronations.wiki/index.php?search={query}&title=Special%3ASearch&go=Go` |
| moddb | `GetModDbUrl` | `https://moddb.com/search?q={query}` |
| planet minecraft | `GetPlanetMinecraftUrl` | `https://planetminecraft.com/resources/?keywords={query}` |
| rtings | `GetRtingsUrl` | `https://rtings.com/search?q={query}` |
| uesp / skyrim wiki / eso wiki / etc. | `GetUespUrl` | `https://en.uesp.net/wiki/Special:Search?search={query}` |
| wikidata | `GetWikiDataSearchUrl` | `https://wikidata.org/w/index.php?search={query}` |

### Special Formats
| Pattern | URL Builder | URL Pattern |
|---------|-------------|-------------|
| Jira key (`AAP-123`) | `GetJiraUrl` | `https://worldpay.atlassian.net/browse/{QUERY_UPPERCASE}` |
| Rally key (`DE123456`) | `GetRallyUrl` | `https://rally1.rallydev.com/#/search?keywords={QUERY}` |
| Wikidata ID (`Q42`) | `GetWikiDataUrl` | `https://wikidata.org/wiki/{QUERY_UPPERCASE}` |
| Currency (`100 USD in EUR`) | `BuildCurrencySearchUrl` | `https://duckduckgo.com/?q={NORMALISED_QUERY}` |
| IP query (`my ip`) | — | `https://duckduckgo.com/?q={QUERY}` |

## 9. Regex Patterns

All patterns are compiled as static readonly fields in `SearchService` with `RegexOptions.Compiled` and `RegexOptions.IgnoreCase` (except `rallyPattern` which is case-sensitive):

| Field | Pattern | Purpose |
|-------|---------|---------|
| `imageSearchKeywordsPattern` | `\b(?:logo\|portrait\|image\|wallpaper\|background\|picture\|photo\|photos\|photograph\|photographs\|illustration\|illustrations\|icon\|icons\|clipart\|thumbnail\|thumbnails)\b` | Image intent detection |
| `jiraPattern` | `^(?:AAP\|AV\|AND\|CP)-\d+$` | Jira issue key (AAP, AV, AND, CP prefixes) |
| `rallyPattern` | `^(?:DE\|F\|US)[0-9]{6,8}$` | Rally issue key (DE/F/US prefixes, 6-8 digits) |
| `wikiDataPattern` | `^Q\d+$` | Wikidata entity ID |
| `currencyPattern` | `^\d[\d.,]*\s+\w+\s+(?:in\|în\|to)\s+\w+$` | Currency conversion query |
| `ipAddressQueryPattern` | `^(?:my\|current)\s+ip(?:\s+address)?$` | IP address lookup intent |
| `zeroWidthCharactersPattern` | `[\u200B-\u200D\uFEFF]` | Zero-width character removal |
| `whitespacePattern` | `\s+` | Whitespace collapsing |
| `arcenservKeywordsPattern` | `\b(?:terraria)\b` | Terraria → arcenserv.info blacklist |
| `fextralifeKeywordsPattern` | `\b(?:baldur\|bg3\|borderlands\|don't\s*starve\|eso\|elder\s*scrolls\|skyrim\|tes)\b` | Fextralife domain blacklist |
| `fandomKeywordsPattern` | `\b(?:40k\|baldur\|bg3\|don't\s*starve\|eso\|factorio\|mc\|minecraft\|terraria\|elder\s*scrolls\|osrs\|skyrim\|tes\|runescape\|puzzle\s*pirates\|ypp\|game\s*of\s*thrones\|warhammer\|wh40k)\b` | Fandom domain blacklist |
| `huijiwikiKeywordsPattern` | `\b(?:borderlands)\b` | Huijiwiki domain blacklist |
| `neoseekerKeywordsPattern` | `\b(?:osrs\|runescape\|terraria)\b` | Neoseeker domain blacklist |
| `strategywikiKeywordsPattern` | `\b(?:osrs\|runescape)\b` | StrategyWiki domain blacklist |
| `aSongOfIceAndFireWikiKeywordsPattern` | `\b(?:asoiaf\|a\s*song\s*of\s*ice\s*and\s*fire\|game\s*of\s*thrones\|house\s*of\s*the\s*dragon)\b` | ASOIAF wiki domain blacklist |
| `stellarisKeywordsPattern` | `\b(?:stellaris)\b` | Stellaris Fandom domain blacklist |
| `heartsOfIronKeywordsPattern` | `\b(?:hoi\s*(?:4\|iv)\|hearts?\s*of\s*iron\s*(?:4\|iv))\b` | Hearts of Iron Fandom domain blacklist |
| `gtaKeywordsPattern` | `\b(?:gta\|grand\s*theft\s*auto\|grand\s*theft)\b` | GTA domain blacklist |
| `kingdomComeDeliveranceKeywordsPattern` | `\b(?:kcd(?:\s*2\|\s*ii)?\|kingdom\s*come(?::)?\s*deliverance(?:\s*(?:2\|ii))?)\b` | KCD domain blacklist |

## 10. Domain Blacklist Details

### GTA Blacklist (9 domains)
Triggered by: `gta`, `grand theft auto`

Excludes:
- `grandtheftwiki.com`
- `gta.fandom.com`
- `gta5wiki.com`
- `gtaboom.com`
- `gtastarsandstripes.miraheze.org`
- `neoseeker.com`
- `rockstargames.fandom.com`
- `sportskeeda.com`
- `wikigta.org`

### ASOIAF Blacklist (6 domains)
Triggered by: `asoiaf`, `song of ice and fire`, `game of thrones`, `house of the dragon`

Excludes:
- `gameofthrones.fandom.com`
- `hbo-tv.fandom.com`
- `hieloyfuego.fandom.com`
- `iceandfire.fandom.com`
- `listofdeaths.fandom.com`
- `wikiofthrones.com`

### KCD Blacklist (3 domains)
Triggered by: `kcd`, `kingdom come deliverance`, `kingdom come deliverance ii`, `kingdom come deliverance 2`

Excludes:
- `kingdom-come-deliverance.fandom.com`
- `kingdom-come-deliverance.vidyawiki.com`
- `kingdomcomedeliverance.wiki.fextralife.com`