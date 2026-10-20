using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMovableAssetDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetCategory",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwardedToBeneficiaryIds",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CumulativeDividend",
                table: "MovableAssets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSoldOrReducedToCash",
                table: "MovableAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialBequest",
                table: "MovableAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfShares",
                table: "MovableAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceCentsPerShare",
                table: "MovableAssets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaleDate",
                table: "MovableAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaleProceeds",
                table: "MovableAssets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareClass",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareCompany",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValuationDate",
                table: "MovableAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuerName",
                table: "MovableAssets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetCategory",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "AwardedToBeneficiaryIds",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "CumulativeDividend",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "IsSoldOrReducedToCash",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "IsSpecialBequest",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "NumberOfShares",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "PriceCentsPerShare",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "SaleDate",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "SaleProceeds",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "ShareClass",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "ShareCompany",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "ValuationDate",
                table: "MovableAssets");

            migrationBuilder.DropColumn(
                name: "ValuerName",
                table: "MovableAssets");
        }
    }
}
