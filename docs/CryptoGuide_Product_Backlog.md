# CryptoGuide — Product Requirements, Architecture and Engineering Backlog

**Project status:** Planning baseline v1.0  
**Date:** 13 November 2026  
**Roles:** Developer — the project owner; Product Owner / Tech Lead — ChatGPT  
**Mode:** Learn by implementing manually. No complete solution is provided in advance.

## 1. Product vision

CryptoGuide is an educational cryptocurrency web app for beginners. Visitors learn concepts, explore market data, and practise with a **simulated** portfolio. Accounts unlock watchlists and saved progress. A Stripe **sandbox** subscription unlocks Premium capabilities. CryptoGuide does **not** execute trades, custody assets, accept deposits, or provide personalised financial advice.

### Target users
- **Guest learner:** wants to understand cryptocurrency and look up coins.
- **Registered beginner:** wants a watchlist, learning progress and a virtual portfolio.
- **Premium learner:** wants multiple watchlists, deeper portfolio insights and exports.

### Success criteria for the first release
- A guest can browse market information and complete public beginner lessons.
- A registered user can save favorites, complete quizzes and record virtual trades.
- A sandbox subscriber receives server-enforced Premium entitlements and can manage the subscription.
- Market API failures, payment problems and invalid input are handled deliberately.
- Core workflows have repeatable tests and can be demonstrated locally at zero provider-charge cost.

## 2. External API selection and costs

| Provider | Proposed role | Free access (checked 13 Nov 2026) | Restrictions / decision |
|---|---|---|---|
| CoinMarketCap (CMC) | Main market data and coin metadata | Keyless subset, or Basic tier: 15,000 monthly call credits, 50 requests/min | Basic advertises commercial use; endpoint credit costs vary. Primary provider. |
| Alternative.me `/fng/` | Fear & Greed market sentiment | Public free endpoint | Must attribute immediately next to the visualisation, including commercial display. |
| Stripe | Hosted Checkout, subscriptions, webhooks, billing portal | Sandbox test transactions move no real money | **Only sandbox** in v1. Live charges have fees. |
| CoinGecko | Development-only comparative market data | Demo: 10,000 calls/month, 100/min | Free Demo is **not** commercially licensed. Not an automatic production fallback. |
| CoinPaprika | Development-only comparative market data | Free: 20,000 calls/month/IP | Free is personal/non-commercial; redistribution is restricted. Not a production fallback. |
| DefiLlama | Optional DeFi education expansion | Selected public endpoints require no key | Paid endpoints and terms differ. Defer. |

Primary documentation:
- CMC: https://coinmarketcap.com/api/pricing/ and https://coinmarketcap.com/api/documentation/pro-api-reference/keyless-public-api/
- CMC API practices: https://coinmarketcap.com/api/documentation/guides/best-practices/
- Alternative.me: https://alternative.me/crypto/fear-and-greed-index/
- Stripe testing: https://docs.stripe.com/testing
- Stripe subscription webhooks: https://docs.stripe.com/billing/subscriptions/webhooks
- Stripe billing pricing (Netherlands): https://stripe.com/en-nl/billing/pricing
- CoinGecko: https://www.coingecko.com/en/api/pricing and https://support.coingecko.com/articles/1043089972-what-are-the-differences-between-commercial-and-custom-licenses
- CoinPaprika: https://coinpaprika.com/api/pricing
- .NET HTTP resilience: https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience

**Practical cost guardrails:** Run PostgreSQL, API and React locally. Use CMC keyless for the first integration and a free Basic key when appropriate. Cache read responses, display freshness timestamps, limit client refreshes, and monitor API credits. Do not enable Stripe live keys. Hosting/domain/email services and a production launch may cost money later, even if local development does not.

**Historical prices:** Do not promise historical BTC/ETH OHLCV from the CMC Basic plan. Access varies by endpoint/plan; keyless DEX candles are not interchangeable with a CEX coin-wide price chart. A general coin price chart is a *Could* item until a permitted, cost-free source and licence are validated. Never bulk-store third-party price history beyond licensing permission; legitimate operational caching is allowed subject to terms.

