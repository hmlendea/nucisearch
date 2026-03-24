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
  - **Text** → randomized web search (Brave / DuckDuckGo / Qwant / Startpage)
  - **Images** → DuckDuckGo Image Search
  - **Torrents** → Yandex search with "Torrent" suffix
  - **Videos** → Yewtu.be (YouTube privacy frontend)
  - **Locations** → Google Maps
- Query parameter support (`?q=`)
- Works as a browser search provider

## Auto Integrations

When using **Auto** mode, queries can be routed to specialized providers based on keywords:

- Altex
- AliExpress
- ArchWiki
- eMAG
- evoMAG
- Flathub
- IMDb (via LibreMDb)
- Minecraft Wiki
- PlanetMinecraft (resources/schematics)
- ProtonDB
- UESP
- YouTube (via yewtu.be)

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