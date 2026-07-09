using NUnit.Framework;
using NuciSearch.Services;

namespace NuciSearch.Tests.Services
{
    [TestFixture]
    public sealed class SearchServiceTests
    {
        SearchService searchService;

        [SetUp]
        public void SetUp()
        {
            searchService = new SearchService();
        }

        // ── GetSearchUrl ──────────────────────────────────────────────────────

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

        [Test]
        public void GivenImagesSearchType_WhenGettingSearchUrl_ThenReturnsDuckDuckGoImagesUrl()
        {
            string result = searchService.GetSearchUrl("cats", "images");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?iax=images&ia=images&q=cats"));
        }

        [Test]
        public void GivenMapsSearchType_WhenGettingSearchUrl_ThenReturnsGoogleMapsUrl()
        {
            string result = searchService.GetSearchUrl("london", "maps");

            Assert.That(result, Is.EqualTo("https://google.ro/maps/search/london"));
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
        public void GivenJiraQuery_WhenGettingSearchUrl_ThenReturnsJiraUrl()
        {
            string result = searchService.GetSearchUrl("AAP-123", "auto");

            Assert.That(result, Is.EqualTo("https://worldpay.atlassian.net/browse/AAP-123"));
        }

        [Test]
        public void GivenRallyQuery_WhenGettingSearchUrl_ThenReturnsRallyUrl()
        {
            string result = searchService.GetSearchUrl("DE123456", "auto");

            Assert.That(result, Is.EqualTo("https://rally1.rallydev.com/#/search?keywords=DE123456"));
        }

        [Test]
        public void GivenSingleWordQuery_WhenGettingSearchUrl_ThenReturnsTextSearchUrl()
        {
            string result = searchService.GetSearchUrl("cats", "auto");

            Assert.That(result.StartsWith("https://search.brave.com/search?q=")
                     || result.StartsWith("https://duckduckgo.com/?q="));
        }

        [Test]
        public void GivenEmagKeyword_WhenGettingSearchUrl_ThenReturnsEmagUrl()
        {
            string result = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result, Is.EqualTo("https://emag.ro/search/laptop"));
        }

        [Test]
        public void GivenYoutubeKeyword_WhenGettingSearchUrl_ThenReturnsYewtubeUrl()
        {
            string result = searchService.GetSearchUrl("youtube cats", "auto");

            Assert.That(result, Is.EqualTo("https://yewtu.be/search?q=cats"));
        }

        [Test]
        public void GivenRedditKeyword_WhenGettingSearchUrl_ThenReturnsRedlibUrl()
        {
            string result = searchService.GetSearchUrl("reddit cats", "auto");

            Assert.That(result.Contains("search?q=cats"));
        }

        [Test]
        public void GivenWikipediaKeyword_WhenGettingSearchUrl_ThenReturnsWikipediaInstanceUrl()
        {
            string result = searchService.GetSearchUrl("wikipedia cats", "auto");

            Assert.That(result.Contains("wikipedia.org") || result.Contains("wikiless"));
        }

        [Test]
        public void GivenIpAddressQuery_WhenGettingSearchUrl_ThenReturnsDuckDuckGoUrl()
        {
            string result = searchService.GetSearchUrl("my ip address", "auto");

            Assert.That(result, Is.EqualTo("https://duckduckgo.com/?q=my%20ip%20address"));
        }

        [Test]
        public void GivenMinecraftQuery_WhenGettingTextSearchUrl_ThenIncludesFandomBlacklist()
        {
            string result = searchService.GetSearchUrl("minecraft building", "text");

            Assert.That(result.Contains("fandom"));
        }

        [Test]
        public void GivenTerrariaSkyrimQuery_WhenGettingTextSearchUrl_ThenIncludesMultipleBlacklists()
        {
            string result = searchService.GetSearchUrl("terraria boss", "text");

            Assert.That(result.Contains("arcenserv") && result.Contains("fandom") && result.Contains("neoseeker"));
        }

        [Test]
        public void GivenQueryWithSurroundingWhitespace_WhenGettingSearchUrl_ThenNormalisesWhitespace()
        {
            string result1 = searchService.GetSearchUrl("  emag  laptop  ", "auto");
            string result2 = searchService.GetSearchUrl("emag laptop", "auto");

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void GivenMinecraftWikiQuery_WhenGettingSearchUrl_ThenReturnsMinecraftWikiUrl()
        {
            string result = searchService.GetSearchUrl("minecraft wiki creeper", "auto");

            Assert.That(result, Is.EqualTo("https://minecraft.wiki/?search=creeper"));
        }

        [Test]
        public void GivenSteamDbKeyword_WhenGettingSearchUrl_ThenReturnsSteamDbUrl()
        {
            string result = searchService.GetSearchUrl("steamdb half-life", "auto");

            Assert.That(result, Is.EqualTo("https://steamdb.info/search/?a=all&q=half-life"));
        }
    }
}
