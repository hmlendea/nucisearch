[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucisearch)](https://github.com/hmlendea/nucisearch/releases/latest)
[![Build Status](https://github.com/hmlendea/nucisearch/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucisearch/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)
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

## Requirements

- .NET SDK/runtime with support for `net10.0`

## Auto Integrations

When using **Auto** mode, queries are routed to specialized providers based on pattern matching or keywords.

### Pattern-based (no keyword needed)

- **JIRA** (Worldpay) — queries matching `AAP-###`, `AV-###`, `AND-###`, `CP-###`
- **Rally** — queries matching `DE`, `F`, or `US` followed by 6–8 digits
- **Currency exchange** — queries matching `[amount] [currency] in/to [currency]` (e.g. `100 EUR in USD`) → DuckDuckGo
  - Normalises `în` → `in`, `euro` → `EUR`, `lei`/`leu` → `RON`, `dollar`/`dollars`/`dolar`/`dolari` → `USD`, `lira`/`liră`/`lire` → `GBP`
  - Uppercases all 3-letter currency codes
- **IP address lookup** — queries `my ip`, `current ip`, `my ip address`, or `current ip address` → DuckDuckGo
- **Wiki blacklists** — for media-related searches (video games, TV series, etc), non-primary wikis (fandom.com, wiki.fextralife.com, arcenserv.info, huijiwiki.com, neoseeker.com, strategywiki.org) are excluded in favour of the official or community-preferred wiki for that franchise, where applicable

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

NuciSearch is an ASP.NET Core Blazor Server application targeting **.NET 10**.

To publish and host it:

```bash
dotnet publish -c Release
```

The output can be deployed to any host that supports .NET 10:
- A VPS or bare-metal server running the .NET 10 runtime
- A reverse proxy (Nginx, Caddy, Apache) forwarding to the Kestrel process
- A container (Docker) with the .NET 10 runtime image
- Azure App Service or other PaaS platforms with .NET support

## Development

### Build

```bash
dotnet build
```

### test

```bash
dotnet test
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from: `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Contributing

Contributions are welcome. Please:
- Keep changes cross-platform
- Keep the existing public API intact unless a breaking change is intentional
- Keep pull requests focused and consistent with the existing code style
- Update documentation when behaviour changes

## Support

If you find this project useful, consider funding its development: https://hmlendea.go.ro/fund.html

## License

Licensed under the **GNU General Public License v3.0** or later.

See [LICENSE](./LICENSE) for details.