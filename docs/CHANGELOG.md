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
- Baseline 2 (2026-11-17, rules-v1: ReviewThreshold 40, BlockThreshold 90, four rules, no warm-up): **recall 16.7%, precision 100%, 0 false positives**.

  | pattern | fraud | caught | recall | cases | precision | false positives |
  |---|---|---|---|---|---|---|
  | ALL | 6 | 1 | 16.7% | 1 | 100.0% | 0 |
  | ImpossibleTravel | 1 | 1 | 100.0% | 1 | 100.0% | 0 |
  | NewMerchantBurst | 5 | 0 | 0.0% | 0 | n/a | 0 |
  | None (normal traffic) | 0 | 0 | n/a | 0 | n/a | 0 |

  Still a tiny sample (6 fraud rows, and the 5 NewMerchantBurst rows are probably one burst). The burst was missed most likely because the victim had under `MinHistory` transactions in a freshly started system (cold start).
- Baseline 1 (2026-11-17, ReviewThreshold 50, three rules): recall 0.0% on 10 fraud rows, 0 cases. Kept for comparison.
