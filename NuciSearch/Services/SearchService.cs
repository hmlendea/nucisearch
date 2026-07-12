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

        private static readonly Regex arcenservKeywordsPattern = new(@"\b(?:terraria)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex fextralifeKeywordsPattern = new(@"\b(?:baldur|bg3|borderlands|don'*t\s*starve|eso|elder\s*scrolls|skyrim|tes)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex fandomKeywordsPattern = new(@"\b(?:40k|baldur|bg3|don'*t\s*starve|eso|factorio|mc|minecraft|terraria|elder\s*scrolls|osrs|skyrim|tes|runescape|puzzle\s*pirates|ypp|game\s*of\s*thrones|warhammer|wh40k)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex huijiwikiKeywordsPattern = new(@"\b(?:borderlands)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex neoseekerKeywordsPattern = new(@"\b(?:osrs|runescape|terraria)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex strategywikiKeywordsPattern = new(@"\b(?:osrs|runescape)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex whitespacePattern = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex zeroWidthCharactersPattern = new(@"[\u200B-\u200D\uFEFF]", RegexOptions.Compiled);
        private static readonly Regex jiraPattern = new(@"^(?:AAP|AV|AND|CP)-\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rallyPattern = new(@"^(?:DE|F|US)[0-9]{6,8}$", RegexOptions.Compiled);
        private static readonly Regex wikiDataPattern = new(@"^Q\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex currencyPattern = new(@"^\d[\d.,]*\s+\w+\s+(?:in|în|to)\s+\w+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ipAddressQueryPattern = new(@"^(?:my|current)\s+ip(?:\s+address)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

                if (searchType.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetDuckDuckGoImagesUrl(query);
                }
                else if (searchType.Equals("maps", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetGoogleMapsUrl(query);
                }
                else if (searchType.Equals("torrents", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetYandexTorrentsUrl(query);
                }
                else if (searchType.Equals("videos", StringComparison.OrdinalIgnoreCase))
                {
                    url = GetYouTubeUrl(query);
                }
                else if (searchType.Equals("text", StringComparison.OrdinalIgnoreCase))
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

                logger.Info(NuciSearchOperation.Search, OperationStatus.Success, successLogInfos);
                return url;
            }
            catch (Exception exception)
            {
                logger.Error(NuciSearchOperation.Search, OperationStatus.Failure, exception, logInfos);
                throw;
            }
        }

        private static string GetAliExpressUrl(string query)
            => $"https://www.aliexpress.com/w/wholesale-{whitespacePattern.Replace(query.Trim(), "-")}.html?spm=a2g0o.detail.search.0";

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
            => $"https://boobpedia.com/wiki/index.php?title=Special%3ASearch&search={Uri.EscapeDataString(query)}&go=Go";

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
            if (CultureInfo.CurrentUICulture.Name.Equals("ro-RO"))
            {
                return $"https://addons.mozilla.org/ro/firefox/search/?q={Uri.EscapeDataString(query)}";
            }

            return $"https://addons.mozilla.org/en-GB/firefox/search/?q={Uri.EscapeDataString(query)}";
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
            if (CultureInfo.CurrentUICulture.Name.Equals("ro-RO"))
            {
                return $"https://google.ro/maps/search/{Uri.EscapeDataString(query)}";
            }

            return $"https://google.co.uk/maps/search/{Uri.EscapeDataString(query)}";
        }

        private static string GetHornbachUrl(string query)
            => $"https://hornbach.ro/s/{Uri.EscapeDataString(query)}";

        private static string GetIkeaUrl(string query)
        {
            if (CultureInfo.CurrentUICulture.Name.Equals("ro-RO"))
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
            => $"https://linkedin.com/search/results/all/?keywords={Uri.EscapeDataString(query)}";

        private static string GetMinecraftHeadsUrl(string query)
            => $"https://minecraft-heads.com/custom-heads/search?searchterm={Uri.EscapeDataString(query)}";

        private static string GetMinecraftWikiUrl(string query)
            => $"https://minecraft.wiki/?search={Uri.EscapeDataString(query)}";

        private static string GetModDbUrl(string query)
            => $"https://moddb.com/search?q={Uri.EscapeDataString(query)}";

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
            => $"https://app.plex.tv/desktop/#!/search?pivot=top&query={Uri.EscapeDataString(query)}";

        private static string GetProtonDbUrl(string query)
            => $"https://protondb.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetWikiDataUrl(string query)
            => $"https://wikidata.org/wiki/{Uri.EscapeDataString(query.ToUpperInvariant())}";

        private static string GetJiraUrl(string query)
            => $"https://worldpay.atlassian.net/browse/{Uri.EscapeDataString(query.ToUpperInvariant())}";

        private static string GetRallyUrl(string query)
            => $"https://rally1.rallydev.com/#/search?keywords={Uri.EscapeDataString(query)}";

        private static string GetRtingsUrl(string query)
            => $"https://rtings.com/search?q={Uri.EscapeDataString(query)}";

        private static string GetSinsayUrl(string query)
            => $"https://sinsay.com/ro/ro/?query={Uri.EscapeDataString(query)}";

        private static string GetSpigotUrl(string query)
            => $"https://spigotmc.org/search/294718421/?q={Uri.EscapeDataString(query)}&o=relevance";

        private static string GetSpyShopUrl(string query)
            => $"https://spy-shop.ro/catalogsearch/result/?q={Uri.EscapeDataString(query)}&o=relevance";

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

            if (CultureInfo.CurrentUICulture.Name.Equals("ro-RO"))
            {
                langCode = "ro";
            }

            string[] instances = [
                $"https://{langCode}.wikipedia.org/w/index.php?search={encodedQuery}",
                $"https://wikiless.tiekoetter.com/w/index.php?search={encodedQuery}&lang={langCode}",
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
            else if (rallyPattern.IsMatch(query))
            {
                return GetRallyUrl(query);
            }
            else if (wikiDataPattern.IsMatch(query))
            {
                return GetWikiDataUrl(query);
            }
            else if (currencyPattern.IsMatch(query))
            {
                return BuildCurrencySearchUrl(query);
            }
            else if (ipAddressQueryPattern.IsMatch(query))
            {
                return $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}";
            }
            else if (words.Count() >= 2)
            {
                return GetAutoUrlForMultiWordQuery(query, words);
            }

            return GetTextSearch(query);
        }

        private static string BuildCurrencySearchUrl(string query)
        {
            string currencyQuery = query;
            currencyQuery = Regex.Replace(currencyQuery, @"în", "in", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(currencyQuery, @"\b(?:lei|leu)\b", "RON", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(currencyQuery, @"\beuros?\b", "EUR", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(currencyQuery, @"\b(?:dollars?|dolari?)\b", "USD", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(currencyQuery, @"(?:lira|liră|lire)(?=\s|$)", "GBP", RegexOptions.IgnoreCase);
            currencyQuery = Regex.Replace(currencyQuery, @"\b[a-zA-Z]{3}\b", match => match.Value.ToUpperInvariant());

            return $"https://duckduckgo.com/?q={Uri.EscapeDataString(currencyQuery)}";
        }

        private static string GetAutoUrlForMultiWordQuery(string query, IEnumerable<string> words)
        {
            if (words.Any(word => word.Equals("aliexpress", StringComparison.OrdinalIgnoreCase)))
            {
                return GetAliExpressUrl(string.Join(" ", words.Where(word => !word.Equals("aliexpress", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("altex", StringComparison.OrdinalIgnoreCase))
            {
                return GetAltexUrl(query.Replace("altex", string.Empty, StringComparison.OrdinalIgnoreCase).Trim());
            }
            else if (words.Any(word => word.Equals("appstore", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("app store", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("apple store", StringComparison.OrdinalIgnoreCase)))
            {
                return GetAppStoreUrl(
                    string.Join(" ", words.Where(word => !word.Equals("appstore", StringComparison.OrdinalIgnoreCase)))
                        .Replace("app store", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("apple store", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim());
            }
            else if (query.Contains("arch wiki", StringComparison.OrdinalIgnoreCase))
            {
                return GetArchWikiUrl(query.Replace("arch wiki", string.Empty, StringComparison.OrdinalIgnoreCase).Trim());
            }
            else if (words.Any(word => word.Equals("auchan", StringComparison.OrdinalIgnoreCase)))
            {
                return GetAuchanUrl(string.Join(" ", words.Where(word => !word.Equals("auchan", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("audible", StringComparison.OrdinalIgnoreCase)))
            {
                return GetAudibleUrl(string.Join(" ", words.Where(word => !word.Equals("audible", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("boobpedia", StringComparison.OrdinalIgnoreCase)))
            {
                return GetBoobpediaUrl(string.Join(" ", words.Where(word => !word.Equals("boobpedia", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("carturesti", StringComparison.OrdinalIgnoreCase)))
            {
                return GetCarturestiUrl(string.Join(" ", words.Where(word => !word.Equals("carturesti", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("decathlon", StringComparison.OrdinalIgnoreCase)))
            {
                return GetDecathlonUrl(string.Join(" ", words.Where(word => !word.Equals("decathlon", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("dedeman", StringComparison.OrdinalIgnoreCase)))
            {
                return GetDedemanUrl(string.Join(" ", words.Where(word => !word.Equals("dedeman", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("dex", StringComparison.OrdinalIgnoreCase)))
            {
                return GetDexOnlineUrl(string.Join(" ", words.Where(word => !word.Equals("dex", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("digi24", StringComparison.OrdinalIgnoreCase)))
            {
                return GetDigi24Url(string.Join(" ", words.Where(word => !word.Equals("digi24", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("ebay", StringComparison.OrdinalIgnoreCase)))
            {
                return GetEbayUrl(string.Join(" ", words.Where(word => !word.Equals("ebay", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("emag", StringComparison.OrdinalIgnoreCase)))
            {
                return GetEmagUrl(string.Join(" ", words.Where(word => !word.Equals("emag", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("evomag", StringComparison.OrdinalIgnoreCase)))
            {
                return GetEvomagUrl(string.Join(" ", words.Where(word => !word.Equals("evomag", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("facebook", StringComparison.OrdinalIgnoreCase)))
            {
                return GetFacebookUrl(string.Join(" ", words.Where(word => !word.Equals("facebook", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("fdroid", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("f-droid", StringComparison.OrdinalIgnoreCase)))
            {
                return GetFdroidUrl(string.Join(" ", words.Where(word => !word.Equals("fdroid", StringComparison.OrdinalIgnoreCase)
                                                                      && !word.Equals("f-droid", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("firefox extension", StringComparison.OrdinalIgnoreCase))
            {
                return GetFirefoxExtensionsUrl(query
                    .Replace("firefox extensions", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("firefox extension", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (words.Any(word => word.Equals("flanco", StringComparison.OrdinalIgnoreCase)))
            {
                return GetFlancoUrl(string.Join(" ", words.Where(word => !word.Equals("flanco", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("flathub", StringComparison.OrdinalIgnoreCase)))
            {
                return GetFlatHubUrl(string.Join(" ", words.Where(word => !word.Equals("flathub", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("flip.ro", StringComparison.OrdinalIgnoreCase)))
            {
                return GetFlipRoUrl(string.Join(" ", words.Where(word => !word.Equals("flip.ro", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("g2a", StringComparison.OrdinalIgnoreCase)))
            {
                return GetG2aUrl(string.Join(" ", words.Where(word => !word.Equals("g2a", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("github", StringComparison.OrdinalIgnoreCase)))
            {
                return GetGitHubUrl(string.Join(" ", words.Where(word => !word.Equals("github", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("gog", StringComparison.OrdinalIgnoreCase)))
            {
                return GetGogUrl(string.Join(" ", words.Where(word => !word.Equals("gog", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("hornbach", StringComparison.OrdinalIgnoreCase)))
            {
                return GetHornbachUrl(string.Join(" ", words.Where(word => !word.Equals("hornbach", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("ikea", StringComparison.OrdinalIgnoreCase)))
            {
                return GetIkeaUrl(string.Join(" ", words.Where(word => !word.Equals("ikea", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("imdb", StringComparison.OrdinalIgnoreCase)))
            {
                return GetImdbUrl(string.Join(" ", words.Where(word => !word.Equals("imdb", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("instagram", StringComparison.OrdinalIgnoreCase)))
            {
                return GetInstagramUrl(string.Join(" ", words.Where(word => !word.Equals("instagram", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("jysk", StringComparison.OrdinalIgnoreCase)))
            {
                return GetJyskUrl(string.Join(" ", words.Where(word => !word.Equals("jysk", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("leroy merlin", StringComparison.OrdinalIgnoreCase))
            {
                return GetLeroyMerlinUrl(query.Replace("leroy merlin", string.Empty, StringComparison.OrdinalIgnoreCase).Trim());
            }
            else if (words.Any(word => word.Equals("lidl", StringComparison.OrdinalIgnoreCase)))
            {
                return GetLidlUrl(string.Join(" ", words.Where(word => !word.Equals("lidl", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("linkedin", StringComparison.OrdinalIgnoreCase)))
            {
                return GetLinkedinUrl(string.Join(" ", words.Where(word => !word.Equals("linkedin", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("moddb", StringComparison.OrdinalIgnoreCase)))
            {
                return GetModDbUrl(string.Join(" ", words.Where(word => !word.Equals("moddb", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("mc wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("minecraft wiki", StringComparison.OrdinalIgnoreCase))
            {
                return GetMinecraftWikiUrl(query
                    .Replace("mc wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("minecraft wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (query.Contains("mc head", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("minecraft head", StringComparison.OrdinalIgnoreCase))
            {
                return GetMinecraftHeadsUrl(query
                    .Replace("minecraft heads", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("minecraft head", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("mc heads", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("mc head", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (query.Contains("mc schematic", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("minecraft schematic", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlanetMinecraftSchematicsUrl(query
                    .Replace("minecraft schematics", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("minecraft schematic", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("mc schematics", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("mc schematic", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (words.Any(word => word.Equals("namemc", StringComparison.OrdinalIgnoreCase)))
            {
                return GetNameMcUrl(string.Join(" ", words.Where(word => !word.Equals("namemc", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("netflix", StringComparison.OrdinalIgnoreCase)))
            {
                return GetNetflixUrl(string.Join(" ", words.Where(word => !word.Equals("netflix", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("nexusmods", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("nexus mods", StringComparison.OrdinalIgnoreCase)))
            {
                return GetNexusModsUrl(
                    string.Join(" ", words.Where(word => !word.Equals("nexusmods", StringComparison.OrdinalIgnoreCase)))
                        .Replace("nexus mods", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim());
            }
            else if (words.Any(word => word.Equals("odysee", StringComparison.OrdinalIgnoreCase)))
            {
                return GetOdyseeUrl(string.Join(" ", words.Where(word => !word.Equals("odysee", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("olx", StringComparison.OrdinalIgnoreCase)))
            {
                return GetOlxUrl(string.Join(" ", words.Where(word => !word.Equals("olx", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("pcgarage", StringComparison.OrdinalIgnoreCase)))
            {
                return GetPcGarageUrl(string.Join(" ", words.Where(word => !word.Equals("pcgarage", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("pinterest", StringComparison.OrdinalIgnoreCase)))
            {
                return GetPinterestUrl(string.Join(" ", words.Where(word => !word.Equals("pinterest", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("planet minecraft", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlanetMinecraftUrl(query.Replace("planet minecraft", string.Empty, StringComparison.OrdinalIgnoreCase).Trim());
            }
            else if (query.Contains("play store", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("playstore", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlayStoreUrl(query
                    .Replace("play store", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("playstore", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim());
            }
            else if (words.Any(word => word.Equals("plex", StringComparison.OrdinalIgnoreCase)))
            {
                return GetPlexUrl(string.Join(" ", words.Where(word => !word.Equals("plex", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("protondb", StringComparison.OrdinalIgnoreCase)))
            {
                return GetProtonDbUrl(string.Join(" ", words.Where(word => !word.Equals("protondb", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("reddit", StringComparison.OrdinalIgnoreCase)))
            {
                return GetRedditUrl(string.Join(" ", words.Where(word => !word.Equals("reddit", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("rtings", StringComparison.OrdinalIgnoreCase)))
            {
                return GetRtingsUrl(string.Join(" ", words.Where(word => !word.Equals("rtings", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("sinsay", StringComparison.OrdinalIgnoreCase)))
            {
                return GetSinsayUrl(string.Join(" ", words.Where(word => !word.Equals("sinsay", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("spigot", StringComparison.OrdinalIgnoreCase)))
            {
                return GetSpigotUrl(string.Join(" ", words.Where(word => !word.Equals("spigot", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("spyshop", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("spyshop.ro", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("spy-shop", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("spy-shop.ro", StringComparison.OrdinalIgnoreCase)))
            {
                return GetSpyShopUrl(string.Join(" ", words.Where(word =>
                    !word.Equals("spyshop", StringComparison.OrdinalIgnoreCase)
                 && !word.Equals("spyshop.ro", StringComparison.OrdinalIgnoreCase)
                 && !word.Equals("spy-shop", StringComparison.OrdinalIgnoreCase)
                 && !word.Equals("spy-shop.ro", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("steamdb", StringComparison.OrdinalIgnoreCase)))
            {
                return GetSteamDbUrl(string.Join(" ", words.Where(word => !word.Equals("steamdb", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("tripadvisor", StringComparison.OrdinalIgnoreCase)))
            {
                return GetTripadvisorUrl(string.Join(" ", words.Where(word => !word.Equals("tripadvisor", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("tvdb", StringComparison.OrdinalIgnoreCase)
                                    || word.Equals("thetvdb", StringComparison.OrdinalIgnoreCase)))
            {
                return GetTvdbUrl(string.Join(" ", words.Where(word =>
                    !word.Equals("tvdb", StringComparison.OrdinalIgnoreCase)
                 && !word.Equals("thetvdb", StringComparison.OrdinalIgnoreCase))));
            }
            else if (query.Contains("uesp", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("elder scrolls wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("eso wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("morrowind wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("oblivion wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("skyrim wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("tes wiki", StringComparison.OrdinalIgnoreCase)
                  || query.Contains("the elder scrolls wiki", StringComparison.OrdinalIgnoreCase))
            {
                return GetUespUrl(
                    string.Join(" ", words.Where(word => !word.Equals("uesp", StringComparison.OrdinalIgnoreCase)))
                        .Replace("elder scrolls wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("eso wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("morrowind wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("oblivion wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("skyrim wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("tes wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("the elder scrolls wiki", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim());
            }
            else if (words.Any(word => word.Equals("vinted", StringComparison.OrdinalIgnoreCase)))
            {
                return GetVintedUrl(string.Join(" ", words.Where(word => !word.Equals("vinted", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("wikidata", StringComparison.OrdinalIgnoreCase)))
            {
                return GetWikiDataSearchUrl(string.Join(" ", words.Where(word => !word.Equals("wikidata", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("wikipedia", StringComparison.OrdinalIgnoreCase)))
            {
                return GetWikiPediaUrl(string.Join(" ", words.Where(word => !word.Equals("wikipedia", StringComparison.OrdinalIgnoreCase))));
            }
            else if (words.Any(word => word.Equals("youtube", StringComparison.OrdinalIgnoreCase)))
            {
                return GetYouTubeUrl(string.Join(" ", words.Where(word => !word.Equals("youtube", StringComparison.OrdinalIgnoreCase))));
            }

            return GetTextSearch(query);
        }
    }
}
