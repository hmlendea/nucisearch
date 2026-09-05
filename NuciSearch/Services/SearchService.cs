using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NuciLog.Core;
using NuciText.Obfuscation;
using NuciSearch.Logging;

namespace NuciSearch.Services
{
    public sealed class SearchService(ILogger logger) : ISearchService
    {
        private static readonly INuciTextObfuscator obfuscator = new NuciTextObfuscator();

        private static readonly Regex arcenservKeywordsPattern = new(
            @"\b(?:terraria)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex fextralifeKeywordsPattern = new(
            @"\b(?:baldur|bg3|borderlands|don'*t\s*starve|eso|elder\s*scrolls|skyrim|tes)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex fandomKeywordsPattern = new(
            @"\b(?:40k|baldur|bg3|don'*t\s*starve|eso|factorio|mc|minecraft|terraria"
                + @"|elder\s*scrolls|osrs|skyrim|tes|runescape|puzzle\s*pirates|ypp"
                + @"|game\s*of\s*thrones|warhammer|wh40k)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex huijiwikiKeywordsPattern = new(
            @"\b(?:borderlands)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex neoseekerKeywordsPattern = new(
            @"\b(?:osrs|runescape|terraria)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex strategywikiKeywordsPattern = new(
            @"\b(?:osrs|runescape)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex stellarisKeywordsPattern = new(
            @"\b(?:stellaris)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex heartsOfIronKeywordsPattern = new(
            @"\b(?:hoi\s*(?:4|iv)|hearts?\s*of\s*iron\s*(?:4|iv))\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex gtaKeywordsPattern = new(
            @"\b(?:gta|grand\s*theft\s*auto|grand\s*theft)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex kingdomComeDeliveranceKeywordsPattern = new(
            @"\b(?:kcd(?:\s*2|\s*ii)?|kingdom\s*come(?::)?\s*deliverance(?:\s*(?:2|ii))?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex aSongOfIceAndFireWikiKeywordsPattern = new(
            @"\b(?:asoiaf|a\s*song\s*of\s*ice\s*and\s*fire|game\s*of\s*thrones"
                + @"|house\s*of\s*the\s*dragon)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex whitespacePattern = new(@"\s+", RegexOptions.Compiled);

        private static readonly Regex zeroWidthCharactersPattern = new(
            @"[\u200B-\u200D\uFEFF]", RegexOptions.Compiled);

        private static readonly Regex jiraPattern = new(
            @"^(?:AAP|AV|AND|CP)-\d+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex rallyPattern = new(
            @"^(?:DE|F|US)[0-9]{6,8}$", RegexOptions.Compiled);

        private static readonly Regex wikiDataPattern = new(
            @"^Q\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex currencyPattern = new(
            @"^\d[\d.,]*\s+\w+\s+(?:in|în|to)\s+\w+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ipAddressQueryPattern = new(
            @"^(?:my|current)\s+ip(?:\s+address)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string GetSearchUrl(string rawQuery, string searchType)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new(NuciSearchLogInfoKey.Query, rawQuery),
                new(NuciSearchLogInfoKey.SearchType, searchType)
            ];

            logger.Info(NuciSearchOperation.Search, OperationStatus.Started, logInfos);

            try
            {
                string query = NormaliseQuery(obfuscator.Deobfuscate(rawQuery));

                if (string.IsNullOrEmpty(query))
                {
                    logger.Info(NuciSearchOperation.Search, OperationStatus.Success, logInfos);
                    return string.Empty;
                }

                string url;

                if (string.Equals(searchType, "images", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetDuckDuckGoImagesUrl(query);
                }
                else if (string.Equals(searchType, "maps", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetGoogleMapsUrl(query);
                }
                else if (string.Equals(
                    searchType, "torrents", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetYandexTorrentsUrl(query);
                }
                else if (string.Equals(
                    searchType, "videos", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetYouTubeUrl(query);
                }
                else if (string.Equals(searchType, "text", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetTextSearch(query);
                }
                else
                {
                    url = GetAutoUrl(query);
                }

                IEnumerable<LogInfo> successLogInfos =
                [
                    new(NuciSearchLogInfoKey.Query, rawQuery),
                    new(NuciSearchLogInfoKey.SearchType, searchType),
                    new(NuciSearchLogInfoKey.Url, url)
                ];

                logger.Info(
                    NuciSearchOperation.Search,
                    OperationStatus.Success,
                    successLogInfos);

                return url;
            }
            catch (Exception exception)
            {
                logger.Error(
                    NuciSearchOperation.Search,
                    OperationStatus.Failure,
                    exception,
                    logInfos);

                throw;
            }
        }

        private static string GetAliExpressUrl(string query)
            => "https://www.aliexpress.com/w/wholesale-"
                + whitespacePattern.Replace(query.Trim(), "-")
                + ".html?spm=a2g0o.detail.search.0";

        private static string GetAltexUrl(string query)
            => $"https://altex.ro/cauta/?q={Uri.EscapeDataString(query)}";

        private static string GetAppStoreUrl(string query)
            => $"https://apple.com/uk/search/{Uri.EscapeDataString(query)}?src=globalnav";

        private static string GetArchWikiUrl(string query)
            => $"https://wiki.archlinux.org/index.php?search={Uri.EscapeDataString(query)}";

        private static string GetAuchanUrl(string query)
            => $"https://auchan.ro/{Uri.EscapeDataString(query)}";

        private static string GetAudibleUrl(string query)
            => $"https://audible.com/search?advsearchKeywords={Uri.EscapeDataString(query)}";

        private static string GetBoobpediaUrl(string query)
            => "https://boobpedia.com/wiki/index.php?title=Special%3ASearch&search="
                + Uri.EscapeDataString(query)
                + "&go=Go";

        private static string GetCarturestiUrl(string query)
            => $"https://carturesti.ro/product/search/{Uri.EscapeDataString(query)}";

        private static string GetDecathlonUrl(string query)
            => $"https://decathlon.ro/search?Ntt={Uri.EscapeDataString(query)}";

        private static string GetDedemanUrl(string query)
            => $"https://dedeman.ro/ro/catalogsearch/result/v2?q={Uri.EscapeDataString(query)}";

        private static string GetDexOnlineUrl(string query)
            => $"https://dexonline.ro/definitie/{Uri.EscapeDataString(query)}";

        private static string GetDigi24Url(string query)
            => $"https://digi24.ro/cautare?q={Uri.EscapeDataString(query)}";

        private static string GetDuckDuckGoImagesUrl(string query)
            => $"https://duckduckgo.com/?iax=images&ia=images&q={Uri.EscapeDataString(query)}";

        private static string GetEbayUrl(string query)
            => $"https://ebay.com/sch/i.html?_nkw={Uri.EscapeDataString(query)}";

        private static string GetEmagUrl(string query)
            => $"https://emag.ro/search/{Uri.EscapeDataString(query)}";

        private static string GetEvomagUrl(string query)
            => $"https://evomag.ro/?sn.q={Uri.EscapeDataString(query)}";

        private static string GetFacebookUrl(string query)
            => GetTextSearch($"site:facebook.com {query}");

        private static string GetFdroidUrl(string query)
            => $"https://search.f-droid.org/?q={Uri.EscapeDataString(query)}";

        private static string GetFirefoxExtensionsUrl(string query)
        {
            if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
            {
                return "https://addons.mozilla.org/ro/firefox/search/?q="
                    + Uri.EscapeDataString(query);
            }

            return "https://addons.mozilla.org/en-GB/firefox/search/?q="
                + Uri.EscapeDataString(query);
        }

        private static string GetFlancoUrl(string query)
            => $"https://flanco.ro/catalogsearch/result/?q={Uri.EscapeDataString(query)}";

        private static string GetFlatHubUrl(string query)
            => $"https://flathub.org/apps/search/{Uri.EscapeDataString(query)}";

        private static string GetFlipRoUrl(string query)
            => $"https://flip.ro/magazin/?search={Uri.EscapeDataString(query)}";

        private static string GetG2aUrl(string query)
            => $"https://g2a.com/search?query={Uri.EscapeDataString(query)}";

        private static string GetGitHubUrl(string query)
            => GetTextSearch($"site:github.com {query}");

        private static string GetGogUrl(string query)
            => $"https://gog.com/en/games?query={Uri.EscapeDataString(query)}";

        private static string GetGoogleMapsUrl(string query)
        {
            if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
            {
                return $"https://google.ro/maps/search/{Uri.EscapeDataString(query)}";
            }

            return $"https://google.co.uk/maps/search/{Uri.EscapeDataString(query)}";
        }

        private static string GetHornbachUrl(string query)
            => $"https://hornbach.ro/s/{Uri.EscapeDataString(query)}";

        private static string GetIkeaUrl(string query)
        {
            if (string.Equals(CultureInfo.CurrentUICulture.Name, "ro-RO"))
            {
                return $"https://ikea.com/ro/ro/search/?q={Uri.EscapeDataString(query)}";
            }

            return $"https://ikea.com/gb/en/search/?q={Uri.EscapeDataString(query)}";
        }

        private static string GetImdbUrl(string query)
            => $"https://libremdb.iket.me/find?q={Uri.EscapeDataString(query)}";

        private static string GetInstagramUrl(string query)
            => $"https://instagram.com/popular/{Uri.EscapeDataString(query)}";

        private static string GetJyskUrl(string query)
            => $"https://jysk.ro/search?query={Uri.EscapeDataString(query)}";

        private static string GetLeroyMerlinUrl(string query)
            => $"https://leroymerlin.ro/produse/search/{Uri.EscapeDataString(query)}";

        private static string GetLidlUrl(string query)
            => $"https://lidl.ro/q/search?q={Uri.EscapeDataString(query)}";

        private static string GetLinkedinUrl(string query)
            => "https://linkedin.com/search/results/all/?keywords="
                + Uri.EscapeDataString(query);

        private static string GetMinecraftHeadsUrl(string query)
            => "https://minecraft-heads.com/custom-heads/search?searchterm="
                + Uri.EscapeDataString(query);

        private static string GetMinecraftWikiUrl(string query)
            => $"https://minecraft.wiki/?search={Uri.EscapeDataString(query)}";

        private static string GetModDbUrl(string query)
            => $"https://moddb.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetMoemaxUrl(string query)
            => $"https://moemax.ro/s/?s={Uri.EscapeDataString(query)}";

        private static string GetNameMcUrl(string query)
            => $"https://namemc.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetNetflixUrl(string query)
            => $"https://netflix.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetNexusModsUrl(string query)
            => $"https://nexusmods.com/search?keyword={Uri.EscapeDataString(query)}";

        private static string GetOdyseeUrl(string query)
            => $"https://odysee.com/$/search?q={Uri.EscapeDataString(query)}";

        private static string GetOlxUrl(string query)
            => $"https://olx.ro/d/oferte/q-{Uri.EscapeDataString(query)}";

        private static string GetPcGarageUrl(string query)
            => $"https://pcgarage.ro/cauta/{Uri.EscapeDataString(query)}";

        private static string GetPinterestUrl(string query)
            => $"https://pinterest.com/search/pins/?q={Uri.EscapeDataString(query)}";

        private static string GetPlanetMinecraftUrl(string query)
            => $"https://planetminecraft.com/resources/?keywords={Uri.EscapeDataString(query)}";

        private static string GetPlanetMinecraftSchematicsUrl(string query)
            => $"https://planetminecraft.com/projects/?keywords={Uri.EscapeDataString(query)}";

        private static string GetPlayStoreUrl(string query)
            => $"https://play.google.com/store/search?q={Uri.EscapeDataString(query)}";

        private static string GetPlexUrl(string query)
            => "https://app.plex.tv/desktop/#!/search?pivot=top&query="
                + Uri.EscapeDataString(query);

        private static string GetProtonDbUrl(string query)
            => $"https://protondb.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetWikiDataUrl(string query)
            => $"https://wikidata.org/wiki/{Uri.EscapeDataString(query.ToUpperInvariant())}";

        private static string GetJiraUrl(string query)
            => "https://worldpay.atlassian.net/browse/"
                + Uri.EscapeDataString(query.ToUpperInvariant());

        private static string GetRallyUrl(string query)
            => $"https://rally1.rallydev.com/#/search?keywords={Uri.EscapeDataString(query)}";

        private static string GetRtingsUrl(string query)
            => $"https://rtings.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetSinsayUrl(string query)
            => $"https://sinsay.com/ro/ro/?query={Uri.EscapeDataString(query)}";

        private static string GetSpigotUrl(string query)
            => "https://spigotmc.org/search/294718421/?q="
                + Uri.EscapeDataString(query)
                + "&o=relevance";

        private static string GetSpyShopUrl(string query)
            => "https://spy-shop.ro/catalogsearch/result/?q="
                + Uri.EscapeDataString(query)
                + "&o=relevance";

        private static string GetSteamDbUrl(string query)
            => $"https://steamdb.info/search/?a=all&q={Uri.EscapeDataString(query)}";

        private static string GetTripadvisorUrl(string query)
            => $"https://tripadvisor.com/Search?q={Uri.EscapeDataString(query)}";

        private static string GetTvdbUrl(string query)
            => $"https://thetvdb.com/search?query={Uri.EscapeDataString(query)}";

        private static string GetUespUrl(string query)
            => $"https://en.uesp.net/wiki/Special:Search?search={Uri.EscapeDataString(query)}";

        private static string GetVintedUrl(string query)
            => $"https://vinted.com/catalog?search_text={Uri.EscapeDataString(query)}";

        private static string GetYandexTorrentsUrl(string query)
            => $"https://yandex.com/search/?text={Uri.EscapeDataString(query + " Torrent")}";

        private static string GetYouTubeUrl(string query)
            => $"https://yewtu.be/search?q={Uri.EscapeDataString(query)}";

        private static string GetRedditUrl(string query)
        {
            string encodedQuery = Uri.EscapeDataString(query);
            string[] instances = [
                $"https://red.artemislena.eu/search?q={encodedQuery}",
                $"https://redlib.catsarch.com/search?q={encodedQuery}",
                $"https://redlib.cow.rip/search?q={encodedQuery}",
                $"https://redlib.nadeko.net/search?q={encodedQuery}",
                $"https://redlib.perennialte.ch/search?q={encodedQuery}",
                $"https://redlib.privadency.com/search?q={encodedQuery}",
                $"https://snoo.habedieeh.re/search?q={encodedQuery}",
            ];

            return instances[Random.Shared.Next(instances.Length)];
        }

        private static string GetWikiDataSearchUrl(string query)
            => $"https://wikidata.org/w/index.php?search={Uri.EscapeDataString(query)}";

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
                $"https://wikiless.tiekoetter.com/w/index.php?search={encodedQuery}"
                    + $"&lang={langCode}",
            ];

            return instances[Random.Shared.Next(instances.Length)];
        }

        private static string ApplyQueryCustomisations(string query)
            => ApplyDomainBlacklist(query);

        private static string ApplyDomainBlacklist(string query)
        {
            if (arcenservKeywordsPattern.IsMatch(query))
            {
                query += " -site:arcenserv.info";
            }

            if (fextralifeKeywordsPattern.IsMatch(query))
            {
                query += " -site:wiki.fextralife.com";
            }

            if (fandomKeywordsPattern.IsMatch(query))
            {
                query += " -site:fandom.com";
            }

            if (huijiwikiKeywordsPattern.IsMatch(query))
            {
                query += " -site:huijiwiki.com";
            }

            if (neoseekerKeywordsPattern.IsMatch(query))
            {
                query += " -site:neoseeker.com";
            }

            if (strategywikiKeywordsPattern.IsMatch(query))
            {
                query += " -site:strategywiki.org";
            }

            if (aSongOfIceAndFireWikiKeywordsPattern.IsMatch(query))
            {
                query += " -site:gameofthrones.fandom.com";
                query += " -site:hbo-tv.fandom.com";
                query += " -site:hieloyfuego.fandom.com";
                query += " -site:iceandfire.fandom.com";
                query += " -site:listofdeaths.fandom.com";
                query += " -site:wikiofthrones.com";
            }

            if (stellarisKeywordsPattern.IsMatch(query))
            {
                query += " -site:stellaris.fandom.com";
            }

            if (heartsOfIronKeywordsPattern.IsMatch(query))
            {
                query += " -site:heartsofiron.fandom.com";
            }

            if (gtaKeywordsPattern.IsMatch(query))
            {
                query += " -site:grandtheftwiki.com";
                query += " -site:gta.fandom.com";
                query += " -site:gta5wiki.com";
                query += " -site:gtaboom.com";
                query += " -site:gtastarsandstripes.miraheze.org";
                query += " -site:neoseeker.com";
                query += " -site:rockstargames.fandom.com";
                query += " -site:sportskeeda.com";
                query += " -site:wikigta.org";
            }

            if (kingdomComeDeliveranceKeywordsPattern.IsMatch(query))
            {
                query += " -site:kingdom-come-deliverance.fandom.com";
                query += " -site:kingdom-come-deliverance.vidyawiki.com";
                query += " -site:kingdomcomedeliverance.wiki.fextralife.com";
            }

            return query;
        }

        private static string GetTextSearch(string query)
        {
            string encodedQuery = Uri.EscapeDataString(ApplyQueryCustomisations(query));
            string[] searchEngines = [
                $"https://search.brave.com/search?q={encodedQuery}",
                $"https://duckduckgo.com/?q={encodedQuery}",
            ];

            return searchEngines[Random.Shared.Next(searchEngines.Length)];
        }

        private static string NormaliseQuery(string rawQuery)
        {
            string normalised = rawQuery.Normalize(NormalizationForm.FormKC);
            normalised = zeroWidthCharactersPattern.Replace(normalised, string.Empty);
            normalised = whitespacePattern.Replace(normalised, " ");

            return normalised.Trim();
        }

        private static string GetAutoUrl(string query)
        {
            IEnumerable<string> words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (jiraPattern.IsMatch(query))
            {
                return GetJiraUrl(query);
            }

            if (rallyPattern.IsMatch(query))
            {
                return GetRallyUrl(query);
            }

            if (wikiDataPattern.IsMatch(query))
            {
                return GetWikiDataUrl(query);
            }

            if (currencyPattern.IsMatch(query))
            {
                return BuildCurrencySearchUrl(query);
            }

            if (ipAddressQueryPattern.IsMatch(query))
            {
                return $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}";
            }

            if (words.Count() >= 2)
            {
                return GetAutoUrlForMultiWordQuery(query, words);
            }

            return GetTextSearch(query);
        }

        private static string BuildCurrencySearchUrl(string query)
        {
            string currencyQuery = query;
            currencyQuery = Regex.Replace(
                currencyQuery, @"în", "in", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(
                currencyQuery, @"\b(?:lei|leu)\b", "RON", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(
                currencyQuery, @"\beuros?\b", "EUR", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(
                currencyQuery,
                @"\b(?:dollars?|dolari?)\b",
                "USD",
                RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(
                currencyQuery,
                @"(?:lira|liră|lire)(?=\s|$)",
                "GBP",
                RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(
                currencyQuery,
                @"\b[a-zA-Z]{3}\b",
                match => match.Value.ToUpperInvariant());

            return $"https://duckduckgo.com/?q={Uri.EscapeDataString(currencyQuery)}";
        }

        private static string GetAutoUrlForMultiWordQuery(
            string query, IEnumerable<string> words)
        {
            if (ContainsKeyword(words, "aliexpress"))
            {
                return GetAliExpressUrl(StripKeyword(words, "aliexpress"));
            }
            else if (query.Contains("altex", StringComparison.OrdinalIgnoreCase))
            {
                return GetAltexUrl(query
                    .Replace("altex", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (ContainsKeyword(words, "appstore") ||
                ContainsKeyword(words, "app store") ||
                ContainsKeyword(words, "apple store"))
            {
                return GetAppStoreUrl(StripKeyword(words, "appstore")
                    .Replace("app store", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("apple store", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (query.Contains("arch wiki", StringComparison.OrdinalIgnoreCase))
            {
                return GetArchWikiUrl(query
                    .Replace("arch wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (ContainsKeyword(words, "auchan"))
            {
                return GetAuchanUrl(StripKeyword(words, "auchan"));
            }
            else if (ContainsKeyword(words, "audible"))
            {
                return GetAudibleUrl(StripKeyword(words, "audible"));
            }
            else if (ContainsKeyword(words, "boobpedia"))
            {
                return GetBoobpediaUrl(StripKeyword(words, "boobpedia"));
            }
            else if (ContainsKeyword(words, "carturesti"))
            {
                return GetCarturestiUrl(StripKeyword(words, "carturesti"));
            }
            else if (ContainsKeyword(words, "decathlon"))
            {
                return GetDecathlonUrl(StripKeyword(words, "decathlon"));
            }
            else if (ContainsKeyword(words, "dedeman"))
            {
                return GetDedemanUrl(StripKeyword(words, "dedeman"));
            }
            else if (ContainsKeyword(words, "dex"))
            {
                return GetDexOnlineUrl(StripKeyword(words, "dex"));
            }
            else if (ContainsKeyword(words, "digi24"))
            {
                return GetDigi24Url(StripKeyword(words, "digi24"));
            }
            else if (ContainsKeyword(words, "ebay"))
            {
                return GetEbayUrl(StripKeyword(words, "ebay"));
            }
            else if (ContainsKeyword(words, "emag"))
            {
                return GetEmagUrl(StripKeyword(words, "emag"));
            }
            else if (ContainsKeyword(words, "evomag"))
            {
                return GetEvomagUrl(StripKeyword(words, "evomag"));
            }
            else if (ContainsKeyword(words, "facebook"))
            {
                return GetFacebookUrl(StripKeyword(words, "facebook"));
            }
            else if (ContainsKeyword(words, "fdroid") || ContainsKeyword(words, "f-droid"))
            {
                IEnumerable<string> remainingWords = words.Where(word =>
                    !string.Equals(word, "fdroid", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(word, "f-droid", StringComparison.OrdinalIgnoreCase));

                return GetFdroidUrl(string.Join(" ", remainingWords));
            }
            else if (query.Contains("firefox extension", StringComparison.OrdinalIgnoreCase))
            {
                string searchQuery = query
                    .Replace(
                        "firefox extensions",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "firefox extension",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return GetFirefoxExtensionsUrl(searchQuery);
            }
            else if (ContainsKeyword(words, "flanco"))
            {
                return GetFlancoUrl(StripKeyword(words, "flanco"));
            }
            else if (ContainsKeyword(words, "flathub"))
            {
                return GetFlatHubUrl(StripKeyword(words, "flathub"));
            }
            else if (ContainsKeyword(words, "flip.ro"))
            {
                return GetFlipRoUrl(StripKeyword(words, "flip.ro"));
            }
            else if (ContainsKeyword(words, "g2a"))
            {
                return GetG2aUrl(StripKeyword(words, "g2a"));
            }
            else if (ContainsKeyword(words, "github"))
            {
                return GetGitHubUrl(StripKeyword(words, "github"));
            }
            else if (ContainsKeyword(words, "gog"))
            {
                return GetGogUrl(StripKeyword(words, "gog"));
            }
            else if (ContainsKeyword(words, "hornbach"))
            {
                return GetHornbachUrl(StripKeyword(words, "hornbach"));
            }
            else if (ContainsKeyword(words, "ikea"))
            {
                return GetIkeaUrl(StripKeyword(words, "ikea"));
            }
            else if (ContainsKeyword(words, "imdb"))
            {
                return GetImdbUrl(StripKeyword(words, "imdb"));
            }
            else if (ContainsKeyword(words, "instagram"))
            {
                return GetInstagramUrl(StripKeyword(words, "instagram"));
            }
            else if (ContainsKeyword(words, "jysk"))
            {
                return GetJyskUrl(StripKeyword(words, "jysk"));
            }
            else if (query.Contains("leroy merlin", StringComparison.OrdinalIgnoreCase))
            {
                return GetLeroyMerlinUrl(query
                    .Replace(
                        "leroy merlin",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (ContainsKeyword(words, "lidl"))
            {
                return GetLidlUrl(StripKeyword(words, "lidl"));
            }
            else if (ContainsKeyword(words, "linkedin"))
            {
                return GetLinkedinUrl(StripKeyword(words, "linkedin"));
            }
            else if (ContainsKeyword(words, "moddb"))
            {
                return GetModDbUrl(StripKeyword(words, "moddb"));
            }
            else if (ContainsKeyword(words, "momax"))
            {
                return GetMoemaxUrl(StripKeyword(words, "momax"));
            }
            else if (query.Contains("mc wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("minecraft wiki", StringComparison.OrdinalIgnoreCase))
            {
                string searchQuery = query
                    .Replace("mc wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "minecraft wiki",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return GetMinecraftWikiUrl(searchQuery);
            }
            else if (query.Contains("mc head", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("minecraft head", StringComparison.OrdinalIgnoreCase))
            {
                string searchQuery = query
                    .Replace(
                        "minecraft heads",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "minecraft head",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace("mc heads", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("mc head", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return GetMinecraftHeadsUrl(searchQuery);
            }
            else if (query.Contains("mc schematic", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("minecraft schematic", StringComparison.OrdinalIgnoreCase))
            {
                string searchQuery = query
                    .Replace(
                        "minecraft schematics",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "minecraft schematic",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "mc schematics",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace("mc schematic", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return GetPlanetMinecraftSchematicsUrl(searchQuery);
            }
            else if (ContainsKeyword(words, "namemc"))
            {
                return GetNameMcUrl(StripKeyword(words, "namemc"));
            }
            else if (ContainsKeyword(words, "netflix"))
            {
                return GetNetflixUrl(StripKeyword(words, "netflix"));
            }
            else if (ContainsKeyword(words, "nexusmods") ||
                ContainsKeyword(words, "nexus mods"))
            {
                return GetNexusModsUrl(StripKeyword(words, "nexusmods")
                    .Replace("nexus mods", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (ContainsKeyword(words, "odysee"))
            {
                return GetOdyseeUrl(StripKeyword(words, "odysee"));
            }
            else if (ContainsKeyword(words, "olx"))
            {
                return GetOlxUrl(StripKeyword(words, "olx"));
            }
            else if (ContainsKeyword(words, "pcgarage"))
            {
                return GetPcGarageUrl(StripKeyword(words, "pcgarage"));
            }
            else if (ContainsKeyword(words, "pinterest"))
            {
                return GetPinterestUrl(StripKeyword(words, "pinterest"));
            }
            else if (query.Contains("planet minecraft", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlanetMinecraftUrl(query
                    .Replace(
                        "planet minecraft",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (query.Contains("play store", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("playstore", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlayStoreUrl(query
                    .Replace("play store", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("playstore", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (ContainsKeyword(words, "plex"))
            {
                return GetPlexUrl(StripKeyword(words, "plex"));
            }
            else if (ContainsKeyword(words, "protondb"))
            {
                return GetProtonDbUrl(StripKeyword(words, "protondb"));
            }
            else if (ContainsKeyword(words, "reddit"))
            {
                return GetRedditUrl(StripKeyword(words, "reddit"));
            }
            else if (ContainsKeyword(words, "rtings"))
            {
                return GetRtingsUrl(StripKeyword(words, "rtings"));
            }
            else if (ContainsKeyword(words, "sinsay"))
            {
                return GetSinsayUrl(StripKeyword(words, "sinsay"));
            }
            else if (ContainsKeyword(words, "spigot"))
            {
                return GetSpigotUrl(StripKeyword(words, "spigot"));
            }
            else if (ContainsKeyword(words, "spyshop") ||
                ContainsKeyword(words, "spyshop.ro") ||
                ContainsKeyword(words, "spy-shop") ||
                ContainsKeyword(words, "spy-shop.ro"))
            {
                IEnumerable<string> remainingWords = words.Where(word =>
                    !string.Equals(word, "spyshop", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(word, "spyshop.ro", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(word, "spy-shop", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(word, "spy-shop.ro", StringComparison.OrdinalIgnoreCase));

                return GetSpyShopUrl(string.Join(" ", remainingWords));
            }
            else if (ContainsKeyword(words, "steamdb"))
            {
                return GetSteamDbUrl(StripKeyword(words, "steamdb"));
            }
            else if (ContainsKeyword(words, "tripadvisor"))
            {
                return GetTripadvisorUrl(StripKeyword(words, "tripadvisor"));
            }
            else if (ContainsKeyword(words, "tvdb") || ContainsKeyword(words, "thetvdb"))
            {
                IEnumerable<string> remainingWords = words.Where(word =>
                    !string.Equals(word, "tvdb", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(word, "thetvdb", StringComparison.OrdinalIgnoreCase));

                return GetTvdbUrl(string.Join(" ", remainingWords));
            }
            else if (query.Contains("uesp", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("elder scrolls wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("eso wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("morrowind wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("oblivion wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("skyrim wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("tes wiki", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("the elder scrolls wiki", StringComparison.OrdinalIgnoreCase))
            {
                string searchQuery = StripKeyword(words, "uesp")
                    .Replace(
                        "elder scrolls wiki",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace("eso wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "morrowind wiki",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace("oblivion wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("skyrim wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("tes wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "the elder scrolls wiki",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

                return GetUespUrl(searchQuery);
            }
            else if (ContainsKeyword(words, "vinted"))
            {
                return GetVintedUrl(StripKeyword(words, "vinted"));
            }
            else if (ContainsKeyword(words, "wikidata"))
            {
                return GetWikiDataSearchUrl(StripKeyword(words, "wikidata"));
            }
            else if (ContainsKeyword(words, "wikipedia"))
            {
                return GetWikiPediaUrl(StripKeyword(words, "wikipedia"));
            }
            else if (ContainsKeyword(words, "youtube"))
            {
                return GetYouTubeUrl(StripKeyword(words, "youtube"));
            }

            return GetTextSearch(query);
        }

        private static bool ContainsKeyword(IEnumerable<string> words, string keyword)
            => words.Any(word => KeywordsAreEqual(word, keyword));

        private static string StripKeyword(IEnumerable<string> words, string keyword)
            => string.Join(
                " ",
                words.Where(word => !KeywordsAreEqual(word, keyword)));

        private static bool KeywordsAreEqual(string firstKeyword, string secondKeyword)
            => string.Equals(
                NormaliseKeyword(firstKeyword),
                NormaliseKeyword(secondKeyword),
                StringComparison.OrdinalIgnoreCase);

        private static string NormaliseKeyword(string value)
            => string.Concat(
                value.Normalize(NormalizationForm.FormD)
                .Where(character =>
                    CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark))
                .Normalize(NormalizationForm.FormC)
                .Replace("oe", "o", StringComparison.OrdinalIgnoreCase);
    }
}
