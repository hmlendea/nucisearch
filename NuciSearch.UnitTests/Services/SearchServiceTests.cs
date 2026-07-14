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
        {
            string result = searchService.GetSearchUrl(string.Empty, "auto");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GivenWhitespaceOnlyQuery_WhenGettingSearchUrl_ThenReturnsEmptyString()
        {
            string result = searchService.GetSearchUrl("   ", "auto");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        // ── Search types ──────────────────────────────────────────────────────

        [Test]
        public void GivenImagesSearchType_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
        {
            string result = searchService.GetSearchUrl("cats", "images");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));
        }

        [Test]
        public void GivenImagesSearchTypeUpperCase_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
        {
            string result = searchService.GetSearchUrl("cats", "IMAGES");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));
        }

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleRoMapsUrl()
        {
            string result = searchService.GetSearchUrl("london", "maps");

            Assert.That(result, Is.EqualTo("https://google.ro/maps/search/london"));
        }

        [Test]
        [SetUICulture("en-GB")]
        public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleCoUkMapsUrl()
        {
            string result = searchService.GetSearchUrl("london", "maps");

            Assert.That(result, Is.EqualTo("https://google.co.uk/maps/search/london"));
        }

        [Test]
        public void GivenTorrentsSearchType_WhenGettingSearchUrl_ThenReturnsYandexTorrentsUrl()
        {
            string result = searchService.GetSearchUrl("ubuntu", "torrents");

            Assert.That(result, Is.EqualTo("https://yandex.com/search/?text=ubuntu%20Torrent"));
        }

        [Test]
        public void GivenVideosSearchType_WhenGettingSearchUrl_ThenReturnsYewtubeUrl()
        {
            string result = searchService.GetSearchUrl("cats", "videos");

            Assert.That(result, Is.EqualTo("https://yewtu.be/search?q=cats"));
        }

        [Test]
        public void GivenTextSearchType_WhenGettingSearchUrl_ThenReturnsBraveOrDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("cats", "text");

            Assert.That(result.StartsWith("https://search.brave.com/search?q=")
                     || result.StartsWith("https://duckduckgo.com/?q="));
        }

        [Test]
        public void GivenSingleWordQuery_WhenGettingSearchUrl_ThenReturnsTextSearchUrl()
        {
            string result = searchService.GetSearchUrl("cats", "auto");

            Assert.That(result.StartsWith("https://search.brave.com/search?q=")
                     || result.StartsWith("https://duckduckgo.com/?q="));
        }

        // ── WikiData ──────────────────────────────────────────────────────────

        [Test]
        public void GivenWikiDataQuery_WhenGettingSearchUrl_ThenReturnsWikiDataUrl()
        {
            string result = searchService.GetSearchUrl("Q20717572", "auto");

            Assert.That(result, Is.EqualTo("https://wikidata.org/wiki/Q20717572"));
        }

        [Test]
        public void GivenWikiDataQuerySingleDigit_WhenGettingSearchUrl_ThenReturnsWikiDataUrl()
        {
            string result = searchService.GetSearchUrl("Q1", "auto");

            Assert.That(result, Is.EqualTo("https://wikidata.org/wiki/Q1"));
        }

        [Test]
        public void GivenLowercaseWikiDataQuery_WhenGettingSearchUrl_ThenReturnsWikiDataUrlUppercased()
        {
            string result = searchService.GetSearchUrl("q42", "auto");

            Assert.That(result, Is.EqualTo("https://wikidata.org/wiki/Q42"));
        }

        [Test]
        public void GivenWikiDataKeyword_WhenGettingSearchUrl_ThenReturnsWikiDataSearchUrl()
        {
            string result = searchService.GetSearchUrl("wikidata Douglas Adams", "auto");

            Assert.That(result, Is.EqualTo("https://wikidata.org/w/index.php?search=Douglas%20Adams"));
        }

        [Test]
        public void GivenWikiDataKeywordUpperCase_WhenGettingSearchUrl_ThenReturnsWikiDataSearchUrl()
        {
            string result = searchService.GetSearchUrl("WIKIDATA Douglas Adams", "auto");

            Assert.That(result, Is.EqualTo("https://wikidata.org/w/index.php?search=Douglas%20Adams"));
        }

        // ── Jira ──────────────────────────────────────────────────────────────

        [Test]
        public void GivenJiraAapQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
        {
            string result = searchService.GetSearchUrl("AAP-123", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));
        }

        [Test]
        public void GivenJiraAvQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
        {
            string result = searchService.GetSearchUrl("AV-456", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/AV-456"));
        }

        [Test]
        public void GivenJiraAndQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
        {
            string result = searchService.GetSearchUrl("AND-789", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/AND-789"));
        }

        [Test]
        public void GivenJiraCpQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
        {
            string result = searchService.GetSearchUrl("CP-1", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/CP-1"));
        }

        [Test]
        public void GivenLowercaseJiraQuery_WhenGettingSearchUrl_ThenReturnsJiraUrlUppercased()
        {
            string result = searchService.GetSearchUrl("aap-123", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));
        }

        // ── Rally ─────────────────────────────────────────────────────────────

        [Test]
        public void GivenRallyDeQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("DE123456", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=DE123456"));
        }

        [Test]
        public void GivenRallyFQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("F1234567", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=F1234567"));
        }

        [Test]
        public void GivenRallyUsQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("US12345678", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=US12345678"));
        }

        [Test]
        public void GivenRallyDeQueryWith8Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("DE12345678", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=DE12345678"));
        }

        [Test]
        public void GivenRallyFQueryWith6Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("F123456", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=F123456"));
        }

        [Test]
        public void GivenRallyUsQueryWith6Digits_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("US123456", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=US123456"));
        }

        // ── Currency conversion ───────────────────────────────────────────────

        [Test]
        public void GivenCurrencyQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("100 USD in EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=100%20USD%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithToKeyword_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("200 USD to RON", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=200%20USD%20to%20RON"));
        }

        [Test]
        public void GivenCurrencyQueryWithLei_WhenGettingSearchUrl_ThenNormalisesLeiToRon()
        {
            string result = searchService.GetSearchUrl("100 lei in euro", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithDollars_WhenGettingSearchUrl_ThenNormalisesToUsd()
        {
            string result = searchService.GetSearchUrl("50 dollars in EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=50%20USD%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithRomanianInPreposition_WhenGettingSearchUrl_ThenNormalisesIn()
        {
            string result = searchService.GetSearchUrl("100 RON în EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=100%20RON%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithEuros_WhenGettingSearchUrl_ThenNormalisesToEur()
        {
            string result = searchService.GetSearchUrl("50 euros in USD", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=50%20EUR%20in%20USD"));
        }

        [Test]
        public void GivenCurrencyQueryWithEuro_WhenGettingSearchUrl_ThenNormalisesToEur()
        {
            string result = searchService.GetSearchUrl("200 euro in RON", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=200%20EUR%20in%20RON"));
        }

        [Test]
        public void GivenCurrencyQueryWithLira_WhenGettingSearchUrl_ThenNormalisesToGbp()
        {
            string result = searchService.GetSearchUrl("100 lira in RON", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=100%20GBP%20in%20RON"));
        }

        [Test]
        public void GivenCurrencyQueryWithDolari_WhenGettingSearchUrl_ThenNormalisesToUsd()
        {
            string result = searchService.GetSearchUrl("100 dolari in EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=100%20USD%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithDecimalAmount_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("61.3 USD in EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=61.3%20USD%20in%20EUR"));
        }

        [Test]
        public void GivenCurrencyQueryWithThousandsSeparator_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("1,000 USD in EUR", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=1%2C000%20USD%20in%20EUR"));
        }

        // ── IP address ────────────────────────────────────────────────────────

        [Test]
        public void GivenMyIpAddressQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("my ip address", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=my%20ip%20address"));
        }

        [Test]
        public void GivenMyIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("my ip", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=my%20ip"));
        }

        [Test]
        public void GivenCurrentIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("current ip", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=current%20ip"));
        }

        [Test]
        public void GivenCurrentIpAddressQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("current ip address", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=current%20ip%20address"));
        }

        [Test]
        public void GivenUppercaseMyIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("MY IP", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=MY%20IP"));
        }

        [Test]
        public void GivenUppercaseCurrentIpQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("CURRENT IP", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=CURRENT%20IP"));
        }

        // ── Keyword redirects ─────────────────────────────────────────────────

        [Test]
        public void GivenAliExpressKeyword_WhenGettingSearchUrl_ThenReturnsAliExpressUrl()
        {
            string result = searchService.GetSearchUrl("aliexpress blue shoes", "auto");

            Assert.That(result, Is.EqualTo("https://www.aliexpress.com/w/wholesale-blue-shoes.html?spm=a2g0o.detail.search.0"));
        }

        [Test]
        public void GivenAltexKeyword_WhenGettingSearchUrl_ThenReturnsAltexUrl()
        {
            string result = searchService.GetSearchUrl("altex laptop", "auto");

            Assert.That(result, Is.EqualTo("https://altex.ro/cauta/?q=laptop"));
        }

        [Test]
        public void GivenAppstoreKeyword_WhenGettingSearchUrl_ThenReturnsAppStoreUrl()
        {
            string result = searchService.GetSearchUrl("appstore spotify", "auto");

            Assert.That(result, Is.EqualTo("https://apple.com/uk/search/spotify?src=globalnav"));
        }

        [Test]
        public void GivenArchWikiKeyword_WhenGettingSearchUrl_ThenReturnsArchWikiUrl()
        {
            string result = searchService.GetSearchUrl("arch wiki bluetooth", "auto");

            Assert.That(result, Is.EqualTo("https://wiki.archlinux.org/index.php?search=bluetooth"));
        }

        [Test]
        public void GivenAuchanKeyword_WhenGettingSearchUrl_ThenReturnsAuchanUrl()
        {
            string result = searchService.GetSearchUrl("auchan lapte", "auto");

            Assert.That(result, Is.EqualTo("https://auchan.ro/lapte"));
        }

        [Test]
        public void GivenAudibleKeyword_WhenGettingSearchUrl_ThenReturnsAudibleUrl()
        {
            string result = searchService.GetSearchUrl("audible dune", "auto");

            Assert.That(result, Is.EqualTo("https://audible.com/search?advsearchKeywords=dune"));
        }

        [Test]
        public void GivenCarturestiKeyword_WhenGettingSearchUrl_ThenReturnsCarturestiUrl()
        {
            string result = searchService.GetSearchUrl("carturesti dune", "auto");

            Assert.That(result, Is.EqualTo("https://carturesti.ro/product/search/dune"));
        }

        [Test]
        public void GivenDecathlonKeyword_WhenGettingSearchUrl_ThenReturnsDecathlonUrl()
        {
            string result = searchService.GetSearchUrl("decathlon bike", "auto");

            Assert.That(result, Is.EqualTo("https://decathlon.ro/search?Ntt=bike"));
        }

        [Test]
        public void GivenDedemanKeyword_WhenGettingSearchUrl_ThenReturnsDedemanUrl()
        {
            string result = searchService.GetSearchUrl("dedeman vopsea", "auto");

            Assert.That(result, Is.EqualTo("https://dedeman.ro/ro/catalogsearch/result/v2?q=vopsea"));
        }

        [Test]
        public void GivenDexKeyword_WhenGettingSearchUrl_ThenReturnsDexOnlineUrl()
        {
            string result = searchService.GetSearchUrl("dex pisica", "auto");

            Assert.That(result, Is.EqualTo("https://dexonline.ro/definitie/pisica"));
        }

        [Test]
        public void GivenDigi24Keyword_WhenGettingSearchUrl_ThenReturnsDigi24Url()
        {
            string result = searchService.GetSearchUrl("digi24 alegeri", "auto");

            Assert.That(result, Is.EqualTo("https://digi24.ro/cautare?q=alegeri"));
        }

        [Test]
        public void GivenEbayKeyword_WhenGettingSearchUrl_ThenReturnsEbayUrl()
        {
            string result = searchService.GetSearchUrl("ebay laptop", "auto");

            Assert.That(result, Is.EqualTo("https://ebay.com/sch/i.html?_nkw=laptop"));
        }

        [Test]
        public void GivenEmagKeyword_WhenGettingSearchUrl_ThenReturnsEmagUrl()
        {
            string result = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result, Is.EqualTo("https://emag.ro/search/laptop"));
        }

        [Test]
        public void GivenEvomagKeyword_WhenGettingSearchUrl_ThenReturnsEvomagUrl()
        {
            string result = searchService.GetSearchUrl("evomag ssd", "auto");

            Assert.That(result, Is.EqualTo("https://evomag.ro/?sn.q=ssd"));
        }

        [Test]
        public void GivenFacebookKeyword_WhenGettingSearchUrl_ThenReturnsTextSearchWithFacebookSite()
        {
            string result = searchService.GetSearchUrl("facebook cats", "auto");

            Assert.That(result.Contains("site%3Afacebook.com%20cats"));
        }

        [Test]
        public void GivenFdroidKeyword_WhenGettingSearchUrl_ThenReturnsFdroidUrl()
        {
            string result = searchService.GetSearchUrl("fdroid vlc", "auto");

            Assert.That(result, Is.EqualTo("https://search.f-droid.org/?q=vlc"));
        }

        [Test]
        public void GivenFDroidHyphenatedKeyword_WhenGettingSearchUrl_ThenReturnsFdroidUrl()
        {
            string result = searchService.GetSearchUrl("f-droid vlc", "auto");

            Assert.That(result, Is.EqualTo("https://search.f-droid.org/?q=vlc"));
        }

        [Test]
        [SetUICulture("en-GB")]
        public void GivenFirefoxExtensionKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsEnGbUrl()
        {
            string result = searchService.GetSearchUrl("firefox extension ublock", "auto");

            Assert.That(result, Is.EqualTo("https://addons.mozilla.org/en-GB/firefox/search/?q=ublock"));
        }

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenFirefoxExtensionKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsRoUrl()
        {
            string result = searchService.GetSearchUrl("firefox extension ublock", "auto");

            Assert.That(result, Is.EqualTo("https://addons.mozilla.org/ro/firefox/search/?q=ublock"));
        }

        [Test]
        public void GivenFlancoKeyword_WhenGettingSearchUrl_ThenReturnsFlancoUrl()
        {
            string result = searchService.GetSearchUrl("flanco tv", "auto");

            Assert.That(result, Is.EqualTo("https://flanco.ro/catalogsearch/result/?q=tv"));
        }

        [Test]
        public void GivenFlathubKeyword_WhenGettingSearchUrl_ThenReturnsFlathubUrl()
        {
            string result = searchService.GetSearchUrl("flathub vlc", "auto");

            Assert.That(result, Is.EqualTo("https://flathub.org/apps/search/vlc"));
        }

        [Test]
        public void GivenFlipRoKeyword_WhenGettingSearchUrl_ThenReturnsFlipRoUrl()
        {
            string result = searchService.GetSearchUrl("flip.ro laptop", "auto");

            Assert.That(result, Is.EqualTo("https://flip.ro/magazin/?search=laptop"));
        }

        [Test]
        public void GivenG2aKeyword_WhenGettingSearchUrl_ThenReturnsG2aUrl()
        {
            string result = searchService.GetSearchUrl("g2a cyberpunk", "auto");

            Assert.That(result, Is.EqualTo("https://g2a.com/search?query=cyberpunk"));
        }

        [Test]
        public void GivenGitHubKeyword_WhenGettingSearchUrl_ThenReturnsTextSearchWithGitHubSite()
        {
            string result = searchService.GetSearchUrl("github dotnet runtime", "auto");

            Assert.That(result.Contains("site%3Agithub.com%20dotnet%20runtime"));
        }

        [Test]
        public void GivenGogKeyword_WhenGettingSearchUrl_ThenReturnsGogUrl()
        {
            string result = searchService.GetSearchUrl("gog witcher", "auto");

            Assert.That(result, Is.EqualTo("https://gog.com/en/games?query=witcher"));
        }

        [Test]
        public void GivenHornbachKeyword_WhenGettingSearchUrl_ThenReturnsHornbachUrl()
        {
            string result = searchService.GetSearchUrl("hornbach vopsea", "auto");

            Assert.That(result, Is.EqualTo("https://hornbach.ro/s/vopsea"));
        }

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenIkeaKeyword_WhenGettingSearchUrl_ThenReturnsIkeaRoUrl()
        {
            string result = searchService.GetSearchUrl("ikea scaun", "auto");

            Assert.That(result, Is.EqualTo("https://ikea.com/ro/ro/search/?q=scaun"));
        }

        [Test]
        [SetUICulture("en-GB")]
        public void GivenIkeaKeyword_WhenGettingSearchUrl_ThenReturnsIkeaGbUrl()
        {
            string result = searchService.GetSearchUrl("ikea chair", "auto");

            Assert.That(result, Is.EqualTo("https://ikea.com/gb/en/search/?q=chair"));
        }

        [Test]
        public void GivenImdbKeyword_WhenGettingSearchUrl_ThenReturnsImdbUrl()
        {
            string result = searchService.GetSearchUrl("imdb inception", "auto");

            Assert.That(result, Is.EqualTo("https://libremdb.iket.me/find?q=inception"));
        }

        [Test]
        public void GivenInstagramKeyword_WhenGettingSearchUrl_ThenReturnsInstagramUrl()
        {
            string result = searchService.GetSearchUrl("instagram cats", "auto");

            Assert.That(result, Is.EqualTo("https://instagram.com/popular/cats"));
        }

        [Test]
        public void GivenJyskKeyword_WhenGettingSearchUrl_ThenReturnsJyskUrl()
        {
            string result = searchService.GetSearchUrl("jysk scaun", "auto");

            Assert.That(result, Is.EqualTo("https://jysk.ro/search?query=scaun"));
        }

        [Test]
        public void GivenLeroyMerlinKeyword_WhenGettingSearchUrl_ThenReturnsLeroyMerlinUrl()
        {
            string result = searchService.GetSearchUrl("leroy merlin vopsea", "auto");

            Assert.That(result, Is.EqualTo("https://leroymerlin.ro/produse/search/vopsea"));
        }

        [Test]
        public void GivenLidlKeyword_WhenGettingSearchUrl_ThenReturnsLidlUrl()
        {
            string result = searchService.GetSearchUrl("lidl cafea", "auto");

            Assert.That(result, Is.EqualTo("https://lidl.ro/q/search?q=cafea"));
        }

        [Test]
        public void GivenLinkedinKeyword_WhenGettingSearchUrl_ThenReturnsLinkedinUrl()
        {
            string result = searchService.GetSearchUrl("linkedin developer", "auto");

            Assert.That(result, Is.EqualTo("https://linkedin.com/search/results/all/?keywords=developer"));
        }

        [Test]
        public void GivenModDbKeyword_WhenGettingSearchUrl_ThenReturnsModDbUrl()
        {
            string result = searchService.GetSearchUrl("moddb half-life", "auto");

            Assert.That(result, Is.EqualTo("https://moddb.com/search?q=half-life"));
        }

        [Test]
        public void GivenMcWikiKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftWikiUrl()
        {
            string result = searchService.GetSearchUrl("mc wiki creeper", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft.wiki/?search=creeper"));
        }

        [Test]
        public void GivenMinecraftWikiKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftWikiUrl()
        {
            string result = searchService.GetSearchUrl("minecraft wiki creeper", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft.wiki/?search=creeper"));
        }

        [Test]
        public void GivenMcHeadKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
        {
            string result = searchService.GetSearchUrl("mc head dragon", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));
        }

        [Test]
        public void GivenMinecraftHeadKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
        {
            string result = searchService.GetSearchUrl("minecraft head dragon", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));
        }

        [Test]
        public void GivenMcSchematicKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
        {
            string result = searchService.GetSearchUrl("mc schematic castle", "auto");

            Assert.That(result, Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));
        }

        [Test]
        public void GivenMinecraftSchematicKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
        {
            string result = searchService.GetSearchUrl("minecraft schematic castle", "auto");

            Assert.That(result, Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));
        }

        [Test]
        public void GivenNameMcKeyword_WhenGettingSearchUrl_ThenReturnsNameMcUrl()
        {
            string result = searchService.GetSearchUrl("namemc Notch", "auto");

            Assert.That(result, Is.EqualTo("https://namemc.com/search?q=Notch"));
        }

        [Test]
        public void GivenNetflixKeyword_WhenGettingSearchUrl_ThenReturnsNetflixUrl()
        {
            string result = searchService.GetSearchUrl("netflix stranger things", "auto");

            Assert.That(result, Is.EqualTo("https://netflix.com/search?q=stranger%20things"));
        }

        [Test]
        public void GivenNexusModsKeyword_WhenGettingSearchUrl_ThenReturnsNexusModsUrl()
        {
            string result = searchService.GetSearchUrl("nexusmods skyrim", "auto");

            Assert.That(result, Is.EqualTo("https://nexusmods.com/search?keyword=skyrim"));
        }

        [Test]
        public void GivenOdyseeKeyword_WhenGettingSearchUrl_ThenReturnsOdyseeUrl()
        {
            string result = searchService.GetSearchUrl("odysee cooking", "auto");

            Assert.That(result, Is.EqualTo("https://odysee.com/$/search?q=cooking"));
        }

        [Test]
        public void GivenOlxKeyword_WhenGettingSearchUrl_ThenReturnsOlxUrl()
        {
            string result = searchService.GetSearchUrl("olx laptop", "auto");

            Assert.That(result, Is.EqualTo("https://olx.ro/d/oferte/q-laptop"));
        }

        [Test]
        public void GivenPcGarageKeyword_WhenGettingSearchUrl_ThenReturnsPcGarageUrl()
        {
            string result = searchService.GetSearchUrl("pcgarage ssd", "auto");

            Assert.That(result, Is.EqualTo("https://pcgarage.ro/cauta/ssd"));
        }

        [Test]
        public void GivenPinterestKeyword_WhenGettingSearchUrl_ThenReturnsPinterestUrl()
        {
            string result = searchService.GetSearchUrl("pinterest cats", "auto");

            Assert.That(result, Is.EqualTo("https://pinterest.com/search/pins/?q=cats"));
        }

        [Test]
        public void GivenPlanetMinecraftKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftUrl()
        {
            string result = searchService.GetSearchUrl("planet minecraft castle", "auto");

            Assert.That(result, Is.EqualTo("https://planetminecraft.com/resources/?keywords=castle"));
        }

        [Test]
        public void GivenPlayStoreKeyword_WhenGettingSearchUrl_ThenReturnsPlayStoreUrl()
        {
            string result = searchService.GetSearchUrl("play store spotify", "auto");

            Assert.That(result, Is.EqualTo("https://play.google.com/store/search?q=spotify"));
        }

        [Test]
        public void GivenPlaystoreKeyword_WhenGettingSearchUrl_ThenReturnsPlayStoreUrl()
        {
            string result = searchService.GetSearchUrl("playstore spotify", "auto");

            Assert.That(result, Is.EqualTo("https://play.google.com/store/search?q=spotify"));
        }

        [Test]
        public void GivenPlexKeyword_WhenGettingSearchUrl_ThenReturnsPlexUrl()
        {
            string result = searchService.GetSearchUrl("plex inception", "auto");

            Assert.That(result, Is.EqualTo("https://app.plex.tv/desktop/#!/search?pivot=top&query=inception"));
        }

        [Test]
        public void GivenProtonDbKeyword_WhenGettingSearchUrl_ThenReturnsProtonDbUrl()
        {
            string result = searchService.GetSearchUrl("protondb cyberpunk", "auto");

            Assert.That(result, Is.EqualTo("https://protondb.com/search?q=cyberpunk"));
        }

        [Test]
        public void GivenRedditKeyword_WhenGettingSearchUrl_ThenReturnsRedlibUrl()
        {
            string result = searchService.GetSearchUrl("reddit cats", "auto");

            Assert.That(result.Contains("search?q=cats"));
        }

        [Test]
        public void GivenRtingsKeyword_WhenGettingSearchUrl_ThenReturnsRtingsUrl()
        {
            string result = searchService.GetSearchUrl("rtings tv", "auto");

            Assert.That(result, Is.EqualTo("https://rtings.com/search?q=tv"));
        }

        [Test]
        public void GivenSinsayKeyword_WhenGettingSearchUrl_ThenReturnsSinsayUrl()
        {
            string result = searchService.GetSearchUrl("sinsay dress", "auto");

            Assert.That(result, Is.EqualTo("https://sinsay.com/ro/ro/?query=dress"));
        }

        [Test]
        public void GivenSpigotKeyword_WhenGettingSearchUrl_ThenReturnsSpigotUrl()
        {
            string result = searchService.GetSearchUrl("spigot worldedit", "auto");

            Assert.That(result, Is.EqualTo("https://spigotmc.org/search/294718421/?q=worldedit&o=relevance"));
        }

        [Test]
        public void GivenSpyshopKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
        {
            string result = searchService.GetSearchUrl("spyshop camera", "auto");

            Assert.That(result, Is.EqualTo("https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));
        }

        [Test]
        public void GivenSpyShopHyphenatedKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
        {
            string result = searchService.GetSearchUrl("spy-shop camera", "auto");

            Assert.That(result, Is.EqualTo("https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));
        }

        [Test]
        public void GivenSteamDbKeyword_WhenGettingSearchUrl_ThenReturnsSteamDbUrl()
        {
            string result = searchService.GetSearchUrl("steamdb half-life", "auto");

            Assert.That(result, Is.EqualTo("https://steamdb.info/search/?a=all&q=half-life"));
        }

        [Test]
        public void GivenTripadvisorKeyword_WhenGettingSearchUrl_ThenReturnsTripadvisorUrl()
        {
            string result = searchService.GetSearchUrl("tripadvisor paris", "auto");

            Assert.That(result, Is.EqualTo("https://tripadvisor.com/Search?q=paris"));
        }

        [Test]
        public void GivenTvDbKeyword_WhenGettingSearchUrl_ThenReturnsTvDbUrl()
        {
            string result = searchService.GetSearchUrl("tvdb breaking bad", "auto");

            Assert.That(result, Is.EqualTo("https://thetvdb.com/search?query=breaking%20bad"));
        }

        [Test]
        public void GivenTheTvDbKeyword_WhenGettingSearchUrl_ThenReturnsTvDbUrl()
        {
            string result = searchService.GetSearchUrl("thetvdb breaking bad", "auto");

            Assert.That(result, Is.EqualTo("https://thetvdb.com/search?query=breaking%20bad"));
        }

        [Test]
        public void GivenUespKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("uesp dragonborn", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dragonborn"));
        }

        [Test]
        public void GivenSkyrimWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("skyrim wiki dragonborn", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dragonborn"));
        }

        [Test]
        public void GivenEsoWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("eso wiki lorebook", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=lorebook"));
        }

        [Test]
        public void GivenElderScrollsWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("elder scrolls wiki dwemer", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dwemer"));
        }

        [Test]
        public void GivenVintedKeyword_WhenGettingSearchUrl_ThenReturnsVintedUrl()
        {
            string result = searchService.GetSearchUrl("vinted jacket", "auto");

            Assert.That(result, Is.EqualTo("https://vinted.com/catalog?search_text=jacket"));
        }

        [Test]
        public void GivenWikipediaKeyword_WhenGettingSearchUrl_ThenReturnsWikipediaInstanceUrl()
        {
            string result = searchService.GetSearchUrl("wikipedia cats", "auto");

            Assert.That(result.Contains("wikipedia.org") || result.Contains("wikiless"));
        }

        [Test]
        [SetUICulture("ro-RO")]
        public void GivenWikipediaKeywordAndRoRoCulture_WhenGettingSearchUrl_ThenReturnsRomanianWikipediaUrl()
        {
            string result = searchService.GetSearchUrl("wikipedia pisica", "auto");

            Assert.That(result.Contains("ro.wikipedia.org") || result.Contains("wikiless"));
        }

        [Test]
        [SetUICulture("en-GB")]
        public void GivenWikipediaKeywordAndEnGbCulture_WhenGettingSearchUrl_ThenReturnsEnglishWikipediaUrl()
        {
            string result = searchService.GetSearchUrl("wikipedia cats", "auto");

            Assert.That(result.Contains("en.wikipedia.org") || result.Contains("wikiless"));
        }

        [Test]
        public void GivenYoutubeKeyword_WhenGettingSearchUrl_ThenReturnsYewtubeUrl()
        {
            string result = searchService.GetSearchUrl("youtube cats", "auto");

            Assert.That(result, Is.EqualTo("https://yewtu.be/search?q=cats"));
        }

        [Test]
        public void GivenBoobpediaKeyword_WhenGettingSearchUrl_ThenReturnsBoobpediaUrl()
        {
            string result = searchService.GetSearchUrl("boobpedia elodia", "auto");

            Assert.That(result, Is.EqualTo("https://boobpedia.com/wiki/index.php?title=Special%3ASearch&search=elodia&go=Go"));
        }

        [Test]
        [SetUICulture("en-GB")]
        public void GivenFirefoxExtensionsPluralKeyword_WhenGettingSearchUrl_ThenReturnsFirefoxExtensionsUrl()
        {
            string result = searchService.GetSearchUrl("firefox extensions ublock", "auto");

            Assert.That(result, Is.EqualTo("https://addons.mozilla.org/en-GB/firefox/search/?q=ublock"));
        }

        [Test]
        public void GivenMcHeadsPluralKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
        {
            string result = searchService.GetSearchUrl("mc heads dragon", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));
        }

        [Test]
        public void GivenMinecraftHeadsPluralKeyword_WhenGettingSearchUrl_ThenReturnsMinecraftHeadsUrl()
        {
            string result = searchService.GetSearchUrl("minecraft heads dragon", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft-heads.com/custom-heads/search?searchterm=dragon"));
        }

        [Test]
        public void GivenMcSchematicsPluralKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
        {
            string result = searchService.GetSearchUrl("mc schematics castle", "auto");

            Assert.That(result, Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));
        }

        [Test]
        public void GivenMinecraftSchematicsPluralKeyword_WhenGettingSearchUrl_ThenReturnsPlanetMinecraftSchematicsUrl()
        {
            string result = searchService.GetSearchUrl("minecraft schematics castle", "auto");

            Assert.That(result, Is.EqualTo("https://planetminecraft.com/projects/?keywords=castle"));
        }

        [Test]
        public void GivenMorrowindWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("morrowind wiki dunmer", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=dunmer"));
        }

        [Test]
        public void GivenOblivionWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("oblivion wiki daedra", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=daedra"));
        }

        [Test]
        public void GivenTesWikiKeyword_WhenGettingSearchUrl_ThenReturnsUespUrl()
        {
            string result = searchService.GetSearchUrl("tes wiki shouts", "auto");

            Assert.That(result, Is.EqualTo("https://en.uesp.net/wiki/Special:Search?search=shouts"));
        }

        [Test]
        public void GivenSpyShopRoKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
        {
            string result = searchService.GetSearchUrl("spy-shop.ro camera", "auto");

            Assert.That(result, Is.EqualTo("https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));
        }

        [Test]
        public void GivenSpyshopRoKeyword_WhenGettingSearchUrl_ThenReturnsSpyShopUrl()
        {
            string result = searchService.GetSearchUrl("spyshop.ro camera", "auto");

            Assert.That(result, Is.EqualTo("https://spy-shop.ro/catalogsearch/result/?q=camera&o=relevance"));
        }

        // ── Domain blacklists ─────────────────────────────────────────────────

        [Test]
        public void GivenMinecraftQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("minecraft building", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenTerrariaQuery_WhenGettingTextSearchUrl_ThenIncludesArcenservFandomAndNeoseekerBlacklists()
        {
            string result = searchService.GetSearchUrl("terraria boss", "text");

            Assert.That(result.Contains("arcenserv") && result.Contains("fandom") && result.Contains("neoseeker"));
        }

        [Test]
        public void GivenBg3Query_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
        {
            string result = searchService.GetSearchUrl("bg3 build", "text");

            Assert.That(result.Contains("fextralife"));
        }

        [Test]
        public void GivenBg3Query_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("bg3 build", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenBorderlandsQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
        {
            string result = searchService.GetSearchUrl("borderlands guns", "text");

            Assert.That(result.Contains("fextralife"));
        }

        [Test]
        public void GivenBorderlandsQuery_WhenGettingTextSearchUrl_ThenIncludesHuijiwikiBlacklist()
        {
            string result = searchService.GetSearchUrl("borderlands guns", "text");

            Assert.That(result.Contains("huijiwiki"));
        }

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("osrs quest guide", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesNeoseekerBlacklist()
        {
            string result = searchService.GetSearchUrl("osrs quest guide", "text");

            Assert.That(result.Contains("neoseeker"));
        }

        [Test]
        public void GivenOsrsQuery_WhenGettingTextSearchUrl_ThenIncludesStrategywikiBlacklist()
        {
            string result = searchService.GetSearchUrl("osrs quest guide", "text");

            Assert.That(result.Contains("strategywiki"));
        }

        [Test]
        public void GivenFactorioQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("factorio base design", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenWarhammerQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("warhammer lore", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenWh40kQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("wh40k space marine", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void Given40kQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("40k tau build", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenSkyrimQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
        {
            string result = searchService.GetSearchUrl("skyrim archery build", "text");

            Assert.That(result.Contains("fextralife"));
        }

        [Test]
        public void GivenSkyrimQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("skyrim archery build", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenBaldurQuery_WhenGettingTextSearchUrl_ThenIncludesFextralifeBlacklist()
        {
            string result = searchService.GetSearchUrl("baldur paladin guide", "text");

            Assert.That(result.Contains("fextralife"));
        }

        [Test]
        public void GivenBaldurQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("baldur paladin guide", "text");

            Assert.That(result.Contains("fandom"));
        }

        // ── Deobfuscation ─────────────────────────────────────────────────────

        [Test]
        public void GivenObfuscatedQuery_WhenGettingSearchUrl_ThenDeobfuscatesBeforeSearching()
        {
            INuciTextObfuscator obfuscator = new NuciTextObfuscator(123456789);
            NuciTextObfuscatorOptions options = new() { UseApproximateReplacements = true };
            string obfuscatedQuery = obfuscator.Obfuscate("cats", options);

            string result = searchService.GetSearchUrl(obfuscatedQuery, "auto");

            Assert.That(result.StartsWith("https://search.brave.com/search?q=")
                     || result.StartsWith("https://duckduckgo.com/?q="));
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

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));
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
        {
            string result = searchService.GetSearchUrl("dark souls guide", "auto");

            Assert.That(result.StartsWith("https://search.brave.com/search?q=")
                     || result.StartsWith("https://duckduckgo.com/?q="));
        }
    }
}
