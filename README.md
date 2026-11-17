[![CI](https://github.com/Akselfab2003/azure-fraud-detection-pipeline/actions/workflows/ci.yml/badge.svg)](https://github.com/Akselfab2003/azure-fraud-detection-pipeline/actions/workflows/ci.yml)
# Azure Fraud Detection Pipeline

Event-driven card-fraud detection on Azure (.NET 11, Aspire, Service Bus). A simulator publishes labelled synthetic transactions to a queue, a scoring worker scores them with rules and stores cases for analysts. Synthetic data only.

Status: local pipeline (steps 0.1 to 0.5 of the build spec) works end to end. CI and the Azure deploy are not done yet.

## Run it locally

You need the .NET SDK (see `global.json`) and Docker running.

```bash
dotnet run --project src/Fraud.AppHost
```

This starts everything and opens the Aspire dashboard: the Service Bus emulator, Postgres (pgvector), Redis, and the Api, Scoring and Simulator projects. In Development the database migrations run on startup.

## How a transaction flows

```
Simulator ──► Service Bus queue ──► Scoring ──► Postgres (transactions, cases)
    │                                  │
    └─► ground_truth (label)           └─► Redis (per-customer state)
```

1. The Simulator writes the fraud label to `ground_truth`, then publishes a `TransactionCreated` message. The label never travels on the message.
2. Scoring loads the customer's state from Redis, runs the rules, stores the transaction with its score, and opens a case if the score reaches the threshold.
3. Scoring then updates the customer state and completes the message. A duplicate delivery is detected through the unique transaction id and skipped.

## Rules

Each rule is a class behind `IRule` and returns points plus a reason code. The risk score is the sum of the points; a score at or above `ReviewThreshold` opens a case (Review), and at or above `BlockThreshold` it is a Block, which also opens a case. Both thresholds are inclusive.

| Rule | Reason code | Fires when |
|---|---|---|
| `VelocityRule` | `VELOCITY_HIGH` | More than `VelocityMaxCount` transactions (including this one) within `VelocityWindow` |
| `AmountBaselineRule` | `AMOUNT_SPIKE` | Amount is above `AmountMultiplier` times the customer's average, once they have at least `MinHistory` transactions |
| `ImpossibleTravelRule` | `IMPOSSIBLE_TRAVEL` | Different country than the last transaction, within `ImpossibleTravelWindow` |
| `NewMerchantBurstRule` | `NEW_MERCHANT_BURST` | At least `NewMerchantBurstThreshold` merchants the customer never used before, within `NewMerchantBurstWindowSeconds` (the current transaction counts). Needs `MinHistory` transactions first; reads stored transactions from Postgres |

All thresholds, windows and points live in the `Scoring` section of `src/Fraud.Scoring/appsettings.json` (`ScoringOptions`).

Customer state in Redis (recent transaction times, last country and time, rolling average, count) expires 24 hours after the customer's last transaction.

## Database schema

Postgres (pgvector image), managed by EF Core in `Fraud.Data`. Migrations are applied automatically on startup in Development only. Names are snake_case, enums are stored as strings, timestamps are `timestamptz`.

| Table | Written by | Holds |
|---|---|---|
| `transactions` | Scoring | Every scored transaction with its risk score and decision. Primary key is the transaction id, so redelivered messages can't create duplicates. Indexed on `(customer_id, occurred_at)`. |
| `cases` | Scoring (create), Api (status and label) | One case per Review/Block transaction (unique FK to `transactions`): risk score, reason codes (jsonb), rule set and model versions, status, analyst label, created time. Indexed on `status` and `(status, created_at)`. `xmin` is used as the concurrency token. |
| `case_events` | Scoring, Api, explainer | Append-only audit trail per case (opened, explained, status changed, labelled). |
| `ground_truth` | Simulator only | The true `is_fraud` label, the fraud pattern and when it was written (`created_at`, indexed) per transaction. Deliberately separate from `transactions` and `cases`, with no foreign key, so the scorer can never see the label. It exists only to measure recall afterwards. |

## Tests

Integration tests use Testcontainers (Postgres and Redis), so Docker must be running.

```bash
dotnet test                                          # everything
dotnet test --project tests/Fraud.Scoring.Tests      # rules, scorer, Redis store, handler
dotnet test --project tests/Fraud.Data.Tests         # schema and repositories
dotnet test --project tests/Fraud.Simulator.Tests    # traffic generation and fraud patterns
```

The build treats warnings as errors.

## License

MIT, see [LICENSE](LICENSE).