## 3. MoSCoW requirements

### MUST — essential to v1
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-01 | Public crypto market dashboard | Guest can load top 50 coins with price, rank, market cap and 24h change; data shows source and as-of time. |
| FR-02 | Coin details | Stable CMC ID routes to coin summary, price, 24h change, market cap and volume; unknown ID yields useful 404 state. |
| FR-03 | Learn basics | At least 6 beginner lessons and a searchable or browsable glossary; basic content is readable without an account. |
| FR-04 | Quizzes | At least one quiz; backend validates answers; signed-in users have persisted completion status. |
| FR-05 | Accounts | User can register, sign in, sign out; authenticated endpoints are protected and no other user’s private data is returned. |
| FR-06 | Free watchlist | A signed-in free user can save up to 10 unique CMC coin IDs in a default watchlist. |
| FR-07 | Simulated portfolio | A signed-in user records virtual buy/sell operations and views positions valued at current market prices; balance cannot go negative. No actual order placed. |
| FR-08 | Stripe subscription sandbox | A signed-in user completes test Checkout; verified webhooks control subscription state; cancel and failed-payment cases are handled. |
| FR-09 | Premium permissions | Server restricts advanced tools. Premium includes multiple watchlists (max 5, up to 50 coins each) and additional reports/export. A frontend-only Premium badge is insufficient. |
| FR-10 | Safe and understandable UI | Responsive layout, loading/error/empty states, accessible text and controls, clear simulated-data and education disclaimers. |
| NFR-01 | Security baseline | Server-side input validation, secure password handling via ASP.NET Core Identity, no secrets in Git/client, Stripe webhook signature verification, scoped queries by user ID. |
| NFR-02 | Functional correctness | Every feature has manual acceptance evidence; critical business logic and Premium access have automated tests. |

### SHOULD — important and planned
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-11 | Search and sorting | Search the loaded coin universe by name/symbol; users can sort by rank, price and 24h change without invalid results. |
| FR-12 | Market sentiment | Fear & Greed indicator includes current score, meaning, last-updated text and inline Alternative.me credit. |
| FR-13 | Free-tier efficiency and resilience | Backend caches market requests; timeout/retry/backoff/circuit-breaker and stale-cache fallback are tested against failures. |
| FR-14 | Subscription management | Customer can open Stripe billing portal and cancel in sandbox; access follows authoritative subscription status. |
| FR-15 | Portfolio analytics | Premium can see allocation percentages, cost basis and unrealised gain/loss; CSV export includes only owner’s virtual transactions. |
| NFR-03 | Observability | Structured logs, useful correlation IDs and safe error responses exist; API usage and failure scenarios are measurable. |

### COULD — later opportunities
| ID | Requirement | Acceptance criteria |
|---|---|---|
| FR-16 | Historical price charts | Chart only if a permitted free endpoint/licence is verified; missing data displays honest unavailability. |
| FR-17 | Multiple fiat currencies | Price conversions supported for at least USD/EUR if free-tier credits remain sufficient. |
| FR-18 | Advanced lesson paths | Further lessons on DeFi, custody, tokenomics, and risk with quizzes and saved progress. |
| FR-19 | Simple alerts | User-managed price threshold alerts, only if a cost-free and reliable delivery model is designed. |
| FR-20 | DefiLlama exploration | Educational DeFi metrics with definitions, source credits, and no unsupported financial claims. |

### WON'T — explicitly out of scope for v1
- Real crypto trading or exchange orders.
- Custodial wallets, seed phrases, or storing private keys.
- Paid market-data providers, real Stripe charges, or a production billing launch.
- AI investment recommendations / predictive buy-or-sell signals.
- Mobile native apps, microservices, Kubernetes, and deployment to paid cloud infrastructure.

## 4. Product and UX design

**Design direction:** Calm, beginner-friendly fintech. Light background, navy typography, restrained blue/teal accents; green/red changes must include +/− signs and text, not color alone. Uncluttered cards, short definitions near each financial term. Mobile-friendly at small widths. An accessible dark mode is optional later.

