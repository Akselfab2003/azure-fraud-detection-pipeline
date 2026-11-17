using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fraud.Data.migrations
{
    /// <inheritdoc />
    public partial class GroundTruthCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "ground_truth",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "IX_ground_truth_created_at",
                table: "ground_truth",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ground_truth_created_at",
                table: "ground_truth");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "ground_truth");
        }
    }
}
