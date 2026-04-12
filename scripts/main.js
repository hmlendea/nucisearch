const form = document.getElementById("search-form");
const queryInput = document.getElementById("query");


function getAliExpressUrl(query) {
    const hyphenated = query.trim().replace(/\s+/g, "-");
    return `https://www.aliexpress.com/w/wholesale-${hyphenated}.html?spm=a2g0o.detail.search.0`;
}
function getAltexUrl(query) { return "https://altex.ro/cauta/?q=" + encodeURIComponent(query); }
function getArchWikiUrl(query) { return "https://wiki.archlinux.org/index.php?search=" + encodeURIComponent(query); }
function getAuchanUrl(query) { return "https://auchan.ro/" + encodeURIComponent(query); }
function getCarturestiUrl(query) { return "https://carturesti.ro/product/search/" + encodeURIComponent(query); }
function getDecathlonUrl(query) { return "https://decathlon.ro/search?Ntt=" + encodeURIComponent(query); }
function getDexOnlineUrl(query) { return "https://dexonline.ro/definitie/" + encodeURIComponent(query); }
function getDigi24Url(query) { return "https://digi24.ro/cautare?q=" + encodeURIComponent(query); }
function getDuckDuckGoImagesUrl(query) { return "https://duckduckgo.com/?iax=images&ia=images&q=" + encodeURIComponent(query); }
function getEbayUrl(query) { return "https://ebay.com/sch/i.html?_nkw=" + encodeURIComponent(query); }
function getEmagUrl(query) { return "https://emag.ro/search/" + encodeURIComponent(query); }
function getEvomagUrl(query) { return "https://evomag.ro/?sn.q=" + encodeURIComponent(query); }
function getFdroidUrl(query) { return "https://search.f-droid.org/?q=" + encodeURIComponent(query); }
function getFirefoxExtensionsUrl(query) { return "https://addons.mozilla.org/en-US/firefox/search/?q=" + encodeURIComponent(query); }
function getFlancoUrl(query) { return "https://flanco.ro/catalogsearch/result/?q=" + encodeURIComponent(query); }
function getFlatHubUrl(query) { return "https://flathub.org/apps/search/" + encodeURIComponent(query); }
function getG2aUrl(query) { return "https://g2a.com/search?query=" + encodeURIComponent(query); }
function getGogUrl(query) { return "https://gog.com/en/games?query=" + encodeURIComponent(query); }
function getGoogleMapsUrl(query) { return "https://google.ro/maps/search/" + encodeURIComponent(query); }
function getIkeaUrl(query) { return "https://ikea.com/ro/ro/search/?q=" + encodeURIComponent(query); }
function getImdbUrl(query) { return "https://libremdb.iket.me/find?q=" + encodeURIComponent(query); }
function getInstagramUrl(query) { return "https://instagram.com/popular/" + encodeURIComponent(query); }
function getJyskUrl(query) { return "https://jysk.ro/search?query=" + encodeURIComponent(query); }
function getLeroyMerlinUrl(query) { return "https://leroymerlin.ro/produse/search/" + encodeURIComponent(query); }
function getLidlUrl(query) { return "https://lidl.ro/q/search?q=" + encodeURIComponent(query); }
function getMinecraftWikiUrl(query) { return "https://minecraft.wiki/?search=" + encodeURIComponent(query); }
function getNetflixUrl(query) { return "https://netflix.com/search?q=" + encodeURIComponent(query); }
function getOlxUrl(query) { return "https://olx.ro/d/oferte/q-" + encodeURIComponent(query); }
function getPcGarageUrl(query) { return "https://pcgarage.ro/cauta/" + encodeURIComponent(query); }
function getPlanetMinecraftUrl(query) { return "https://planetminecraft.com/resources/?keywords=" + encodeURIComponent(query); }
function getPlanetMinecraftSchematicsUrl(query) { return "https://planetminecraft.com/projects/?keywords=" + encodeURIComponent(query); }
function getPlayStoreUrl(query) { return "https://play.google.com/store/search?q=" + encodeURIComponent(query); }
function getPlexUrl(query) { return "https://app.plex.tv/desktop/#!/search?pivot=top&query=" + encodeURIComponent(query); }
function getProtonDbUrl(query) { return "https://protondb.com/search?q=" + encodeURIComponent(query); }
function getRallyUrl(query) { return "https://rally1.rallydev.com/#/search?keywords=" + encodeURIComponent(query); }
function getRedditUrl(query) {
    const searchQuery = encodeURIComponent(query);
    const instances = [
        `https://red.artemislena.eu/search?q=${searchQuery}`,
        `https://redlib.catsarch.com/search?q=${searchQuery}`,
        `https://redlib.cow.rip/search?q=${searchQuery}`,
        `https://redlib.nadeko.net/search?q=${searchQuery}`,
        `https://redlib.perennialte.ch/search?q=${searchQuery}`,
        `https://redlib.privadency.com/search?q=${searchQuery}`,
        `https://snoo.habedieeh.re/search?q=${searchQuery}`
    ];

    return instances[Math.floor(Math.random() * instances.length)];
}
function getSpigotUrl(query) { return "https://spigotmc.org/search/294718421/?q=" + encodeURIComponent(query) + "&o=relevance"; }
function getSteamDbUrl(query) { return "https://steamdb.info/search/?a=all&q=" + encodeURIComponent(query); }
function getTripadvisorUrl(query) { return "https://tripadvisor.com/Search?q=" + encodeURIComponent(query); }
function getUespUrl(query) { return "https://en.uesp.net/wiki/Special:Search?search=" + encodeURIComponent(query); }
function getVintedUrl(query) { return "https://vinted.com/catalog?search_text=" + encodeURIComponent(query); }
function getWikiPediaUrl(query) {
    const searchQuery = encodeURIComponent(query);
    const instances = [
        `https://ro.wikipedia.org/w/index.php?search=${searchQuery}`,
        `https://wikiless.tiekoetter.com/w/index.php?search=${searchQuery}`
    ];

    return instances[Math.floor(Math.random() * instances.length)];
}
function getYandexTorrentsUrl(query) { return "https://yandex.com/search/?text=" + encodeURIComponent(query + " Torrent") }
function getYouTubeUrl(query) { return "https://yewtu.be/search?q=" + encodeURIComponent(query); }

