# Azure Fraud Detection Pipeline

## Database schema

Postgres (pgvector image), managed by EF Core in `Fraud.Data`. Migrations are applied automatically on startup in Development only. Names are snake_case, enums are stored as strings, timestamps are `timestamptz`.

| Table | Written by | Holds |
|---|---|---|
| `transactions` | Scoring | Every scored transaction with its risk score and decision. Primary key is the transaction id, so redelivered messages can't create duplicates. Indexed on `(customer_id, occurred_at)`. |
| `cases` | Scoring (create), Api (status and label) | One case per Review/Block transaction (unique FK to `transactions`): risk score, reason codes (jsonb), rule set and model versions, status, analyst label, created time. Indexed on `status`. `xmin` is used as the concurrency token. |
| `case_events` | Scoring, Api, explainer | Append-only audit trail per case (opened, explained, status changed, labelled). |
| `ground_truth` | Simulator only | The true `is_fraud` label and fraud pattern per transaction. Deliberately separate from `transactions` and `cases`, with no foreign key, so the scorer can never see the label. It exists only to measure recall afterwards. |

Run the integration tests (needs Docker, uses Testcontainers): `dotnet test --project tests/Fraud.Data.Tests`
