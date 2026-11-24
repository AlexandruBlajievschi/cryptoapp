# Learning content and seeding (CG-011)

The initial curriculum contains six original CryptoGuide lessons and 25 glossary
terms. Content is maintained under `backend/Features/Learning/SeedData`: a JSON
lesson manifest, one Markdown file per lesson, and a JSON glossary. These files
are embedded in the backend assembly, so seeding works without internet access
and the same content ships with build/publish output.

## Curriculum

| Order | Slug | What the lesson teaches |
|---|---|---|
| 1 | `what-is-crypto` | Networks versus assets and prices; coins versus tokens; transactions; the limits of ownership claims. |
| 2 | `blockchain` | Hash-linked blocks, validation, conflicting spends, consensus, proof of work/stake, confirmation and finality. |
| 3 | `bitcoin-vs-altcoins` | Bitcoin versus other networks; Ethereum, ether and smart contracts; gas; why unit price and rank are misleading comparisons; market capitalisation. |
| 4 | `wallets-and-private-keys` | Public/private keys, addresses, wallets, recovery phrases, self-custody versus custodians, and secret-sharing scams. |
| 5 | `exchanges` | Centralised/decentralised venues, internal balances versus withdrawals, network compatibility, fees, spread, slippage and liquidity. |
| 6 | `risks-and-scams` | Market, custody, key and contract risks; stablecoin depegging; impersonation, fake pages, guaranteed-return claims and misleading audit claims. |

Every lesson includes a learning goal, explanation, fictional example, common
misconception, takeaways and related glossary terms. Content is plain Markdown;
the future UI must render it safely. Examples involve no real funds, keys or
orders. The portfolio is explicitly described as a simulation.

## Glossary

All 25 definitions are maintained by CryptoGuide in `glossary.json`.
The glossary has no external API dependency or source-metadata fields.

Included terms: Blockchain, Hash, Public key, Private key, Wallet, Custody,
Consensus, Proof of work, Proof of stake, Finality, Smart contract, Stablecoin,
Gas, Bitcoin, Ethereum, Cryptocurrency, Coin, Token, Altcoin, Seed phrase,
Exchange, Market capitalisation, Volatility, Liquidity and Transaction.

These terms support the six introductory lessons. More advanced protocol
profiles, enterprise architecture and historical failure case studies can be
added when the curriculum needs them.

## Seeding and updates

`Program.cs` registers both `UseSeeding` and `UseAsyncSeeding`, following
[EF Core's seeding guidance](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding).
Run from the repository root with the local PostgreSQL container healthy:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project backend/backend.csproj
```

EF applies pending migrations and then seeds under its
migration lock, even when no migrations remain to apply. No content is fetched
from the internet. `LearningSeedCatalog` validates the embedded content before
the seeder changes any rows. Normal API startup does not migrate or seed.

The catalog is authoritative for entries it owns: lessons are matched by stable
slug and glossary entries by exact term. Missing entries are inserted; matching
entries have their content and order refreshed without replacing
their IDs. Database edits to those fields are overwritten on the next seed run;
edit the version-controlled source files instead. Unrelated rows are retained.
Removing an item from the catalog does not delete its database row. Renaming a
slug or term needs a deliberate data migration to preserve identity.

Both paths save their changes atomically through `SaveChanges`. Seeding is
skipped when the initial schema migration is not applied, including a downgrade
to an empty database. Use migrations to initialise this
application; `EnsureCreated` is not a supported replacement for that workflow.
SQL migration scripts alone do not execute the seeding callbacks.

`Features/Learning/Contracts` defines `LessonSummaryDto`, `LessonDto` (Markdown
content), and `GlossaryTermDto` (term and definition). CG-012 will connect
these contracts to public read endpoints; no learning endpoints are added here.

## Verification

CG-011 acceptance review:

| Requirement | Evidence |
|---|---|
| Six required beginner lessons | Six Markdown files and an ordered lesson manifest; catalog tests cover all required topics. |
| At least 15 concise glossary definitions | Twenty-five locally maintained entries; tests check uniqueness and nonempty definitions. |
| Defined API contracts and reproducible seed source | Lesson summary/detail and glossary DTOs; embedded JSON/Markdown; repeated PostgreSQL seeding preserves rows and IDs. |

Public HTTP reads are the next ticket, CG-012. No frontend files changed in
CG-011, so frontend validation is not applicable to this change.

Verified locally on 24 November 2026:

- PostgreSQL on port 5432 uses the initial learning schema. Glossary rows have
  only `Id`, `Term` and `Definition` columns.
- Six nonempty lessons exist with unique slugs and order 1 through 6.
- Twenty-five glossary terms exist with locally maintained CryptoGuide definitions.
- Running `dotnet ef database update` a second time left row counts and full-row
  fingerprints unchanged, including IDs and content.
- Catalog tests check required topics, sections, related-term references,
  glossary uniqueness, nonempty definitions and independent deserialization.
- All 74 backend tests pass; EF reports no model changes pending a migration.
