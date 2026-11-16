using System.Collections.Generic;
using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fraud.Data.Migrations
{
    /// <inheritdoc />
    public partial class CaseReasonsAsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "reasons",
                table: "cases",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(List<CaseReason>),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<CaseReason>>(
                name: "reasons",
                table: "cases",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
