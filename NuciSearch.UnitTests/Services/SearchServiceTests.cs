using Moq;
using NuciLog.Core;
using NuciText.Obfuscation;
using NUnit.Framework;
using NuciSearch.Services;

namespace NuciSearch.UnitTests.Services
{
    [TestFixture]
    public sealed class SearchServiceTests
    {
        SearchService searchService;

        [SetUp]
        public void SetUp()
        {
            Mock<ILogger> loggerMock = new();
            searchService = new SearchService(loggerMock.Object);
        }

        // ── Empty / whitespace ────────────────────────────────────────────────

        [Test]
        public void GivenEmptyQuery_WhenGettingSearchUrl_ThenReturnsEmptyString()
            => Assert.That(
                searchService.GetSearchUrl(string.Empty, "auto"),
                Is.Empty);

        [Test]
        public void GivenWhitespaceOnlyQuery_WhenGettingSearchUrl_ThenReturnsEmptyString()
            => Assert.That(
                searchService.GetSearchUrl("   ", "auto"),
                Is.Empty);

        // ── Search types ──────────────────────────────────────────────────────

        [Test]
        public void GivenImagesSearchType_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
            => Assert.That(
                searchService.GetSearchUrl("cats", "images"),
                Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));

        [Test]
        public void GivenImagesSearchTypeUpperCase_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
            => Assert.That(
                searchService.GetSearchUrl("cats", "IMAGES"),
                Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));

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

        [Test]
        public void GivenTorrentsSearchType_WhenGettingSearchUrl_ThenReturnsYandexTorrentsUrl()
            => Assert.That(
                searchService.GetSearchUrl("ubuntu", "torrents"),
                Is.EqualTo("https://yandex.com/search/?text=ubuntu%20Torrent"));

        [Test]
        public void GivenVideosSearchType_WhenGettingSearchUrl_ThenReturnsYewtubeUrl()
            => Assert.That(
                searchService.GetSearchUrl("cats", "videos"),
                Is.EqualTo("https://yewtu.be/search?q=cats"));

        [Test]
        public void GivenTextSearchType_WhenGettingSearchUrl_ThenReturnsBraveOrDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("cats", "text"),
                Does.StartWith("https://search.brave.com/search?q=")
                    .Or.StartWith("https://duckduckgo.com/?q="));

        [Test]
        public void GivenSingleWordQuery_WhenGettingSearchUrl_ThenReturnsTextSearchUrl()
            => Assert.That(
                searchService.GetSearchUrl("cats", "auto"),
                Does.StartWith("https://search.brave.com/search?q=")
                    .Or.StartWith("https://duckduckgo.com/?q="));

        // ── WikiData ──────────────────────────────────────────────────────────

        [Test]
        public void GivenWikiDataQuery_WhenGettingSearchUrl_ThenReturnsWikiDataUrl()
            => Assert.That(
                searchService.GetSearchUrl("Q20717572", "auto"),
                Is.EqualTo("https://wikidata.org/wiki/Q20717572"));

        [Test]
        public void GivenWikiDataQuerySingleDigit_WhenGettingSearchUrl_ThenReturnsWikiDataUrl()
            => Assert.That(
                searchService.GetSearchUrl("Q1", "auto"),
                Is.EqualTo("https://wikidata.org/wiki/Q1"));

        [Test]
        public void GivenLowercaseWikiDataQuery_WhenGettingSearchUrl_ThenReturnsWikiDataUrlUppercased()
            => Assert.That(
                searchService.GetSearchUrl("q42", "auto"),
                Is.EqualTo("https://wikidata.org/wiki/Q42"));

        [Test]
        public void GivenWikiDataKeyword_WhenGettingSearchUrl_ThenReturnsWikiDataSearchUrl()
            => Assert.That(
                searchService.GetSearchUrl("wikidata Douglas Adams", "auto"),
                Is.EqualTo("https://wikidata.org/w/index.php?search=Douglas%20Adams"));

        [Test]
        public void GivenWikiDataKeywordUpperCase_WhenGettingSearchUrl_ThenReturnsWikiDataSearchUrl()
            => Assert.That(
                searchService.GetSearchUrl("WIKIDATA Douglas Adams", "auto"),
                Is.EqualTo("https://wikidata.org/w/index.php?search=Douglas%20Adams"));

        // ── Jira ──────────────────────────────────────────────────────────────

        [Test]
        public void GivenJiraAapQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
            => Assert.That(
                searchService.GetSearchUrl("AAP-123", "auto"),
                Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));

        [Test]
        public void GivenJiraAvQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
            => Assert.That(
                searchService.GetSearchUrl("AV-456", "auto"),
                Is.EqualTo("https://worldpay.atlassian.net/browse/AV-456"));

        [Test]
        public void GivenJiraAndQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
            => Assert.That(
                searchService.GetSearchUrl("AND-789", "auto"),
                Is.EqualTo("https://worldpay.atlassian.net/browse/AND-789"));

        [Test]
        public void GivenJiraCpQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
            => Assert.That(
                searchService.GetSearchUrl("CP-1", "auto"),
                Is.EqualTo("https://worldpay.atlassian.net/browse/CP-1"));

        [Test]
        public void GivenLowercaseJiraQuery_WhenGettingSearchUrl_ThenReturnsJiraUrlUppercased()
            => Assert.That(
                searchService.GetSearchUrl("aap-123", "auto"),
                Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));

        // ── Rally ─────────────────────────────────────────────────────────────

        [Test]
        public void GivenRallyDeQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("DE123456", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=DE123456"));

        [Test]
        public void GivenRallyFQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("F1234567", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=F1234567"));

        [Test]
        public void GivenRallyUsQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("US12345678", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=US12345678"));

        [Test]
        public void GivenRallyDeQueryWith8Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("DE12345678", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=DE12345678"));

        [Test]
        public void GivenRallyFQueryWith6Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("F123456", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=F123456"));

        [Test]
        public void GivenRallyUsQueryWith6Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
            => Assert.That(
                searchService.GetSearchUrl("US123456", "auto"),
                Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=US123456"));

        // ── Currency conversion ───────────────────────────────────────────────

        [Test]
        public void GivenCurrencyQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("100 USD in EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=100%20USD%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithToKeyword_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("200 USD to RON", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=200%20USD%20to%20RON"));

        [Test]
        public void GivenCurrencyQueryWithLei_WhenGettingSearchUrl_ThenNormalisesLeiToRon()
            => Assert.That(
                searchService.GetSearchUrl("100 lei in euro", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithDollars_WhenGettingSearchUrl_ThenNormalisesToUsd()
            => Assert.That(
                searchService.GetSearchUrl("50 dollars in EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=50%20USD%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithRomanianInPreposition_WhenGettingSearchUrl_ThenNormalisesIn()
            => Assert.That(
                searchService.GetSearchUrl("100 RON \u00een EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithEuros_WhenGettingSearchUrl_ThenNormalisesToEur()
            => Assert.That(
                searchService.GetSearchUrl("50 euros in USD", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=50%20EUR%20in%20USD"));

        [Test]
        public void GivenCurrencyQueryWithEuro_WhenGettingSearchUrl_ThenNormalisesToEur()
            => Assert.That(
                searchService.GetSearchUrl("200 euro in RON", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=200%20EUR%20in%20RON"));

        [Test]
        public void GivenCurrencyQueryWithLira_WhenGettingSearchUrl_ThenNormalisesToGbp()
            => Assert.That(
                searchService.GetSearchUrl("100 lira in RON", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=100%20GBP%20in%20RON"));

        [Test]
        public void GivenCurrencyQueryWithDolari_WhenGettingSearchUrl_ThenNormalisesToUsd()
            => Assert.That(
                searchService.GetSearchUrl("100 dolari in EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=100%20USD%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithDecimalAmount_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("61.3 USD in EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=61.3%20USD%20in%20EUR"));

        [Test]
        public void GivenCurrencyQueryWithThousandsSeparator_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("1,000 USD in EUR", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=1%2C000%20USD%20in%20EUR"));

        // ── IP address ────────────────────────────────────────────────────────

        [Test]
        public void GivenMyIpAddressQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("my ip address", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=my%20ip%20address"));

        [Test]
        public void GivenMyIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("my ip", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=my%20ip"));

        [Test]
        public void GivenCurrentIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("current ip", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=current%20ip"));

        [Test]
        public void GivenCurrentIpAddressQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("current ip address", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=current%20ip%20address"));

        [Test]
        public void GivenUppercaseMyIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("MY IP", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=MY%20IP"));

        [Test]
        public void GivenUppercaseCurrentIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
            => Assert.That(
                searchService.GetSearchUrl("CURRENT IP", "auto"),
                Is.EqualTo("https://duckduckgo.com/?q=CURRENT%20IP"));

        // ── Keyword redirects ─────────────────────────────────────────────────

        [Test]
        public void GivenAliExpressKeyword_WhenGettingSearchUrl_ThenReturnsAliExpressUrl()
            => Assert.That(
                searchService.GetSearchUrl("aliexpress blue shoes", "auto"),
                Is.EqualTo(
                    "https://www.aliexpress.com/w/wholesale-blue-shoes.html"
                    + "?spm=a2g0o.detail.search.0"));

        [Test]
        public void GivenAltexKeyword_WhenGettingSearchUrl_ThenReturnsAltexUrl()
            => Assert.That(
                searchService.GetSearchUrl("altex laptop", "auto"),
                Is.EqualTo("https://altex.ro/cauta/?q=laptop"));

        [Test]
        public void GivenAppstoreKeyword_WhenGettingSearchUrl_ThenReturnsAppStoreUrl()
            => Assert.That(
                searchService.GetSearchUrl("appstore spotify", "auto"),
                Is.EqualTo("https://apple.com/uk/search/spotify?src=globalnav"));

        [Test]
        public void GivenArchWikiKeyword_WhenGettingSearchUrl_ThenReturnsArchWikiUrl()
            => Assert.That(
                searchService.GetSearchUrl("arch wiki bluetooth", "auto"),
                Is.EqualTo("https://wiki.archlinux.org/index.php?search=bluetooth"));

        [Test]
        public void GivenAuchanKeyword_WhenGettingSearchUrl_ThenReturnsAuchanUrl()
            => Assert.That(
                searchService.GetSearchUrl("auchan lapte", "auto"),
                Is.EqualTo("https://auchan.ro/lapte"));

        [Test]
        public void GivenAudibleKeyword_WhenGettingSearchUrl_ThenReturnsAudibleUrl()
            => Assert.That(
                searchService.GetSearchUrl("audible dune", "auto"),
                Is.EqualTo("https://audible.com/search?advsearchKeywords=dune"));

        [Test]
        public void GivenCarturestiKeyword_WhenGettingSearchUrl_ThenReturnsCarturestiUrl()
            => Assert.That(
                searchService.GetSearchUrl("carturesti dune", "auto"),
                Is.EqualTo("https://carturesti.ro/product/search/dune"));

        [Test]
        public void GivenDecathlonKeyword_WhenGettingSearchUrl_ThenReturnsDecathlonUrl()
            => Assert.That(
                searchService.GetSearchUrl("decathlon bike", "auto"),
                Is.EqualTo("https://decathlon.ro/search?Ntt=bike"));

        [Test]
        public void GivenDedemanKeyword_WhenGettingSearchUrl_ThenReturnsDedemanUrl()
            => Assert.That(
                searchService.GetSearchUrl("dedeman vopsea", "auto"),
                Is.EqualTo("https://dedeman.ro/ro/catalogsearch/result/v2?q=vopsea"));

        [Test]
        public void GivenDexKeyword_WhenGettingSearchUrl_ThenReturnsDexOnlineUrl()
            => Assert.That(
                searchService.GetSearchUrl("dex pisica", "auto"),
                Is.EqualTo("https://dexonline.ro/definitie/pisica"));

        [Test]
        public void GivenDigi24Keyword_WhenGettingSearchUrl_ThenReturnsDigi24Url()
            => Assert.That(
                searchService.GetSearchUrl("digi24 alegeri", "auto"),
                Is.EqualTo("https://digi24.ro/cautare?q=alegeri"));

        [Test]
        public void GivenEbayKeyword_WhenGettingSearchUrl_ThenReturnsEbayUrl()
            => Assert.That(
                searchService.GetSearchUrl("ebay laptop", "auto"),
                Is.EqualTo("https://ebay.com/sch/i.html?_nkw=laptop"));

        [Test]
        public void GivenEmagKeyword_WhenGettingSearchUrl_ThenReturnsEmagUrl()
            => Assert.That(
                searchService.GetSearchUrl("emag laptop", "auto"),
                Is.EqualTo("https://emag.ro/search/laptop"));

        [Test]
        public void GivenEvomagKeyword_WhenGettingSearchUrl_ThenReturnsEvomagUrl()
            => Assert.That(
                searchService.GetSearchUrl("evomag ssd", "auto"),
                Is.EqualTo("https://evomag.ro/?sn.q=ssd"));

        [Test]
        public void GivenFacebookKeyword_WhenGettingSearchUrl_ThenReturnsTextSearchWithFacebookSite()
            => Assert.That(
                searchService.GetSearchUrl("facebook cats", "auto"),
                Does.Contain("site%3Afacebook.com%20cats"));

        [Test]
        public void GivenFdroidKeyword_WhenGettingSearchUrl_ThenReturnsFdroidUrl()
            => Assert.That(
                searchService.GetSearchUrl("fdroid vlc", "auto"),
                Is.EqualTo("https://search.f-droid.org/?q=vlc"));

        [Test]
        public void GivenFDroidHyphenatedKeyword_WhenGettingSearchUrl_ThenReturnsFdroidUrl()
            => Assert.That(
                searchService.GetSearchUrl("f-droid vlc", "auto"),
                Is.EqualTo("https://search.f-droid.org/?q=vlc"));

        [Test]
        [SetUICulture("en-GB")]
        public void GivenFirefoxExtensionKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsEnGbUrl()
            => Assert.That(
                searchService.GetSearchUrl("firefox extension ublock", "auto"),
                Is.EqualTo("https://addons.mozilla.org/en-GB/firefox/search/?q=ublock"));

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenFirefoxExtensionKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsRoUrl()
            => Assert.That(
                searchService.GetSearchUrl("firefox extension ublock", "auto"),
                Is.EqualTo("https://addons.mozilla.org/ro/firefox/search/?q=ublock"));

        [Test]
        public void GivenFlancoKeyword_WhenGettingSearchUrl_ThenReturnsFlancoUrl()
            => Assert.That(
                searchService.GetSearchUrl("flanco tv", "auto"),
                Is.EqualTo("https://flanco.ro/catalogsearch/result/?q=tv"));

        [Test]
        public void GivenFlathubKeyword_WhenGettingSearchUrl_ThenReturnsFlathubUrl()
            => Assert.That(
                searchService.GetSearchUrl("flathub vlc", "auto"),
                Is.EqualTo("https://flathub.org/apps/search/vlc"));

        [Test]
        public void GivenFlipRoKeyword_WhenGettingSearchUrl_ThenReturnsFlipRoUrl()
            => Assert.That(
                searchService.GetSearchUrl("flip.ro laptop", "auto"),
                Is.EqualTo("https://flip.ro/magazin/?search=laptop"));

        [Test]
        public void GivenG2aKeyword_WhenGettingSearchUrl_ThenReturnsG2aUrl()
            => Assert.That(
                searchService.GetSearchUrl("g2a cyberpunk", "auto"),
                Is.EqualTo("https://g2a.com/search?query=cyberpunk"));

        [Test]
        public void GivenGitHubKeyword_WhenGettingSearchUrl_ThenReturnsTextSearchWithGitHubSite()
            => Assert.That(
                searchService.GetSearchUrl("github dotnet runtime", "auto"),
                Does.Contain("site%3Agithub.com%20dotnet%20runtime"));

        [Test]
        public void GivenGogKeyword_WhenGettingSearchUrl_ThenReturnsGogUrl()
            => Assert.That(
                searchService.GetSearchUrl("gog witcher", "auto"),
                Is.EqualTo("https://gog.com/en/games?query=witcher"));

        [Test]
        public void GivenHornbachKeyword_WhenGettingSearchUrl_ThenReturnsHornbachUrl()
            => Assert.That(
                searchService.GetSearchUrl("hornbach vopsea", "auto"),
                Is.EqualTo("https://hornbach.ro/s/vopsea"));

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenIkeaKeyword_WhenGettingSearchUrl_ThenReturnsIkeaRoUrl()
            => Assert.That(
                searchService.GetSearchUrl("ikea scaun", "auto"),
                Is.EqualTo("https://ikea.com/ro/ro/search/?q=scaun"));

        [Test]
        [SetUICulture("en-GB")]
        public void GivenIkeaKeyword_WhenGettingSearchUrl_ThenReturnsIkeaGbUrl()
            => Assert.That(
                searchService.GetSearchUrl("ikea chair", "auto"),
                Is.EqualTo("https://ikea.com/gb/en/search/?q=chair"));

        [Test]
        public void GivenImdbKeyword_WhenGettingSearchUrl_ThenReturnsImdbUrl()
            => Assert.That(
                searchService.GetSearchUrl("imdb inception", "auto"),
                Is.EqualTo("https://libremdb.iket.me/find?q=inception"));

        [Test]
        public void GivenInstagramKeyword_WhenGettingSearchUrl_ThenReturnsInstagramUrl()
            => Assert.That(
                searchService.GetSearchUrl("instagram cats", "auto"),
                Is.EqualTo("https://instagram.com/popular/cats"));

        [Test]
        public void GivenJyskKeyword_WhenGettingSearchUrl_ThenReturnsJyskUrl()
            => Assert.That(
                searchService.GetSearchUrl("jysk scaun", "auto"),
                Is.EqualTo("https://jysk.ro/search?query=scaun"));

        [Test]
        public void GivenLeroyMerlinKeyword_WhenGettingSearchUrl_ThenReturnsLeroyMerlinUrl()
            => Assert.That(
                searchService.GetSearchUrl("leroy merlin vopsea", "auto"),
                Is.EqualTo("https://leroymerlin.ro/produse/search/vopsea"));

        [Test]
        public void GivenLidlKeyword_WhenGettingSearchUrl_ThenReturnsLidlUrl()
            => Assert.That(
                searchService.GetSearchUrl("lidl cafea", "auto"),
                Is.EqualTo("https://lidl.ro/q/search?q=cafea"));

        [Test]
        public void GivenLinkedinKeyword_WhenGettingSearchUrl_ThenReturnsLinkedinUrl()
            => Assert.That(
                searchService.GetSearchUrl("linkedin developer", "auto"),
                Is.EqualTo("https://linkedin.com/search/results/all/?keywords=developer"));

        [Test]
        public void GivenModDbKeyword_WhenGettingSearchUrl_ThenReturnsModDbUrl()
            => Assert.That(
                searchService.GetSearchUrl("moddb half-life", "auto"),
                Is.EqualTo("https://moddb.com/search?q=half-life"));

        [Test]
        public void GivenMcWikiKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftWikiUrl()
            => Assert.That(
                searchService.GetSearchUrl("mc wiki creeper", "auto"),
                Is.EqualTo("https://minecraft.wiki/?search=creeper"));

        [Test]
        public void GivenMinecraftWikiKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftWikiUrl()
            => Assert.That(
                searchService.GetSearchUrl("minecraft wiki creeper", "auto"),
                Is.EqualTo("https://minecraft.wiki/?search=creeper"));

        [Test]
        public void GivenMcHeadKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
            => Assert.That(
                searchService.GetSearchUrl("mc head dragon", "auto"),
                Is.EqualTo(
                    "https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));

        [Test]
        public void GivenMinecraftHeadKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
            => Assert.That(
                searchService.GetSearchUrl("minecraft head dragon", "auto"),
                Is.EqualTo(
                    "https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));

        [Test]
        public void GivenMcSchematicKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
            => Assert.That(
                searchService.GetSearchUrl("mc schematic castle", "auto"),
                Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));

        [Test]
        public void GivenMinecraftSchematicKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
            => Assert.That(
                searchService.GetSearchUrl("minecraft schematic castle", "auto"),
                Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));

        [Test]
        public void GivenNameMcKeyword_WhenGettingSearchUrl_ThenReturnsNameMcUrl()
            => Assert.That(
                searchService.GetSearchUrl("namemc Notch", "auto"),
                Is.EqualTo("https://namemc.com/search?q=Notch"));

        [Test]
        public void GivenNetflixKeyword_WhenGettingSearchUrl_ThenReturnsNetflixUrl()
            => Assert.That(
                searchService.GetSearchUrl("netflix stranger things", "auto"),
                Is.EqualTo("https://netflix.com/search?q=stranger%20things"));

        [Test]
        public void GivenNexusModsKeyword_WhenGettingSearchUrl_ThenReturnsNexusModsUrl()
            => Assert.That(
                searchService.GetSearchUrl("nexusmods skyrim", "auto"),
                Is.EqualTo("https://nexusmods.com/search?keyword=skyrim"));

        [Test]
        public void GivenOdyseeKeyword_WhenGettingSearchUrl_ThenReturnsOdyseeUrl()
            => Assert.That(
                searchService.GetSearchUrl("odysee cooking", "auto"),
                Is.EqualTo("https://odysee.com/$/search?q=cooking"));

        [Test]
        public void GivenOlxKeyword_WhenGettingSearchUrl_ThenReturnsOlxUrl()
            => Assert.That(
                searchService.GetSearchUrl("olx laptop", "auto"),
                Is.EqualTo("https://olx.ro/d/oferte/q-laptop"));

        [Test]
        public void GivenPcGarageKeyword_WhenGettingSearchUrl_ThenReturnsPcGarageUrl()
            => Assert.That(
                searchService.GetSearchUrl("pcgarage ssd", "auto"),
                Is.EqualTo("https://pcgarage.ro/cauta/ssd"));

        [Test]
        public void GivenPinterestKeyword_WhenGettingSearchUrl_ThenReturnsPinterestUrl()
            => Assert.That(
                searchService.GetSearchUrl("pinterest cats", "auto"),
                Is.EqualTo("https://pinterest.com/search/pins/?q=cats"));

        [Test]
        public void GivenPlanetMinecraftKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftUrl()
            => Assert.That(
                searchService.GetSearchUrl("planet minecraft castle", "auto"),
                Is.EqualTo("https://planetminecraft.com/resources/?keywords=castle"));

        [Test]
        public void GivenPlayStoreKeyword_WhenGettingSearchUrl_ThenReturnsPlayStoreUrl()
            => Assert.That(
                searchService.GetSearchUrl("play store spotify", "auto"),
                Is.EqualTo("https://play.google.com/store/search?q=spotify"));

        [Test]
        public void GivenPlaystoreKeyword_WhenGettingSearchUrl_ThenReturnsPlayStoreUrl()
            => Assert.That(
                searchService.GetSearchUrl("playstore spotify", "auto"),
                Is.EqualTo("https://play.google.com/store/search?q=spotify"));

        [Test]
        public void GivenPlexKeyword_WhenGettingSearchUrl_ThenReturnsPlexUrl()
            => Assert.That(
                searchService.GetSearchUrl("plex inception", "auto"),
                Is.EqualTo(
                    "https://app.plex.tv/desktop/#!/search?pivot=top&query=inception"));

        [Test]
        public void GivenProtonDbKeyword_WhenGettingSearchUrl_ThenReturnsProtonDbUrl()
            => Assert.That(
                searchService.GetSearchUrl("protondb cyberpunk", "auto"),
                Is.EqualTo("https://protondb.com/search?q=cyberpunk"));

        [Test]
        public void GivenRedditKeyword_WhenGettingSearchUrl_ThenReturnsRedlibUrl()
            => Assert.That(
                searchService.GetSearchUrl("reddit cats", "auto"),
                Does.Contain("search?q=cats"));

        [Test]
        public void GivenRtingsKeyword_WhenGettingSearchUrl_ThenReturnsRtingsUrl()
            => Assert.That(
                searchService.GetSearchUrl("rtings tv", "auto"),
                Is.EqualTo("https://rtings.com/search?q=tv"));

        [Test]
        public void GivenSinsayKeyword_WhenGettingSearchUrl_ThenReturnsSinsayUrl()
            => Assert.That(
                searchService.GetSearchUrl("sinsay dress", "auto"),
                Is.EqualTo("https://sinsay.com/ro/ro/?query=dress"));

        [Test]
        public void GivenSpigotKeyword_WhenGettingSearchUrl_ThenReturnsSpigotUrl()
            => Assert.That(
                searchService.GetSearchUrl("spigot worldedit", "auto"),
                Is.EqualTo(
                    "https://spigotmc.org/search/294718421/?q=worldedit&o=relevance"));

        [Test]
        public void GivenSpyshopKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
            => Assert.That(
                searchService.GetSearchUrl("spyshop camera", "auto"),
                Is.EqualTo(
                    "https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));

        [Test]
        public void GivenSpyShopHyphenatedKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
            => Assert.That(
                searchService.GetSearchUrl("spy-shop camera", "auto"),
                Is.EqualTo(
                    "https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));

        [Test]
        public void GivenSteamDbKeyword_WhenGettingSearchUrl_ThenReturnsSteamDbUrl()
            => Assert.That(
                searchService.GetSearchUrl("steamdb half-life", "auto"),
                Is.EqualTo("https://steamdb.info/search/?a=all&q=half-life"));

        [Test]
        public void GivenTripadvisorKeyword_WhenGettingSearchUrl_ThenReturnsTripadvisorUrl()
            => Assert.That(
                searchService.GetSearchUrl("tripadvisor paris", "auto"),
                Is.EqualTo("https://tripadvisor.com/Search?q=paris"));

        [Test]
        public void GivenTvDbKeyword_WhenGettingSearchUrl_ThenReturnsTvDbUrl()
            => Assert.That(
                searchService.GetSearchUrl("tvdb breaking bad", "auto"),
                Is.EqualTo("https://thetvdb.com/search?query=breaking%20bad"));

        [Test]
        public void GivenTheTvDbKeyword_WhenGettingSearchUrl_ThenReturnsTvDbUrl()
            => Assert.That(
                searchService.GetSearchUrl("thetvdb breaking bad", "auto"),
                Is.EqualTo("https://thetvdb.com/search?query=breaking%20bad"));

        [Test]
        public void GivenUespKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("uesp dragonborn", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dragonborn"));

        [Test]
        public void GivenSkyrimWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("skyrim wiki dragonborn", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dragonborn"));

        [Test]
        public void GivenEsoWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("eso wiki lorebook", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=lorebook"));

        [Test]
        public void GivenElderScrollsWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("elder scrolls wiki dwemer", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dwemer"));

        [Test]
        public void GivenVintedKeyword_WhenGettingSearchUrl_ThenReturnsVintedUrl()
            => Assert.That(
                searchService.GetSearchUrl("vinted jacket", "auto"),
                Is.EqualTo("https://vinted.com/catalog?search_text=jacket"));

        [Test]
        public void GivenWikipediaKeyword_WhenGettingSearchUrl_ThenReturnsWikipediaInstanceUrl()
            => Assert.That(
                searchService.GetSearchUrl("wikipedia cats", "auto"),
                Does.Contain("wikipedia.org").Or.Contain("wikiless"));

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenWikipediaKeywordAndRoRoCulture_WhenGettingSearchUrl_ThenReturnsRomanianWikipediaUrl()
            => Assert.That(
                searchService.GetSearchUrl("wikipedia pisica", "auto"),
                Does.Contain("ro.wikipedia.org").Or.Contain("wikiless"));

        [Test]
        [SetUICulture("en-GB")]
        public void GivenWikipediaKeywordAndEnGbCulture_WhenGettingSearchUrl_ThenReturnsEnglishWikipediaUrl()
            => Assert.That(
                searchService.GetSearchUrl("wikipedia cats", "auto"),
                Does.Contain("en.wikipedia.org").Or.Contain("wikiless"));

        [Test]
        public void GivenYoutubeKeyword_WhenGettingSearchUrl_ThenReturnsYewtubeUrl()
            => Assert.That(
                searchService.GetSearchUrl("youtube cats", "auto"),
                Is.EqualTo("https://yewtu.be/search?q=cats"));

        [Test]
        public void GivenBoobpediaKeyword_WhenGettingSearchUrl_ThenReturnsBoobpediaUrl()
            => Assert.That(
                searchService.GetSearchUrl("boobpedia elodia", "auto"),
                Is.EqualTo(
                    "https://boobpedia.com/wiki/index.php?title=Special%3ASearch"
                    + "&search=elodia&go=Go"));

        [Test]
        [SetUICulture("en-GB")]
        public void GivenFirefoxExtensionsPluralKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsUrl()
            => Assert.That(
                searchService.GetSearchUrl("firefox extensions ublock", "auto"),
                Is.EqualTo("https://addons.mozilla.org/en-GB/firefox/search/?q=ublock"));

        [Test]
        public void GivenMcHeadsPluralKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
            => Assert.That(
                searchService.GetSearchUrl("mc heads dragon", "auto"),
                Is.EqualTo(
                    "https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));

        [Test]
        public void GivenMinecraftHeadsPluralKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
            => Assert.That(
                searchService.GetSearchUrl("minecraft heads dragon", "auto"),
                Is.EqualTo(
                    "https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));

        [Test]
        public void GivenMcSchematicsPluralKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
            => Assert.That(
                searchService.GetSearchUrl("mc schematics castle", "auto"),
                Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));

        [Test]
        public void GivenMinecraftSchematicsPluralKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
            => Assert.That(
                searchService.GetSearchUrl("minecraft schematics castle", "auto"),
                Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));

        [Test]
        public void GivenMorrowindWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("morrowind wiki dunmer", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dunmer"));

        [Test]
        public void GivenOblivionWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("oblivion wiki daedra", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=daedra"));

        [Test]
        public void GivenTesWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
            => Assert.That(
                searchService.GetSearchUrl("tes wiki shouts", "auto"),
                Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=shouts"));

        [Test]
        public void GivenSpyShopRoKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
            => Assert.That(
                searchService.GetSearchUrl("spy-shop.ro camera", "auto"),
                Is.EqualTo(
                    "https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));

        [Test]
        public void GivenSpyshopRoKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
            => Assert.That(
                searchService.GetSearchUrl("spyshop.ro camera", "auto"),
                Is.EqualTo(
                    "https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));

        // ── Domain blacklists ─────────────────────────────────────────────────

        [Test]
        public void GivenMinecraftQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("minecraft building", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenTerrariaQuery_WhenGettingTextSearchUrl_ThenIncludesArcenservFandomAndNeoseekerBlacklists()
            => Assert.That(
                searchService.GetSearchUrl("terraria boss", "text"),
                Does.Contain("arcenserv").And.Contain("fandom").And.Contain("neoseeker"));

        [Test]
        public void GivenBg3Query_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("bg3 build", "text"),
                Does.Contain("fextralife"));

        [Test]
        public void GivenBg3Query_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("bg3 build", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenBorderlandsQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("borderlands guns", "text"),
                Does.Contain("fextralife"));

        [Test]
        public void GivenBorderlandsQuery_WhenGettingTextSearchUrl_ThenIncludesHuijiwikiBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("borderlands guns", "text"),
                Does.Contain("huijiwiki"));

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("osrs quest guide", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesNeoseekerBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("osrs quest guide", "text"),
                Does.Contain("neoseeker"));

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesStrategywikiBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("osrs quest guide", "text"),
                Does.Contain("strategywiki"));

        [Test]
        public void GivenFactorioQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("factorio base design", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenWarhammerQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("warhammer lore", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenWh40kQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("wh40k space marine", "text"),
                Does.Contain("fandom"));

        [Test]
        public void Given40kQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("40k tau build", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenSkyrimQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("skyrim archery build", "text"),
                Does.Contain("fextralife"));

        [Test]
        public void GivenSkyrimQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("skyrim archery build", "text"),
                Does.Contain("fandom"));

        [Test]
        public void GivenBaldurQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("baldur paladin guide", "text"),
                Does.Contain("fextralife"));

        [Test]
        public void GivenBaldurQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
            => Assert.That(
                searchService.GetSearchUrl("baldur paladin guide", "text"),
                Does.Contain("fandom"));

        // ── Deobfuscation ─────────────────────────────────────────────────────

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

        [Test]
        public void GivenObfuscatedKeywordQuery_WhenGettingSearchUrl_ThenDeobfuscatesAndRedirects()
        {
            INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
            NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
            string obfuscatedQuery = obfuscator.Obfuscate("emag laptop", options);

            string result = searchService.GetSearchUrl(obfuscatedQuery, "auto");

            Assert.That(result, Is.EqualTo("https://emag.ro/search/laptop"));
        }

        [Test]
        public void GivenObfuscatedIpAddressQuery_WhenGettingSearchUrl_ThenDeobfuscatesAndMatchesPattern()
        {
            INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
            NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
            string obfuscatedQuery = obfuscator.Obfuscate("my ip address", options);

            string result = searchService.GetSearchUrl(obfuscatedQuery, "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=my%20ip%20address"));
        }

        [Test]
        public void GivenObfuscatedQueryWithSearchType_WhenGettingSearchUrl_ThenDeobfuscatesQueryOnly()
        {
            INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
            NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
            string obfuscatedQuery = obfuscator.Obfuscate("cats", options);

            string result = searchService.GetSearchUrl(obfuscatedQuery, "images");

            Assert.That(
                result,
                Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));
        }

        // ── Query normalisation ───────────────────────────────────────────────

        [Test]
        public void GivenQueryWithSurroundingWhitespace_WhenGettingSearchUrl_ThenNormalisesWhitespace()
        {
            string result1 = searchService.GetSearchUrl("  emag  laptop  ", "auto");
            string result2 = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GivenQueryWithMultipleSpaces_WhenGettingSearchUrl_ThenCollapsesWhitespace()
        {
            string result1 = searchService.GetSearchUrl("emag    laptop", "auto");
            string result2 = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GivenQueryWithZeroWidthCharacters_WhenGettingSearchUrl_ThenStripsZeroWidthCharacters()
        {
            string result1 = searchService.GetSearchUrl("emag\u200B laptop", "auto");
            string result2 = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result1, Is.EqualTo(result2));
        }

        // ── Text search fallback ──────────────────────────────────────────────

        [Test]
        public void GivenMultiWordQueryWithNoKeyword_WhenGettingSearchUrl_ThenReturnsTextSearchUrl()
            => Assert.That(
                searchService.GetSearchUrl("dark souls guide", "auto"),
                Does.StartWith("https://search.brave.com/search?q=")
                    .Or.StartWith("https://duckduckgo.com/?q="));
    }
}
