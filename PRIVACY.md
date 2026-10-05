# Privacy and Personal Data

This document describes how NuciSearch at https://github.com/hmlendea/nucisearch handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

**Information reviewed:** 2026-10-05

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [International Transfers](#international-transfers)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how NuciSearch at https://github.com/hmlendea/nucisearch handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

NuciSearch is a self-hosted ASP.NET Core Blazor Server application. This document covers the data handling behaviour of the software itself. Instance operators control their instance's configuration, local storage, logs, backups, access controls, retention, and request handling. The project maintainers do not operate user instances and do not receive data from self-hosted deployments unless the operator configures external integrations that transmit data.

The application sends the client IP address to ipwho.is (https://ipwho.is/) for geolocation lookup when a search request is processed. This is a required integration for the IP-based culture provider. Operators can disable this by modifying the `IpCultureProvider` or `GeolocationService` to use a local geolocation database instead.

No telemetry, update checks, crash reports, or other data is sent to project maintainers by default.

## 📥 Data We Handle

### Data Provided to the Application

- **Search queries** — The `q` query string parameter containing the user's search terms, submitted via the search form or OpenSearch integration.
- **Client IP address** — Automatically provided by the HTTP request context for geolocation-based culture selection.

### Data Generated or Collected by the Application

- **Application logs** — Structured logs via NuciLog including search queries, search types, operation status, and error details. Logs are written to a local file when `NuciLoggerSettings:isFileOutputEnabled` is true (default).
- **In-memory geolocation cache** — IP address to country code mappings cached for 24 hours using `IMemoryCache`.

### Data Received from Integrations

- **Country code** — Two-letter country code returned by ipwho.is for the client IP address.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- **Search query routing** — Search queries and search type are processed to determine the appropriate search engine URL and perform redirection.
- **Geolocation-based culture selection** — Client IP address is sent to ipwho.is to obtain a country code, which determines the UI culture (en-GB or ro-RO) for the session.
- **OpenSearch descriptor generation** — Localised description text is included in the OpenSearch XML response.
- **Logging** — Search queries, search types, IP addresses (in error contexts), operation status, and exceptions are logged for operational visibility.

## 🗄️ Storage, Retention, and Deletion

- **Log files** — Stored locally at the path configured by `NuciLoggerSettings:logFilePath` (default: `logfile.log`). Retention is controlled by the instance operator; the application does not implement automatic log rotation or deletion.
- **In-memory cache** — IP-to-country-code mappings stored in `IMemoryCache` with a 24-hour sliding expiration. Cache is cleared on application restart.
- **No persistent database** — The application does not use a database for personal data storage.

For self-hosted deployments, the instance operator controls storage location, log rotation, backups, and deletion of all local data.

## 🔗 External Processing and Integrations

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| ipwho.is | Geolocation lookup (IP to country code) | Client IP address | https://ipwho.is/ |

The application has no other built-in external data transfers.

## 🌍 International Transfers

The client IP address is sent to ipwho.is, which is an external service. The physical location of ipwho.is infrastructure is not controlled by the project. Instance operators who require data residency guarantees should replace the geolocation integration with a local database or self-hosted alternative.

## 🛡️ Data Protection and Security

- **Transport security** — The application uses HTTPS when deployed behind a reverse proxy with TLS termination (standard ASP.NET Core practice).
- **Input handling** — Search queries are validated and sanitised via pattern matching and obfuscation handling (NuciText.Obfuscation).
- **Error handling** — Exceptions are caught and logged without exposing stack traces to users; error details remain in server logs.
- **Secrets** — No secrets are embedded in the application. Configuration values (log file path, log level) are read from `appsettings.json`.

For self-hosted deployments, the instance operator is responsible for:
- Applying .NET and OS security updates
- Configuring TLS certificates and reverse proxy
- Securing log file access and backups
- Network exposure and firewall rules
- Access controls for the application endpoint

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/nucisearch/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers via GitHub Issues at https://github.com/hmlendea/nucisearch/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the instance URL or deployment identifier if applicable; do not send passwords, access tokens, or other secrets.