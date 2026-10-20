using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLdGeneralSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfAccount",
                table: "Estates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstateDutyRate",
                table: "Estates",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstateDutyRebate",
                table: "Estates",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExecutorFeePercentage",
                table: "Estates",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTestate",
                table: "Estates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVatRegistered",
                table: "Estates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LettersOfExecutorshipDate",
                table: "Estates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MastersFee",
                table: "Estates",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MastersOffice",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfAccount",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "Estates",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "DateOfAccount",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "EstateDutyRate",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "EstateDutyRebate",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "ExecutorFeePercentage",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "IsTestate",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "IsVatRegistered",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "LettersOfExecutorshipDate",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "MastersFee",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "MastersOffice",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "PlaceOfAccount",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Estates");
        }
    }
}
