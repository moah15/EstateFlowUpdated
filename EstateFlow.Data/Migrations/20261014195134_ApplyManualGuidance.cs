using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplyManualGuidance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharePercentage",
                table: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "ShareType",
                table: "Beneficiaries",
                newName: "RelationshipDescription");

            migrationBuilder.RenameColumn(
                name: "ShareFraction",
                table: "Beneficiaries",
                newName: "MaritalStatus");

            migrationBuilder.RenameColumn(
                name: "BequestDescription",
                table: "Beneficiaries",
                newName: "Gender");

            migrationBuilder.AddColumn<string>(
                name: "EstateBankAccountNumber",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstateBankBranchCode",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstateBankName",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasMultipleLdAccounts",
                table: "Estates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousSpouseRebateUsed",
                table: "Estates",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PreviousSpousesCount",
                table: "Estates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceOfEstate",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodicilDate",
                table: "Deceased",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Deceased",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LivedInSouthAfrica",
                table: "Deceased",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvancesPaid",
                table: "Beneficiaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BareDominiumFraction",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Beneficiaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsUltimateBeneficiaryPreviousEstate",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResidueDenominator",
                table: "Beneficiaries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidueNumerator",
                table: "Beneficiaries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstateBankAccountNumber",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "EstateBankBranchCode",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "EstateBankName",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "HasMultipleLdAccounts",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "PreviousSpouseRebateUsed",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "PreviousSpousesCount",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "SourceOfEstate",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "CodicilDate",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "LivedInSouthAfrica",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "AdvancesPaid",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "BareDominiumFraction",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsUltimateBeneficiaryPreviousEstate",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ResidueDenominator",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ResidueNumerator",
                table: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "RelationshipDescription",
                table: "Beneficiaries",
                newName: "ShareType");

            migrationBuilder.RenameColumn(
                name: "MaritalStatus",
                table: "Beneficiaries",
                newName: "ShareFraction");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Beneficiaries",
                newName: "BequestDescription");

            migrationBuilder.AddColumn<decimal>(
                name: "SharePercentage",
                table: "Beneficiaries",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }
    }
}
