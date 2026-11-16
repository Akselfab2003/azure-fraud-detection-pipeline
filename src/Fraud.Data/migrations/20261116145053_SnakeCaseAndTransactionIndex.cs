using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fraud.Data.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseAndTransactionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseEvents_Cases_CaseId",
                table: "CaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Transactions_TransactionId",
                table: "Cases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CustomerId_MerchantId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cases",
                table: "Cases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroundTruth",
                table: "GroundTruth");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseEvents",
                table: "CaseEvents");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "transactions");

            migrationBuilder.RenameTable(
                name: "Cases",
                newName: "cases");

            migrationBuilder.RenameTable(
                name: "GroundTruth",
                newName: "ground_truth");

            migrationBuilder.RenameTable(
                name: "CaseEvents",
                newName: "case_events");

            migrationBuilder.RenameColumn(
                name: "Decision",
                table: "transactions",
                newName: "decision");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "transactions",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "transactions",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "Channel",
                table: "transactions",
                newName: "channel");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "transactions",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "transactions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ScoredAt",
                table: "transactions",
                newName: "scored_at");

            migrationBuilder.RenameColumn(
                name: "RiskScore",
                table: "transactions",
                newName: "risk_score");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "transactions",
                newName: "occurred_at");

            migrationBuilder.RenameColumn(
                name: "MerchantId",
                table: "transactions",
                newName: "merchant_id");

            migrationBuilder.RenameColumn(
                name: "MerchantCategory",
                table: "transactions",
                newName: "merchant_category");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "transactions",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "cases",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reasons",
                table: "cases",
                newName: "reasons");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "cases",
                newName: "label");

            migrationBuilder.RenameColumn(
                name: "Explanation",
                table: "cases",
                newName: "explanation");

            migrationBuilder.RenameColumn(
                name: "Embedding",
                table: "cases",
                newName: "embedding");

            migrationBuilder.RenameColumn(
                name: "Decision",
                table: "cases",
                newName: "decision");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cases",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "cases",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "cases",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "RuleSetVersion",
                table: "cases",
                newName: "rule_set_version");

            migrationBuilder.RenameColumn(
                name: "RiskScore",
                table: "cases",
                newName: "risk_score");

            migrationBuilder.RenameColumn(
                name: "PromptVersion",
                table: "cases",
                newName: "prompt_version");

            migrationBuilder.RenameColumn(
                name: "ModelVersion",
                table: "cases",
                newName: "model_version");

            migrationBuilder.RenameColumn(
                name: "LabelledBy",
                table: "cases",
                newName: "labelled_by");

            migrationBuilder.RenameColumn(
                name: "LabelledAt",
                table: "cases",
                newName: "labelled_at");

            migrationBuilder.RenameColumn(
                name: "LabelNote",
                table: "cases",
                newName: "label_note");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "cases",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AnomalyScore",
                table: "cases",
                newName: "anomaly_score");

            migrationBuilder.RenameIndex(
                name: "IX_Cases_Status",
                table: "cases",
                newName: "IX_cases_status");

            migrationBuilder.RenameIndex(
                name: "IX_Cases_TransactionId",
                table: "cases",
                newName: "IX_cases_transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_Cases_Status_CreatedAt",
                table: "cases",
                newName: "IX_cases_status_created_at");

            migrationBuilder.RenameColumn(
                name: "Pattern",
                table: "ground_truth",
                newName: "pattern");

            migrationBuilder.RenameColumn(
                name: "IsFraud",
                table: "ground_truth",
                newName: "is_fraud");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "ground_truth",
                newName: "transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_GroundTruth_Pattern",
                table: "ground_truth",
                newName: "IX_ground_truth_pattern");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "case_events",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "At",
                table: "case_events",
                newName: "at");

            migrationBuilder.RenameColumn(
                name: "Actor",
                table: "case_events",
                newName: "actor");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "case_events",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DataJson",
                table: "case_events",
                newName: "data_json");

            migrationBuilder.RenameColumn(
                name: "CaseId",
                table: "case_events",
                newName: "case_id");

            migrationBuilder.RenameIndex(
                name: "IX_CaseEvents_CaseId_At",
                table: "case_events",
                newName: "IX_case_events_case_id_at");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactions",
                table: "transactions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cases",
                table: "cases",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ground_truth",
                table: "ground_truth",
                column: "transaction_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_case_events",
                table: "case_events",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_customer_id_occurred_at",
                table: "transactions",
                columns: new[] { "customer_id", "occurred_at" });

            migrationBuilder.AddForeignKey(
                name: "FK_case_events_cases_case_id",
                table: "case_events",
                column: "case_id",
                principalTable: "cases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cases_transactions_transaction_id",
                table: "cases",
                column: "transaction_id",
                principalTable: "transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_events_cases_case_id",
                table: "case_events");

            migrationBuilder.DropForeignKey(
                name: "FK_cases_transactions_transaction_id",
                table: "cases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactions",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_customer_id_occurred_at",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cases",
                table: "cases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ground_truth",
                table: "ground_truth");

            migrationBuilder.DropPrimaryKey(
                name: "PK_case_events",
                table: "case_events");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "cases",
                newName: "Cases");

            migrationBuilder.RenameTable(
                name: "ground_truth",
                newName: "GroundTruth");

            migrationBuilder.RenameTable(
                name: "case_events",
                newName: "CaseEvents");

            migrationBuilder.RenameColumn(
                name: "decision",
                table: "Transactions",
                newName: "Decision");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "Transactions",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Transactions",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "channel",
                table: "Transactions",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Transactions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "scored_at",
                table: "Transactions",
                newName: "ScoredAt");

            migrationBuilder.RenameColumn(
                name: "risk_score",
                table: "Transactions",
                newName: "RiskScore");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "Transactions",
                newName: "OccurredAt");

            migrationBuilder.RenameColumn(
                name: "merchant_id",
                table: "Transactions",
                newName: "MerchantId");

            migrationBuilder.RenameColumn(
                name: "merchant_category",
                table: "Transactions",
                newName: "MerchantCategory");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Transactions",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Cases",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reasons",
                table: "Cases",
                newName: "Reasons");

            migrationBuilder.RenameColumn(
                name: "label",
                table: "Cases",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "explanation",
                table: "Cases",
                newName: "Explanation");

            migrationBuilder.RenameColumn(
                name: "embedding",
                table: "Cases",
                newName: "Embedding");

            migrationBuilder.RenameColumn(
                name: "decision",
                table: "Cases",
                newName: "Decision");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Cases",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Cases",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "Cases",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "rule_set_version",
                table: "Cases",
                newName: "RuleSetVersion");

            migrationBuilder.RenameColumn(
                name: "risk_score",
                table: "Cases",
                newName: "RiskScore");

            migrationBuilder.RenameColumn(
                name: "prompt_version",
                table: "Cases",
                newName: "PromptVersion");

            migrationBuilder.RenameColumn(
                name: "model_version",
                table: "Cases",
                newName: "ModelVersion");

            migrationBuilder.RenameColumn(
                name: "labelled_by",
                table: "Cases",
                newName: "LabelledBy");

            migrationBuilder.RenameColumn(
                name: "labelled_at",
                table: "Cases",
                newName: "LabelledAt");

            migrationBuilder.RenameColumn(
                name: "label_note",
                table: "Cases",
                newName: "LabelNote");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Cases",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "anomaly_score",
                table: "Cases",
                newName: "AnomalyScore");

            migrationBuilder.RenameIndex(
                name: "IX_cases_status",
                table: "Cases",
                newName: "IX_Cases_Status");

            migrationBuilder.RenameIndex(
                name: "IX_cases_transaction_id",
                table: "Cases",
                newName: "IX_Cases_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_cases_status_created_at",
                table: "Cases",
                newName: "IX_Cases_Status_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "pattern",
                table: "GroundTruth",
                newName: "Pattern");

            migrationBuilder.RenameColumn(
                name: "is_fraud",
                table: "GroundTruth",
                newName: "IsFraud");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "GroundTruth",
                newName: "TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_ground_truth_pattern",
                table: "GroundTruth",
                newName: "IX_GroundTruth_Pattern");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "CaseEvents",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "at",
                table: "CaseEvents",
                newName: "At");

            migrationBuilder.RenameColumn(
                name: "actor",
                table: "CaseEvents",
                newName: "Actor");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CaseEvents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "data_json",
                table: "CaseEvents",
                newName: "DataJson");

            migrationBuilder.RenameColumn(
                name: "case_id",
                table: "CaseEvents",
                newName: "CaseId");

            migrationBuilder.RenameIndex(
                name: "IX_case_events_case_id_at",
                table: "CaseEvents",
                newName: "IX_CaseEvents_CaseId_At");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cases",
                table: "Cases",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroundTruth",
                table: "GroundTruth",
                column: "TransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseEvents",
                table: "CaseEvents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId_MerchantId",
                table: "Transactions",
                columns: new[] { "CustomerId", "MerchantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CaseEvents_Cases_CaseId",
                table: "CaseEvents",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Transactions_TransactionId",
                table: "Cases",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
