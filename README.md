[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/nucisearch)](https://github.com/hmlendea/nucisearch/releases/latest)

# NuciSearch

A lightweight and minimalist search engine wrapper built around existing search services.

NuciSearch provides a simple search interface that redirects queries to specialized engines such as DuckDuckGo, YouTube, or Google Maps, depending on the selected search mode.

The goal is to provide a **clean, fast, and dependency-free search page** that can be self-hosted and easily integrated with browsers via OpenSearch.

# Features

- Minimal and lightweight design
- No external dependencies
- Self-hostable static site
- OpenSearch support (can be installed as a browser search engine)
- Multiple search modes:
  - **Auto** → keyword-based routing or fallback text search
  - **Text** → randomized web search (Brave / DuckDuckGo)
  - **Images** → DuckDuckGo Image Search
  - **Torrents** → Yandex search with "Torrent" suffix
  - **Videos** → Yewtu.be (YouTube privacy frontend)
  - **Locations** → Google Maps
- Query parameter support (`?q=`)
- Works as a browser search provider

## Auto Integrations

When using **Auto** mode, queries are routed to specialized providers based on pattern matching or keywords.

### Pattern-based (no keyword needed)

- **JIRA** (Worldpay) — queries matching `AAP-NNN`, `AV-NNN`, `AND-NNN`, `CP-NNN`
- **Rally** — queries matching `DE`, `F`, or `US` followed by 6–8 digits
- **Currency exchange** — queries matching `[amount] [currency] in/to [currency]` (e.g. `100 EUR in USD`) → DuckDuckGo
  - Normalises `în` → `in`, `euro` → `EUR`, `lei`/`leu` → `RON`, `dollar`/`dollars`/`dolar`/`dolari` → `USD`, `lira`/`liră`/`lire` → `GBP`
  - Uppercases all 3-letter currency codes

### Keyword-triggered

- **AliExpress** — keyword: `aliexpress`
- **Altex** — keyword: `altex`
- **App Store** (Apple) — keyword: `appstore`, `app store`, or `apple store`
- **Arch Wiki** — keyword: `arch wiki`
- **Auchan** — keyword: `auchan`
- **Audible** — keyword: `audible`
- **Boobpedia** — keyword: `boobpedia`
- **Cărturești** — keyword: `carturesti`
- **Decathlon** — keyword: `decathlon`
- **Dedeman** — keyword: `dedeman`
- **Dex Online** — keyword: `dex`
- **Digi24** — keyword: `digi24`
- **eBay** — keyword: `ebay`
- **eMAG** — keyword: `emag`
- **evoMAG** — keyword: `evomag`
- **Facebook** — keyword: `facebook`
- **F-Droid** — keyword: `fdroid` or `f-droid`
- **Firefox Extensions** — keyword: `firefox extension` or `firefox extensions`
- **Flanco** — keyword: `flanco`
- **Flathub** — keyword: `flathub`
- **Flip.ro** — keyword: `flip.ro`
- **G2A** — keyword: `g2a`
- **GitHub** — keyword: `github`
- **GOG** — keyword: `gog`
- **Hornbach** — keyword: `hornbach`
- **IKEA** — keyword: `ikea`
- **IMDb** (via LibreMDb) — keyword: `imdb`
- **Instagram** — keyword: `instagram`
- **JYSK** — keyword: `jysk`
- **Leroy Merlin** — keyword: `leroy merlin`
- **Lidl** — keyword: `lidl`
- **LinkedIn** — keyword: `linkedin`
- **Minecraft Heads** — keyword: `mc head`, `mc heads`, `minecraft head`, or `minecraft heads`
- **Minecraft Wiki** — keyword: `mc wiki` or `minecraft wiki`
- **ModDB** — keyword: `moddb`
- **NameMC** — keyword: `namemc`
- **Netflix** — keyword: `netflix`
- **Nexus Mods** — keyword: `nexusmods` or `nexus mods`
- **Odysee** — keyword: `odysee`
- **OLX** — keyword: `olx`
- **PC Garage** — keyword: `pcgarage`
- **Pinterest** — keyword: `pinterest`
- **PlanetMinecraft** — keyword: `planet minecraft`
- **PlanetMinecraft Schematics** — keyword: `mc schematic(s)` or `minecraft schematic(s)`
- **Play Store** — keyword: `play store` or `playstore`
- **Plex** — keyword: `plex`
- **ProtonDB** — keyword: `protondb`
- **Reddit** (via Redlib) — keyword: `reddit`
- **Rtings** — keyword: `rtings`
- **Sinsay** — keyword: `sinsay`
- **Spigot** — keyword: `spigot`
- **Spy-Shop** — keyword: `spyshop`, `spyshop.ro`, `spy-shop`, or `spy-shop.ro`
- **SteamDB** — keyword: `steamdb`
- **TripAdvisor** — keyword: `tripadvisor`
- **TVDB** — keyword: `tvdb` or `thetvdb`
- **UESP** *(Unofficial Elder Scrolls Pages)* — keyword: `uesp`, `elder scrolls wiki`, `eso wiki`, `morrowind wiki`, `oblivion wiki`, `skyrim wiki`, `tes wiki`, or `the elder scrolls wiki`
- **Vinted** — keyword: `vinted`
- **Wikipedia** (via Wikiless) — keyword: `wikipedia`
- **YouTube** (via yewtu.be) — keyword: `youtube`

# Browser Integration

NuciSearch supports **OpenSearch**, allowing it to be installed as a search engine in browsers.

OpenSearch description: https://search.nuilandia.ro/opensearch.xml

# Self-Hosting

Because NuciSearch is a static site, hosting it is very simple.

Any static web server will work:
- Apache
- Nginx
- Caddy
- GitHub Pages
- other static hosting platforms

# Development

This project intentionally avoids frameworks and build systems.

Only standard web technologies are used:
- HTML
- CSS
- JavaScript

You can edit the files directly and reload the page.

# License

This project is licensed under the **GPLv3 License**.

See the `LICENSE` file for details.

# Support

If you find this project useful, consider funding its development: https://hmlendea.go.ro/fund.html