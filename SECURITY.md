# Security Policy

This document describes the security policy for NuciSearch, including supported versions, vulnerability reporting procedures, and disclosure expectations.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/nucisearch/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- Search query routing and redirection logic
- Geolocation IP lookup and country code resolution
- OpenSearch descriptor generation and query handling
- Query deobfuscation and pattern matching
- Localisation and culture provider behaviour
- Configuration and logging settings
- Dependency injection and service lifetime management

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Third-party search engine behaviour or vulnerabilities
- Browser integration issues outside OpenSearch specification
- Infrastructure, hosting, or network-layer concerns
- Operating system or .NET runtime vulnerabilities
- Social engineering or phishing via search results

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.