**Navigation:** Dashboard · Learn · Watchlist · Portfolio · Premium · Account. Guests can see Dashboard, Learn and Premium pricing, but prompts to sign in when saving data.

**Screens:**
1. **Dashboard `/`** — top-line market overview, small sentiment card, searchable/sortable top 50 table, CTA to learn a term.
2. **Coin details `/coins/:id`** — identity, price, change, market cap, volume, explanation of metrics, save-to-watchlist action.
3. **Learn `/learn` and `/learn/:slug`** — category list, lessons, glossary, quiz, completion feedback.
4. **Watchlists `/watchlists`** — saved coin entries with current prices, add/remove; Premium multi-list controls.
5. **Portfolio `/portfolio`** — virtual holdings, value, buy/sell simulation, transaction history; clearly labelled “Simulation”.
6. **Premium `/premium`** — comparison of benefits, Stripe-hosted Checkout in sandbox, billing management.
7. **Account `/account`** — authentication, profile basics, subscription state.

**UI states for every screen:** Loading skeleton or indicator; normal data; empty state with useful action; recoverable failure with retry; expired/stale market-data message as appropriate.

**Accessibility:** keyboard focus, semantic forms/tables, labels for controls, sufficient contrast, mobile overflow handling, accessible notification text.

## 5. Architecture and engineering conventions

**Style:** Modular monolith. One deployable .NET API and one React SPA. Avoid premature microservices, generic repositories, CQRS frameworks, Redis and message brokers.

```
React + TypeScript (Vite)
  -> /api (single-origin proxy in development)
ASP.NET Core Web API (.NET 10 LTS)
  -> feature/application services
  -> market adapter (ICryptoMarketProvider) -> CoinMarketCap
  -> sentiment adapter -> Alternative.me
  -> billing adapter -> Stripe
  -> EF Core -> PostgreSQL (Docker local)
  -> in-memory cache (added later)
```

**Backend:** ASP.NET Core Web API (.NET 10 LTS), dependency injection, `IHttpClientFactory` typed clients, async IO, EF Core migrations, ASP.NET Core Identity for password hashing/authentication, server-side authorization and validation, xUnit tests. For a same-origin SPA, prefer secure HttpOnly auth cookies with antiforgery protections on state-changing requests. Configure Vite `/api` proxy locally; re-evaluate deployment-origin behavior before launch. Do not build a homegrown password hashing or JWT security system.

**Frontend:** React, TypeScript strict mode, Vite, React Router, CSS modules or plain CSS to learn layout. First build data fetching with simple `fetch`/hooks so it is understood; later introduce TanStack Query if duplicate loading/caching logic justifies it. Vitest/React Testing Library added when frontend tests are due.

**Proposed structure:**
```
cryptoguide/
  backend/
    CryptoGuide.Api/
      Features/Markets, Features/Learning, Features/Watchlists,
      Features/Portfolio, Features/Billing, Features/Auth
      Infrastructure/ExternalApis, Infrastructure/Persistence
    CryptoGuide.Tests/
  frontend/
    src/pages, src/components, src/features, src/api, src/types
  docs/
  docker-compose.yml
  README.md
```

