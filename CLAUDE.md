# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Event-driven card-fraud detection pipeline on Azure (.NET 11, Aspire, Service Bus), built as a portfolio project. A simulator publishes labelled synthetic transactions to a queue; a scoring worker scores them (rules, later a Python anomaly model) and stores cases; an API serves analysts; an LLM only *explains* cases and never changes a score. Synthetic data only.

The full plan lives in a Claude artifact titled "Fraud Detection Pipeline on Azure — Project Plan" (tabs: Plan, **Build spec**, Data model). Find it by that name in the user's artifact list. The Build spec tab is the authoritative step list (M0–M4, steps 0.1–4.5); where tabs disagree it wins. Read it with the Claude Docs connector, not by web fetch.

## Commands

```bash
dotnet build azure-fraud-detection-pipeline.slnx
dotnet run --project src/Fraud.AppHost        # runs everything locally + Aspire dashboard (needs Docker for the Service Bus emulator)
dotnet test                                    # integration tests (Fraud.Data.Tests) need Docker (Testcontainers)
dotnet test --filter "FullyQualifiedName~SomeTest"   # single test, once xUnit projects exist
```

- Solution file is `.slnx`. `src/aspire.config.json` points the Aspire CLI at `Fraud.AppHost`.
- Package versions are centrally managed in `Directory.Packages.props` (transitive pinning on). Add `<PackageVersion>` there and a version-less `<PackageReference>` in the csproj.
- `Directory.Build.props` currently only sets `net11.0`. The spec calls for nullable + warnings-as-errors there (step 0.1); not yet done.

## Architecture

Projects in `src/`:

- **Fraud.AppHost** — Aspire orchestrator. Defines the Azure Service Bus resource (`RunAsEmulator()`) with queue `transactions-queue`, and wires Api, Scoring, Simulator via `AddProject`. Scoring and Simulator `WaitFor` + `WithReference` the bus. New infra (Postgres+pgvector, Redis, anomaly-scorer) is added here.
- **Fraud.ServiceDefaults** — shared OpenTelemetry, health checks, service discovery, resilience. Every service calls `builder.AddServiceDefaults()`.
- **Fraud.Contracts** — only types crossing process boundaries (messages, `QueueNames`). No logic, EF or Azure packages. Only add fields to records, never rename/remove (old messages may be queued).
- **Fraud.Simulator** — worker generating traffic. `Patterns/` holds one `IFraudPattern` per fraud type (CardTesting, AccountTakeover, ImpossibleTravel, NewMerchantBurst) returning sequences of `GeneratedTransaction`; `DataModels/` holds `CustomerProfile`, `Merchant`, `SimulatorOptions` (bound from the `SimulatorOptions` config section).
- **Fraud.Scoring** — worker consuming the queue via `ServiceBusProcessor` (`AutoCompleteMessages = false`, so handlers must call `CompleteMessageAsync`). Meant to hold the rules/`RiskScorer` core, free of Service Bus/EF/Redis types so it is unit-testable.
- **Fraud.Api** — ASP.NET Core minimal API. Still the template (`WeatherForecast`); planned: `/cases`, `/cases/{id}`, label, `/transactions` ingest, DLQ admin.

The Service Bus client is injected through the Aspire integration (`AddAzureServiceBusClient("ServiceBus")`); connection name `ServiceBus` must match the AppHost resource name. No connection strings in code.

The repo is early and mid-build: the code is a partial implementation of the Build spec, so some pieces (Fraud.Data, tests, infra, CI, the Python scorer) don't exist yet. Treat the spec as the target and check the code for what is actually there.

## Locked decisions (from the Build spec)

- Local orchestration is Aspire only (no docker-compose). Azure deploy is own Bicep in `infra/` via `azd`, from GitHub Actions with OIDC.
- Postgres+pgvector for data, Redis for per-customer `CustomerState`, App Configuration for thresholds/rates, Service Bus for messaging.
- Tests: xUnit unit tests; integration tests use Testcontainers (e.g. `Testcontainers.PostgreSql` with the `pgvector/pgvector` image) and need Docker running. Do not use the Aspire testing host for integration tests.
- Data ownership: only Scoring writes transactions/cases; only Api changes status/labels; only the Simulator writes ground truth.
- Conventions: `decimal` + ISO currency for money; `DateTimeOffset` UTC with injected `TimeProvider`; `Guid.CreateVersion7()` IDs; enums stored as strings; idempotent consumer (unique constraint on transaction id).
- No secrets in code or `appsettings.json` — user-secrets locally, Key Vault in Azure.
