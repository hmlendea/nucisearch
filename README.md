[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucisearch)](https://github.com/hmlendea/nucisearch/releases/latest)
[![Build Status](https://github.com/hmlendea/nucisearch/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucisearch/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciSearch

NuciSearch is a lightweight self-hosted search wrapper that routes a query to an appropriate specialised engine based on the selected mode and query pattern.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Localisation](#localisation)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [Supporting the Project](#supporting-the-project)
- [License](#license)

## ✨ Capabilities

- Provides `auto`, `text`, `images`, `torrents`, `videos`, and `maps` search modes
- Performs pattern-based routing for common query formats (for example issue keys, Wikidata IDs, and currency conversion requests)
- Integrates with browsers through OpenSearch

## 🚀 Usage

Run the web application, open it in your browser, choose a search mode, and submit your query.

The application can also consume `q` from the query string and redirect automatically:

```text
https://search.nuilandia.ro/?q=minecraft%20wiki%20creeper
```

OpenSearch descriptor:

```text
https://search.nuilandia.ro/opensearch.xml
```

## 🖥️ System Requirements

- **OS:** Linux, MacOS, Windows
- **RAM:** 512 MB minimum
- **Runtime:** ASP.NET Core runtime compatible with .NET 10

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/nucisearch/releases)

You can deploy from the published GitHub release artefacts or execute from source using the development commands below.

## ⚙️ Configuration

All settings are loaded from the configuration file. The subsequent keys are recognised:

| Section | Key | Description |
|---------|-----|-------------|
| `Logging:LogLevel` | `Default` | Default application log level |
| `Logging:LogLevel` | `Microsoft.AspNetCore` | ASP.NET Core log level |
| `AllowedHosts` | `*` | Host filtering configuration |
| `NuciLoggerSettings` | `logFilePath` | Path to the log output file |
| `NuciLoggerSettings` | `isFileOutputEnabled` | Whether file logging is enabled |

## 🌍 Localisation

Translations are located in the project's localisation resources. The subsequent languages are currently supported:

| Language | Code | Status |
|----------|------|--------|
| English | `en` | Complete |
| Romanian | `ro-RO` | Complete |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

All NuGet dependencies are restored automatically by `dotnet restore`.

### Build

```bash
dotnet build NuciSearch
```

### Run

```bash
dotnet run --project NuciSearch
```

### Test

```bash
dotnet test NuciSearch.slnx
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Purpose |
|---------|---------|
| `NuciLog` | Structured application logging |
| `NuciLog.Core` | Core logging abstractions |
| `NuciText.Obfuscation` | Query deobfuscation |

## 🗂️ Project Structure

The solution contains the subsequent projects:

- `NuciSearch`: The primary ASP.NET Core Blazor Server application
- `NuciSearch.UnitTests`: Unit tests for the application services

The key directories inside `NuciSearch/` are:

| Directory | Purpose |
|-----------|---------|
| `Components/` | Blazor components, routes, layout, and pages |
| `Localisation/` | IP-based culture provider |
| `Logging/` | Logging operation names and log keys |
| `Resources/` | Localised resource files |
| `Services/` | Search routing and geolocation logic |
| `wwwroot/` | Static assets, styles, and OpenSearch descriptor |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Maintain the pull requests as focused and consistent with the existing code style
- Maintain your branch up-to-date with `master`
- Revise the documentation when behaviour changes
- Properly test all changes, including edge cases and error conditions
- Add unit tests for any new or changed functionality

## 💝 Supporting the Project

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nucisearch/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.