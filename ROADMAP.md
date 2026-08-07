# NuciSearch Roadmap

This roadmap outlines the planned evolution of NuciSearch, with periodic revision as priorities, feedback, and maintenance realities evolve.

## 📑 Table of Contents

- [Current Focus](#current-focus)
- [Planned Labour](#planned-labour)
  - [Keyword Expansion](#keyword-expansion)
  - [UI and Experience](#ui-and-experience)
  - [No Fixed Horizon](#no-fixed-horizon)
  - [Future Exploration](#future-exploration)
- [Objectives](#objectives)
- [Dependencies and Risks](#dependencies-and-risks)
- [Out of Scope](#out-of-scope)
- [Delivered Labour](#delivered-labour)
- [Contribution](#contribution)
- [Support](#support)

## 🎯 Current Focus

- **In Progress**: Search routing reliability hardening — Increase routing consistency for pattern-based and keyword-based flows under edge-case queries
- **Planned**: Test suite extension for search service behaviour — Increase confidence through broader unit coverage of provider routing and fallback behaviour

## 🗺️ Planned Labour

### Keyword Expansion

- Add broader provider keyword coverage for shopping, reference, and media categories
- Expand pattern-based routing for structured queries while reducing ambiguous matches

### UI and Experience

- Improve visual hierarchy, spacing, and interaction states for search mode controls
- Enhance accessibility, including keyboard navigation and clearer focus indicators

### No Fixed Horizon

- Evaluate optional administrative configuration for enabling or disabling selected integrations
- Consolidate documentation for self-hosted deployment patterns and browser integration

### Future Exploration

- Investigate lightweight caching strategies for repeated geolocation lookups
- Evaluate optional privacy-preserving analytics with explicit opt-in controls

## 🧱 Objectives

| Objective | Target Period | Result | Status |
|-----------|---------------|---------|--------|
| Stabilise auto-routing edge cases | Not time-bound | Fewer misroutes for ambiguous or malformed queries | In Progress |
| Expand keyword coverage depth | Not time-bound | Broader provider detection across supported query domains | Planned |
| Improve interface clarity and accessibility | Not time-bound | More intuitive and accessible user interaction flow | Planned |
| Prepare deployment guidance refinement | Not time-bound | Clearer operational documentation for maintainers and self-hosters | Planned |

## ⚠️ Dependencies and Risks

- External search provider behaviour changes — Routing outcomes may degrade without code changes — Mitigate with provider-focused regression tests and periodic verification
- Limited contributor bandwidth — Planned labour may shift between horizons — Mitigate by prioritising high-impact maintenance and focused pull requests
- Third-party service availability variance — User experience may fluctuate due to upstream outages — Mitigate with robust fallback paths where practical

## 🚫 Out of Scope

- Building and maintaining a proprietary web index
- Replacing upstream provider ranking algorithms

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions through repository issues and focused pull requests aligned with current priorities.

## 🆘 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/nucisearch/issues).
