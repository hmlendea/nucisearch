# NuciSearch Documentation

Comprehensive technical documentation for NuciSearch — a self-hosted search wrapper that routes queries to specialised engines based on mode and query patterns.

## Documentation Structure

| Document | Purpose |
|----------|---------|
| [Architecture Deep Dive](architecture-deep-dive.md) | Architectural decisions, patterns, and rationale beyond the root ARCHITECTURE.md |
| [Search Routing Logic](search-routing-logic.md) | Complete specification of query routing, pattern matching, and URL construction |
| [Localisation & Culture](localisation-culture.md) | IP-based culture detection, supported locales, and localisation architecture |
| [Logging & Observability](logging-observability.md) | Structured logging with NuciLog, operations, log info keys, and diagnostics |
| [Geolocation Service](geolocation-service.md) | IP→country resolution, caching strategy, and external integration |
| [Testing Strategy](testing-strategy.md) | Unit test patterns, coverage areas, and test organisation |
| [Deployment & Operations](deployment-operations.md) | Self-hosted deployment, configuration, scaling considerations |
| [API Reference](api-reference.md) | Service interfaces, method signatures, and usage examples |
| [Extensibility](extensibility.md) | Extension points for adding search providers, patterns, and behaviours |

## Relationship to Root Documentation

- **ARCHITECTURE.md** — System context, high-level architecture, runtime flow, cross-cutting concerns
- **SECURITY.md** — Vulnerability reporting, supported versions, disclosure policy
- **PRIVACY.md** — Data categories, processing purposes, storage, operator responsibilities
- **ROADMAP.md** — Planned features and milestones
- **This `docs/` directory** — Implementation-level detail, causal reasoning, and operational guidance

## Quick Navigation

### For Developers
- Start with [Architecture Deep Dive](architecture-deep-dive.md) for design rationale
- Reference [Search Routing Logic](search-routing-logic.md) when modifying query handling
- Use [API Reference](api-reference.md) for service contracts

### For Operators
- Review [Deployment & Operations](deployment-operations.md) for hosting guidance
- Consult [Logging & Observability](logging-observability.md) for diagnostics
- Check [Geolocation Service](geolocation-service.md) for external dependency behaviour

### For Contributors
- Read [Testing Strategy](testing-strategy.md) before adding tests
- Follow [Extensibility](extensibility.md) patterns for new features
- Understand [Localisation & Culture](localisation-culture.md) for i18n changes