function getTextSearch(query) {
    const searchQuery = encodeURIComponent(query);
    const searchEngines = [
        `https://search.brave.com/search?q=${searchQuery}`,
        `https://duckduckgo.com/?q=${searchQuery}`,
        `https://qwant.com/?q=${searchQuery}`,
        `https://startpage.com/do/search?q=${searchQuery}`,
    ];

    return searchEngines[Math.floor(Math.random() * searchEngines.length)];
}

function runSearch(rawQuery) {
    const query = rawQuery
        .normalize("NFKC")
        .replace(/[\u200B-\u200D\uFEFF]/g, "")
        .replace(/\s+/g, " ")
        .trim();

    if (!query) return;

    const searchTypeElement = document.querySelector('input[name="search-type"]:checked');
    const searchType = searchTypeElement ? searchTypeElement.value : "auto";
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
        targetUrl = getTextSearch(query);
    } else {
        const words = query.split(/\s+/);

        if (/^(?:DE|F|US)[0-9]{6,8}$/.test(query)) {
            targetUrl = getRallyUrl(query);
        }
        else if (words.length >= 2) {
            if (words.some(w => w.toLowerCase() === "aliexpress")) {
                targetUrl = getAliExpressUrl(words.filter(w => w.toLowerCase() !== "aliexpress").join(" "));
            } else if (query.toLowerCase().includes("altex")) {
                targetUrl = getAltexUrl(query.replace(/altex/gi, "").trim());
            } else if (query.toLowerCase().includes("arch wiki")) {
                targetUrl = getArchWikiUrl(query.replace(/arch wiki/gi, "").trim());
            } else if (words.some(w => w.toLowerCase() === "auchan")) {
                targetUrl = getAuchanUrl(words.filter(w => w.toLowerCase() !== "auchan").join(" "));
            } else if (words.some(w => w.toLowerCase() === "carturesti")) {
                targetUrl = getCarturestiUrl(words.filter(w => w.toLowerCase() !== "carturesti").join(" "));
            } else if (words.some(w => w.toLowerCase() === "decathlon")) {
                targetUrl = getDecathlonUrl(words.filter(w => w.toLowerCase() !== "decathlon").join(" "));
            } else if (words.some(w => w.toLowerCase() === "dex")) {
                targetUrl = getDexOnlineUrl(words.filter(w => w.toLowerCase() !== "dex").join(" "));
            } else if (words.some(w => w.toLowerCase() === "digi24")) {
                targetUrl = getDigi24Url(words.filter(w => w.toLowerCase() !== "digi24").join(" "));
            } else if (words.some(w => w.toLowerCase() === "ebay")) {
                targetUrl = getEbayUrl(words.filter(w => w.toLowerCase() !== "ebay").join(" "));
            } else if (words.some(w => w.toLowerCase() === "emag")) {
                targetUrl = getEmagUrl(words.filter(w => w.toLowerCase() !== "emag").join(" "));
            } else if (words.some(w => w.toLowerCase() === "evomag")) {
                targetUrl = getEvomagUrl(words.filter(w => w.toLowerCase() !== "evomag").join(" "));
            } else if (words.some(w => w.toLowerCase() === "fdroid") ||
                       words.some(w => w.toLowerCase() === "f-droid")) {
                targetUrl = getFdroidUrl(words.filter(w => w.toLowerCase() !== "fdroid" &&
                                                           w.toLowerCase() !== "f-droid").join(" "));
            } else if (query.toLowerCase().includes("firefox extension") ||
                       query.toLowerCase().includes("firefox extensions")) {
                targetUrl = getFirefoxExtensionsUrl(query
                    .replace(/firefox extensions/gi, "")
                    .replace(/firefox extension/gi, "")
                    .trim());
            } else if (words.some(w => w.toLowerCase() === "flanco")) {
                targetUrl = getFlancoUrl(words.filter(w => w.toLowerCase() !== "flanco").join(" "));
            } else if (words.some(w => w.toLowerCase() === "flathub")) {
                targetUrl = getFlatHubUrl(words.filter(w => w.toLowerCase() !== "flathub").join(" "));
            } else if (words.some(w => w.toLowerCase() === "g2a")) {
                targetUrl = getG2aUrl(words.filter(w => w.toLowerCase() !== "g2a").join(" "));
            } else if (words.some(w => w.toLowerCase() === "gog")) {
                targetUrl = getGogUrl(words.filter(w => w.toLowerCase() !== "gog").join(" "));
            } else if (words.some(w => w.toLowerCase() === "ikea")) {
                targetUrl = getIkeaUrl(words.filter(w => w.toLowerCase() !== "ikea").join(" "));
            } else if (words.some(w => w.toLowerCase() === "imdb")) {
                targetUrl = getImdbUrl(words.filter(w => w.toLowerCase() !== "imdb").join(" "));
            } else if (words.some(w => w.toLowerCase() === "instagram")) {
                targetUrl = getInstagramUrl(words.filter(w => w.toLowerCase() !== "instagram").join(" "));
            } else if (words.some(w => w.toLowerCase() === "jysk")) {
                targetUrl = getJyskUrl(words.filter(w => w.toLowerCase() !== "jysk").join(" "));
            } else if (query.toLowerCase().includes("leroy merlin")) {
                targetUrl = getLeroyMerlinUrl(query.replace(/leroy merlin/gi, "").trim());
            } else if (words.some(w => w.toLowerCase() === "lidl")) {
                targetUrl = getLidlUrl(words.filter(w => w.toLowerCase() !== "lidl").join(" "));
            } else if (query.toLowerCase().includes("mc wiki") ||
                       query.toLowerCase().includes("minecraft wiki")) {
                targetUrl = getMinecraftWikiUrl(query
                    .replace(/mc wiki/gi, "")
                    .replace(/minecraft wiki/gi, "")
                    .trim());
            } else if (query.toLowerCase().includes("mc schematic") ||
                       query.toLowerCase().includes("mc schematics") ||
                       query.toLowerCase().includes("minecraft schematic") ||
                       query.toLowerCase().includes("minecraft schematics")) {
                targetUrl = getPlanetMinecraftSchematicsUrl(query
                    .replace(/mc schematics/gi, "")
                    .replace(/mc schematic/gi, "")
                    .replace(/minecraft schematics/gi, "")
                    .replace(/minecraft schematic/gi, "")
                    .trim());
            } else if (words.some(w => w.toLowerCase() === "netflix")) {
                targetUrl = getNetflixUrl(words.filter(w => w.toLowerCase() !== "netflix").join(" "));
            } else if (words.some(w => w.toLowerCase() === "olx")) {
                targetUrl = getOlxUrl(words.filter(w => w.toLowerCase() !== "olx").join(" "));
            } else if (words.some(w => w.toLowerCase() === "pcgarage")) {
                targetUrl = getPcGarageUrl(words.filter(w => w.toLowerCase() !== "pcgarage").join(" "));
            } else if (query.toLowerCase().includes("planet minecraft")) {
                targetUrl = getPlanetMinecraftUrl(query.replace(/planet minecraft/gi, "").trim());
            } else if (query.toLowerCase().includes("play store") ||
                       query.toLowerCase().includes("playstore")) {
                targetUrl = getPlayStoreUrl(query
                    .replace(/play store/gi, "")
                    .replace(/playstore/gi, "")
                    .trim());
            } else if (words.some(w => w.toLowerCase() === "plex")) {
                targetUrl = getPlexUrl(words.filter(w => w.toLowerCase() !== "plex").join(" "));
            } else if (words.some(w => w.toLowerCase() === "protondb")) {
                targetUrl = getProtonDbUrl(words.filter(w => w.toLowerCase() !== "protondb").join(" "));
            } else if (words.some(w => w.toLowerCase() === "reddit")) {
                targetUrl = getRedditUrl(words.filter(w => w.toLowerCase() !== "reddit").join(" "));
            } else if (words.some(w => w.toLowerCase() === "spigot")) {
                targetUrl = getSpigotUrl(words.filter(w => w.toLowerCase() !== "spigot").join(" "));
            } else if (words.some(w => w.toLowerCase() === "steamdb")) {
                targetUrl = getSteamDbUrl(words.filter(w => w.toLowerCase() !== "steamdb").join(" "));
            } else if (words.some(w => w.toLowerCase() === "tripadvisor")) {
                targetUrl = getTripadvisorUrl(words.filter(w => w.toLowerCase() !== "tripadvisor").join(" "));
            } else if (words.some(w => w.toLowerCase() === "uesp")) {
                targetUrl = getUespUrl(words.filter(w => w.toLowerCase() !== "uesp").join(" "));
            } else if (words.some(w => w.toLowerCase() === "vinted")) {
                targetUrl = getVintedUrl(words.filter(w => w.toLowerCase() !== "vinted").join(" "));
            } else if (words.some(w => w.toLowerCase() === "wikipedia")) {
                targetUrl = getWikiPediaUrl(words.filter(w => w.toLowerCase() !== "wikipedia").join(" "));
            } else if (words.some(w => w.toLowerCase() === "youtube")) {
                targetUrl = getYouTubeUrl(words.filter(w => w.toLowerCase() !== "youtube").join(" "));
            } else {
                targetUrl = getTextSearch(query);
            }
        } else {
            targetUrl = getTextSearch(query);
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