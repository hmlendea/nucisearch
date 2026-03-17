const form = document.getElementById("search-form");
const queryInput = document.getElementById("query");


function getAliExpressUrl(query) {
    const hyphenated = query.trim().replace(/\s+/g, "-");
    return `https://www.aliexpress.com/w/wholesale-${hyphenated}.html?spm=a2g0o.detail.search.0`;
}
function getArchWikiUrl(query) { return "https://wiki.archlinux.org/index.php?search=" + encodeURIComponent(query); }
function getDuckDuckGoImagesUrl(query) { return "https://duckduckgo.com/?iax=images&ia=images&q=" + encodeURIComponent(query); }
function getDuckDuckGoUrl(query) { return "https://duckduckgo.com/?q=" + encodeURIComponent(query); }
function getEmagUrl(query) { return "https://emag.ro/search/" + encodeURIComponent(query); }
function getGoogleMapsUrl(query) { return "https://google.ro/maps/search/" + encodeURIComponent(query); }
function getYandexTorrentsUrl(query) { return "https://yandex.com/search/?text=" + encodeURIComponent(query + " Torrent") }
function getYouTubeUrl(query) { return "https://yewtu.be/search?q=" + encodeURIComponent(query); }


function runSearch(rawQuery) {
    const query = rawQuery.trim();
    if (!query) return;

    const searchType = document.querySelector('input[name="search-type"]:checked').value;
    let targetUrl;

    if (searchType === "images") {
        targetUrl = getDuckDuckGoImagesUrl(query);
    } else if (searchType === "maps") {
        targetUrl = getGoogleMapsUrl(query);
    } else if (searchType === "torrents") {
        targetUrl = getYandexTorrentsUrl(query);
    } else if (searchType === "videos") {
        targetUrl = getYouTubeUrl(query);
    } else if (searchType === "text") {
        targetUrl = getDuckDuckGoUrl(query);
    } else {
        const words = query.split(/\s+/);

        if (words.length >= 2) {
            if (words.some(w => w.toLowerCase() === "aliexpress")) {
                targetUrl = getAliExpressUrl(words.filter(w => w.toLowerCase() !== "aliexpress").join(" "));
            } else if (query.toLowerCase().includes("arch wiki")) {
                targetUrl = getArchWikiUrl(query.replace(/arch wiki/gi, "").trim());
            } else if (words.some(w => w.toLowerCase() === "youtube")) {
                targetUrl = getYouTubeUrl(words.filter(w => w.toLowerCase() !== "youtube").join(" "));
            } else if (words.some(w => w.toLowerCase() === "emag")) {
                targetUrl = getEmagUrl(words.filter(w => w.toLowerCase() !== "emag").join(" "));
            } else {
                targetUrl = getDuckDuckGoUrl(query);
            }
        } else {
            targetUrl = getDuckDuckGoUrl(query);
        }
    }

    window.location.href = targetUrl;
}

form.addEventListener("submit", function (event) {
    event.preventDefault();
    runSearch(queryInput.value);
});

window.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    const queryFromUrl = params.get("q");

    if (queryFromUrl && queryFromUrl.trim()) {
        queryInput.value = queryFromUrl;
        runSearch(queryFromUrl);
    }
});