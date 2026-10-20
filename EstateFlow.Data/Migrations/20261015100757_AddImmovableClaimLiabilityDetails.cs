using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImmovableClaimLiabilityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountReference",
                table: "Liabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Liabilities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Liabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLodged",
                table: "Liabilities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePaid",
                table: "Liabilities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestAfterDeath",
                table: "Liabilities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredByCreditLife",
                table: "Liabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Liabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTakenOverByHeir",
                table: "Liabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecuredAgainst",
                table: "Liabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Liabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwardedToBeneficiaryIds",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErfNumber",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExclusiveUseAreas",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtentSquareMetres",
                table: "ImmovableProperties",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSoldByEstate",
                table: "ImmovableProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialBequest",
                table: "ImmovableProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MarketValue",
                table: "ImmovableProperties",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaserName",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleDate",
                table: "ImmovableProperties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "ImmovableProperties",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchemeName",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionNumber",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShareDenominator",
                table: "ImmovableProperties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShareNumerator",
                table: "ImmovableProperties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Township",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValuationDate",
                table: "ImmovableProperties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuerName",
                table: "ImmovableProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccruedInterest",
                table: "FinancialClaims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AwardedToBeneficiaryIds",
                table: "FinancialClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimType",
                table: "FinancialClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CollectedAmount",
                table: "FinancialClaims",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CollectedDate",
                table: "FinancialClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCollectedAsCash",
                table: "FinancialClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreditLifeInsurance",
                table: "FinancialClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInFavourOfEstate",
                table: "FinancialClaims",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialBequest",
                table: "FinancialClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinkedLiabilityNote",
                table: "FinancialClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValuationDate",
                table: "FinancialClaims",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountReference",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "DateLodged",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "DatePaid",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "InterestAfterDeath",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "IsCoveredByCreditLife",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "IsTakenOverByHeir",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "SecuredAgainst",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Liabilities");

            migrationBuilder.DropColumn(
                name: "AwardedToBeneficiaryIds",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ErfNumber",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ExclusiveUseAreas",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ExtentSquareMetres",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "IsSoldByEstate",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "IsSpecialBequest",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "MarketValue",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "PurchaserName",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "SaleDate",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "SchemeName",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "SectionNumber",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ShareDenominator",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ShareNumerator",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "Township",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ValuationDate",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "ValuerName",
                table: "ImmovableProperties");

            migrationBuilder.DropColumn(
                name: "AccruedInterest",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "AwardedToBeneficiaryIds",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "ClaimType",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "CollectedAmount",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "CollectedDate",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "IsCollectedAsCash",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "IsCreditLifeInsurance",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "IsInFavourOfEstate",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "IsSpecialBequest",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "LinkedLiabilityNote",
                table: "FinancialClaims");

            migrationBuilder.DropColumn(
                name: "ValuationDate",
                table: "FinancialClaims");
        }
    }
}
