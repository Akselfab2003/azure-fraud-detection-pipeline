# Changelog

One entry per step: what changed, and what it taught. Newest first. Edit the "Learned" lines in your own words before using them as interview material.

## 0.5 Rules engine v1

- Redis holds per-customer state; three rules (velocity, amount vs baseline, impossible travel) behind `IRule`; `RiskScorer` sums points and decides; `TransactionHandler` ties state, scoring and storage together.
- Option and reason-code names now match the spec (`RuleSetVersion`, `ReviewThreshold`, `VelocityMaxCount`, `VELOCITY_HIGH`, `AMOUNT_SPIKE`). `BlockThreshold` (90) produces `Decision.Block`; Review and Block both open a case.
- `NewMerchantBurstRule` counts merchants the customer had not used before the window (not just distinct merchants), skips customers with fewer than `MinHistory` transactions, and is registered. It reads history from Postgres, so rules are async and the scorer awaits them one at a time (a shared DbContext is not thread-safe).
- Tests use NSubstitute for rules, repositories, the state store and the clock.
- Transaction and case are saved in one `SaveChanges` (unit of work), so a crash can't leave a transaction without its case.
- Duplicate deliveries are skipped twice over: an up-front lookup, and a catch for the unique-key violation when two deliveries race.
- Learned: rules must see the state *before* the transaction, and state may only be updated *after* the save, or a retry double-counts.
- Learned: the customer state has to outlive the velocity window. A 10-minute expiry silently reset the rolling average; it now expires 24 hours after the last transaction.
- Learned: an amount baseline needs a minimum history (`MinHistory`), or the second transaction is judged against a baseline of one.
- Learned: the impossible-travel window must be longer than the fraud pattern's travel time, or the rule never fires.
- Baseline (2026-11-17, rules-v1: ReviewThreshold 50, BlockThreshold 90, velocity 40, amount 30, travel 40 points): **recall 0.0%**, 0 cases opened.

  | pattern | fraud | caught | recall | cases | precision | false positives |
  |---|---|---|---|---|---|---|
  | ALL | 10 | 0 | 0.0% | 0 | n/a | 0 |
  | ImpossibleTravel | 1 | 0 | 0.0% | 0 | n/a | 0 |
  | NewMerchantBurst | 9 | 0 | 0.0% | 0 | n/a | 0 |
  | None (normal traffic) | 0 | 0 | n/a | 0 | n/a | 0 |

  Only 10 fraud rows, so treat this as a smoke test, not a measurement. Likely cause of the zero: no single rule reaches ReviewThreshold 50 on its own (40, 30, 40), and there is no new-merchant rule yet. Re-run with more fraud rows and record the new numbers here.

## 0.4 Postgres and case storage

- `Fraud.Data` with the DbContext, entities and migrations; pgvector image with a data volume; migrations run on startup in Development only.
- Ground truth lives in its own table with no foreign key, so the scorer can never see the label. It also records when the label was written.

## 0.3 Realistic simulator

- Seeded customers and merchants, day/night traffic, four fraud patterns, configurable fraud share.

## 0.2 First message flow with tracing

- Simulator to Service Bus emulator to Scoring, with the client coming from the Aspire integration.

## 0.1 Repo and solution

- Solution with the AppHost, ServiceDefaults, Api, Scoring, Simulator, Contracts and Data projects.
- Nullable and warnings-as-errors are set once in `Directory.Build.props`.
- MIT license.