**API contract conventions:** `/api/...` JSON; standard HTTP status codes; unique integer CMC ID instead of symbols; UTC timestamps in ISO 8601; server handles pagination, validation, and entitlement checks; monetary values represented with appropriate precision (C# `decimal`) rather than `double` for holdings and prices; never expose provider-specific response envelopes as public API contracts.

**Potential endpoints (design targets; evolve during implementation):**
```
GET    /api/health
GET    /api/market/coins?limit=50&page=1
GET    /api/market/coins/{coinId}
GET    /api/market/global
GET    /api/market/sentiment
GET    /api/learning/lessons
GET    /api/learning/lessons/{slug}
POST   /api/learning/quizzes/{quizId}/submissions
GET    /api/me/progress
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
GET    /api/me
GET    /api/watchlists
POST   /api/watchlists
POST   /api/watchlists/{id}/coins
DELETE /api/watchlists/{id}/coins/{coinId}
GET    /api/portfolio
GET    /api/portfolio/transactions
POST   /api/portfolio/transactions
GET    /api/billing/entitlements
POST   /api/billing/checkout-session
POST   /api/billing/portal-session
POST   /api/billing/webhook
GET    /api/portfolio/export.csv
```

**Core data model:**
- `AppUser` (Identity-provided ID, account fields)
- `Lesson`, `Quiz`, `Question`, `AnswerOption`, `UserLessonProgress`
- `Watchlist` (`Id`, `UserId`, `Name`), `WatchlistItem` (`WatchlistId`, `CmcCoinId`, unique pair)
- `VirtualPortfolio` (`Id`, `UserId`, quote currency initially USD)
- `VirtualTransaction` (`Id`, `PortfolioId`, `CmcCoinId`, `Side`, `Quantity`, `UnitPriceUsd`, `OccurredAtUtc`)
- `BillingSubscription` (`UserId`, `StripeCustomerId`, `StripeSubscriptionId`, `Status`, `CurrentPeriodEndUtc`)
- `ProcessedStripeEvent` (`EventId` unique, `ProcessedAtUtc`) for webhook idempotency

**Portfolio semantics:** BUY/SELL events are virtual, recorded at a clearly displayed current/reference price. No margin, leverage, shorting or fees. The portfolio starts with zero holdings; user may simulate a BUY without a cash-balance model. SELL is rejected when owned quantity is insufficient. Basic view shows quantities and current holdings value. Premium analysis calculates cost basis and P/L using a documented weighted-average method; no promise of financial accuracy for real reporting.

**Security invariants:** All user-owned rows filtered by server-derived authenticated user ID; never trust a submitted user ID, role, `isPremium` flag, or client-provided Stripe status. Webhook signature validation over raw body. Checkout success redirect is never proof of payment. Validate all limits server-side. Store keys only in local user-secrets/environment variables, never commit them.

**Resilience strategy (after integrations work):**
- Market reads: shared cache + clearly labelled data age; bounded timeout; small exponential backoff for 429/5xx with jitter; circuit breaker; stale-cache fallback; graceful partial UI.
- Stripe operations: do **not** blindly retry subscription-creating POSTs. Use Stripe idempotency keys where supported, deduplicate webhook event IDs, handle out-of-order events by consulting authoritative subscription state when needed, and keep Premium entitlements consistent.
- Never retry invalid 4xx requests (except rate-limit handling) or repeatedly consume exhausted free API credits.

## 6. Ordered backlog

Each ticket below is scoped to a small, demonstrable slice. Every acceptance criterion (AC) must be shown working before the ticket is moved to Done. `M` = Must, `S` = Should, `C` = Could; `S/M/L` effort estimates are relative, not hours.

### Milestone A — Foundation (Sprint 0)

**CG-001 — Initialize repository and working backend (M, S)**
- AC: Git repository contains `backend`, `frontend`, `docs`, `.gitignore`, initial README.
- AC: An ASP.NET Core API created by hand runs locally; `GET /api/health` returns HTTP 200 with a simple JSON `status` value.
- AC: A meaningful first commit exists; `dotnet build` succeeds; no generated build folders are tracked.

**CG-002 — Initialize React/TypeScript frontend (M, S)**
- AC: Vite + React + TypeScript strict mode starts locally and displays a CryptoGuide landing shell.
- AC: Top navigation works at desktop and mobile widths; code is committed.

**CG-003 — Connect frontend to backend (M, S)**
- AC: React calls `/api/health` through a development proxy and renders backend status.
- AC: Stopping backend shows a controlled error, not an infinite spinner or unexplained crash.
- AC: Request and response can be inspected using browser DevTools and Postman.

**CG-004 — Set up tests, secrets and development instructions (M, S)**
- AC: Backend test project runs with `dotnet test`; one genuine assertion passes.
- AC: `.env.example`/user-secrets instructions exist; secrets and local DB passwords are not committed.
- AC: README explains local run steps, health endpoint and test command.

### Milestone B — Market data (Sprint 1)

**CG-005 — Integrate CMC keyless API in .NET (M, M)**
- AC: Typed HttpClient gets top 50 listing from documented CMC keyless URL, asynchronously.
- AC: Provider HTTP status, timeout and malformed JSON can be recognized and surfaced as application errors.
- AC: The external API is called **only** from backend. No CMC call occurs in React.

**CG-006 — Map external models into CryptoGuide DTOs (M, S)**
- AC: Public coin DTO contains `id`, `name`, `symbol`, `rank`, `priceUsd`, `marketCapUsd`, `volume24hUsd`, `change24hPercent`, `lastUpdatedUtc`.
- AC: Missing nullable provider values do not crash mapping, and at least two mapping tests pass.

**CG-007 — Create public market endpoints (M, S)**
- AC: `GET /api/market/coins?limit=50&page=1` returns consistent JSON and validates page/limit boundaries.
- AC: `GET /api/market/coins/{id}` returns coin details; unknown IDs return 404.
- AC: Swagger/OpenAPI shows routes; Postman tests both successful and error responses.

**CG-008 — Build market dashboard UI (M, M)**
- AC: Top 50 coins show rank/name/symbol/price/change/market cap in a responsive table.
- AC: Loading, failure and empty states are distinct; prices formatted consistently; data freshness/source visible.

**CG-009 — Build coin detail screen (M, S)**
- AC: Clicking a dashboard row navigates to `/coins/:id`; details load from CryptoGuide backend.
- AC: Unknown coin shows a clear not-found UI and back navigation.
- AC: Key metrics have novice-friendly definitions/tooltips.

**CG-010 — Search and sort the market list (S, S)**
- AC: Search by coin name/symbol filters within the clearly defined loaded dataset.
- AC: Sort by rank, price and 24h change works without mutating source data; reset works.
- AC: Search with zero matches shows a useful empty state.

### Milestone C — Education (Sprint 2)

**CG-011 — Model and seed lessons/glossary (M, M)**
- AC: At least six lessons: What is crypto?; Blockchain; Bitcoin vs altcoins; Wallets and private keys; Exchanges; Risks and scams.
- AC: At least 15 terms (e.g. market cap, volatility, seed phrase) have concise beginner definitions.
- AC: Lessons/glossary are available through defined API contracts and data are maintained in a reproducible seed source.

**CG-012 — Implement lesson read endpoints (M, S)**
- AC: Guest can list lessons and fetch by slug, with no sign-in required.
- AC: Unknown slug returns 404; lesson content is consistently ordered.

**CG-013 — Build learning and glossary pages (M, M)**
- AC: User can browse/read lessons and explanations on desktop/mobile.
- AC: Navigation to the next lesson works; glossary terms are discoverable.

**CG-014 — Implement quiz API (M, M)**
- AC: At least one quiz with five questions; server grades answers and returns score + educational explanations.
- AC: Client cannot set its own score; missing/invalid answers receive validation errors.

### Milestone D — Accounts and progress (Sprint 3)

**CG-015 — Set up local PostgreSQL and EF migrations (M, M)**
- AC: PostgreSQL starts via Docker Compose and app connects using local secrets/config.
- AC: A migration creates initial persistent tables; restarting containers preserves saved data in a named volume.

**CG-016 — Implement account authentication (M, L)**
- AC: Register, login, logout and current-user flows work using ASP.NET Core Identity.
- AC: Passwords are never stored as plaintext; duplicate email and wrong password are handled safely.
- AC: State-changing cookie-auth requests have appropriate antiforgery protections; protected route returns 401 to guests.

**CG-017 — Build account screens and session handling (M, M)**
- AC: React register/login/logout flows work and display form errors without leaking internal details.
- AC: Reload preserves appropriate authenticated session; guest navigation remains usable.

**CG-018 — Persist learning progress (M, M)**
- AC: Signed-in user can complete a lesson/quiz and see progress after logout/login.
- AC: Duplicate completions do not create duplicate rows; one user cannot read/change another user's progress.

### Milestone E — Watchlists and sentiment (Sprint 4)

**CG-019 — Persist default watchlist (M, S)**
- AC: Each newly registered user can own one default watchlist; watchlist ownership enforced in queries.
- AC: Unique `(WatchlistId, CmcCoinId)` index prevents duplicate coins.

**CG-020 — Add/remove watchlist coins via API (M, M)**
- AC: Free user can add/remove up to 10 unique coin IDs; invalid or over-limit additions fail with meaningful 4xx responses.
- AC: Another user's watchlist cannot be modified by guessing its identifier.

**CG-021 — Build watchlist UI (M, M)**
- AC: From a coin page user can save/unsave the coin; Watchlist page lists current quotes.
- AC: Sign-in needed and watchlist-full states are clearly shown; saved items persist across sessions.

**CG-022 — Integrate Alternative.me sentiment endpoint (S, S)**
- AC: Backend returns score, classification and timestamp from `GET https://api.alternative.me/fng/`.
- AC: Unavailable provider leads to controlled missing-data response, not a failed dashboard.

**CG-023 — Render sentiment education card (S, S)**
- AC: Dashboard shows 0–100 score, label and simple explanation of fear/greed.
- AC: The words “Source: Alternative.me” appear immediately next to the displayed value; missing data is explained.

### Milestone F — Virtual portfolio (Sprint 5)

**CG-024 — Model virtual portfolio and transactions (M, M)**
- AC: Migration defines user-owned portfolio and transaction rows using precise decimal quantities/prices and UTC timestamp.
- AC: Data model records BUY and SELL with references to stable CMC coin IDs.

**CG-025 — Implement virtual buy/sell business rules (M, L)**
- AC: Valid virtual BUY adds quantity; valid SELL reduces quantity; selling more than owned is rejected.
- AC: Zero/negative amounts, unknown coins and cross-user modifications are rejected.
- AC: Add tests for buy, partial sell, full sell, oversell and user ownership; concurrent conflicting sells cannot make holdings negative.

**CG-026 — Calculate basic current portfolio view (M, M)**
- AC: API returns holdings and current valuations using current CMC market quotes, with as-of timestamp.
- AC: API reports missing market quotes as unavailable rather than silently treating them as zero.

**CG-027 — Build portfolio dashboard UI (M, M)**
- AC: User sees holdings, quantities, current valuation and transaction history.
- AC: Persistent “Virtual portfolio — no real assets or orders” wording is visible.

**CG-028 — Build simulated trade form (M, M)**
- AC: Signed-in user can submit buy/sell quantities, sees reference quote and confirmation, and updated holdings.
- AC: Validation errors do not discard typed inputs unnecessarily; invalid double-submits are guarded.

### Milestone G — Stripe and Premium (Sprint 6)

**CG-029 — Configure Stripe sandbox and product (M, S)**
- AC: Stripe test-mode Product/recurring Price are configured; IDs and secrets stored outside Git.
- AC: Local README documents use of Stripe CLI/test account and unequivocally forbids live keys for v1.

**CG-030 — Implement Checkout Session initiation (M, M)**
- AC: Authorized backend endpoint creates a Stripe-hosted subscription Checkout Session for configured Price and logged-in user.
- AC: User is redirected to Stripe hosted Checkout; no credit card data touches our API.
- AC: Request does not allow arbitrary user IDs, price IDs or amounts; repeated creation attempts are handled deliberately.

**CG-031 — Verify and persist Stripe subscription webhooks (M, L)**
- AC: Backend verifies webhook signature against raw request body and returns 4xx for invalid signatures.
- AC: Handles `invoice.paid`, `invoice.payment_failed`, `customer.subscription.updated`, `customer.subscription.deleted` and relevant initial events.
- AC: Subscription state is saved against correct user; Checkout redirect alone never unlocks Premium.

**CG-032 — Create server-side Premium entitlements (M, M)**
- AC: Server decides from trusted subscription state whether user is Premium and exposes an entitlement endpoint.
- AC: Premium-only endpoints are denied to guests and Free users, even if React requests them directly.
- AC: Inactive/cancelled or failed subscription status follows explicitly documented access rules.

**CG-033 — Implement Premium features (M, L)**
- AC: Premium user can have up to 5 watchlists, each up to 50 coins; Free user remains capped at one/10.
- AC: Premium portfolio analytics show allocation percentages and unrealised P/L using documented cost-basis rules.
- AC: Premium CSV export contains only authenticated user's simulated transactions; Free user receives 403.

**CG-034 — Build Premium page and entitlement-driven UI (M, M)**
- AC: Free/Premium comparison, upgrade CTA, success/cancel pages and entitlement UI work in Stripe sandbox.
- AC: Loading or stale subscription state does not flash unauthorized Premium functionality.

**CG-035 — Integrate Stripe Customer Portal (S, M)**
- AC: Authenticated user can launch hosted portal only for their own Stripe Customer.
- AC: Cancellation/update in sandbox is reflected after webhook processing; a billing error leaves basic app available.

**CG-036 — Test negative and duplicate billing scenarios (M, M)**
- AC: Simulated payment success, failure, cancel and duplicate/reordered webhooks have repeatable test evidence.
- AC: Duplicate webhook ID does not repeat business side effects; unverified webhook cannot grant Premium.

### Milestone H — Resilience and quotas (Sprint 7)

**CG-037 — Add operational market response caching (S, M)**
- AC: Same market query within configured 5-minute TTL does not repeatedly call CMC.
- AC: Cached response records last fetch time and expiration; a clearly labelled stale response can be used when provider fails.
- AC: Cache invalidation and appropriate upper bounds are tested; cache use follows provider terms.

**CG-038 — Implement bounded timeouts and retry policy (S, M)**
- AC: Market outbound GET has explicit total/attempt timeout and jittered backoff for transient failures.
- AC: `429` respects `Retry-After` where present and bounded retries; routine 400/401 errors are not retried.
- AC: Tests with fake HttpMessageHandler verify requests and retry limits.

**CG-039 — Implement circuit breaker and graceful fallback (S, M)**
- AC: Repeated simulated 5xx/timeout failures open a circuit that avoids additional provider calls temporarily.
- AC: Last permitted cached market data shown with stale indication; with no cache, UI displays helpful failure.
- AC: Learning, sign-in and portfolio transaction history remain available when market data is down.

**CG-040 — Add API quota and provider observability (S, S)**
- AC: Logs/metrics report outbound requests, duration, 429s, failures and cache hit/miss without logging keys or user secrets.
- AC: Configured refresh and usage guardrails are documented; a basic quota-exceeded state is testable.

**CG-041 — Harden billing idempotency and recovery (M, M)**
- AC: Checkout creation uses appropriate idempotency where applicable; webhook event IDs have DB uniqueness protection.
- AC: Out-of-order subscription events cannot incorrectly leave a cancelled user Premium; authoritative Stripe status can be reconciled when necessary.
- AC: Transient Stripe failures do not corrupt entitlements or remove unrelated Free app access.

**CG-042 — Conduct failure-injection exercise (S, M)**
- AC: Simulate no internet, 429, 500, timeout, corrupted JSON, Stripe webhook replay and DB interruption.
- AC: Record observed API response, frontend message, logs and whether core features remain operational; fix at least two uncovered defects.

### Milestone I — Quality and release readiness (Sprint 8)

**CG-043 — Add API integration tests (M, M)**
- AC: Tests cover guest routes, authentication/ownership, watchlist limits, virtual sells and Premium authorization.
- AC: Repeatable test setup creates/cleans database state; tests do not require billable external API calls.

**CG-044 — Add frontend behavior tests and accessibility checks (S, M)**
- AC: Tests cover loading/error/empty/normal market states and at least one watchlist and portfolio interaction.
- AC: Keyboard navigation, form labels, table responsiveness and focus indication are manually checked.

**CG-045 — Final documentation, security review and demo (M, M)**
- AC: README explains setup, app architecture, API providers, usage/licensing limits, Stripe sandbox, migrations and tests.
- AC: Fresh-machine setup steps are verified; secrets scanning and basic input/authorization review find no critical issues.
- AC: Demo path: guest market → learn → register → watchlist → simulated trade → test subscription → Premium feature → graceful API outage.

### Later optional backlog (not included in v1 completion)

**CG-046 — Validate compliant chart source (C, M)**: Document a truly free licensed BTC/ETH history source, prove endpoint works, then design a time-series DTO.

**CG-047 — Implement supported charts (C, M)**: Show 7d/30d charts with stated source/freshness and correct empty/error states.

**CG-048 — Add USD/EUR preference (C, M)**: Conversion is consistent across market, portfolio and saved transactions; quota usage remains controlled.

**CG-049 — Expand learning paths (C, M)**: Add DeFi/tokenomics content, reliable glossary definitions, quizzes and completion progress.

**CG-050 — Explore user alerts (C, L)**: Design scheduled checks, notification delivery, deduplication, opt-out and free-tier cost first; build only if feasible.

## 7. Definition of Done (DoD)

A ticket is **Done** only when:
1. All ticket acceptance criteria are demonstrated manually (normal and error paths).
2. Appropriate automated test exists for significant behavior, or test debt is explicitly documented.
3. Backend builds and frontend typecheck/build succeed (where applicable).
4. No credentials are committed; validation and user ownership are respected.
5. Changes are committed with an explanatory message; README/docs updated where behavior changed.
6. Developer can explain the cause of at least one failure/debugging investigation encountered during the task.

**Suggested status workflow:** Backlog → Ready → In Progress → Review → Done (and Blocked when needed). **WIP limit:** one feature ticket at a time. Branch format: `feature/CG-005-cmc-client`; use [Conventional Commits](Conventional_Commits.md), for example `feat(api): CG-005 integrate CMC market data client`.

## 8. The teaching / leadership agreement

- Tech Lead assigns the **next single ticket**, its scope, AC, suggested concepts to research and testing expectations.
- Developer designs and writes all implementation by hand; asks questions, shares errors and code snippets for review.
- Tech Lead does not hand over a full implementation unless explicitly requested. Prefer probing questions, debugging strategy and small focused hints.
- At review, developer demonstrates behavior, tests and what was learned. If AC fail, the ticket goes back to In Progress with specific feedback.
- New features enter Backlog and receive MoSCoW priority and dependencies before being implemented.

### Debugging practice per ticket
- Reproduce: exact steps, inputs and expected vs actual result.
- Inspect: browser Network/Console, breakpoint or debugger, server logs, relevant request/status/body.
- Hypothesize: state the likely fault and one experiment that can disprove it.
- Fix: implement smallest justified change, rerun failing case and regression tests.
- Reflect: one-sentence root cause and preventive test.

## 9. Current assignment — CG-001

**Status: In Progress — review changes requested**  
**Review:** [CG-001 review, 13 November 2026](CG-001_Review.md). The developer will create the initial commit after addressing the findings.  
**Goal:** Set up an intentionally minimal project repository and runnable backend without implementing any external API yet.

**Developer deliverables:**
1. A Git repository called `cryptoguide` with `backend/`, `frontend/` and `docs/` directories and a sensible `.gitignore`.
2. Under `backend/`, a .NET 10 ASP.NET Core Web API project you created manually with CLI/IDE tooling (scaffolding is allowed; the application logic should be yours).
3. `GET /api/health` returning HTTP 200 and a small JSON object with a healthy status.
4. Confirmation from terminal/Postman that the endpoint works, a successful `dotnet build`, and a first Git commit.
5. One short description of how an HTTP request reaches your endpoint.

**Not in CG-001:** React setup, database, CMC API, authentication, Docker, Stripe, resilience, architecture layering.

**Review handoff:** Share your folder tree, command(s) used, HTTP status/response from Postman, build output, and any error you debugged. Tech Lead will review against the AC and assign CG-002 when complete.

---

**Living backlog:** v1.0 is intentionally open to further stories. Recheck API prices/quotas/terms before a public launch; this plan is a learning-project baseline, not permission to redistribute restricted data or operate a live paid service.
