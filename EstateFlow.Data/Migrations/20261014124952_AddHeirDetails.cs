using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeirDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BequestDescription",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashLegacyAmount",
                table: "Beneficiaries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianOrTrustee",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMinor",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPredeceased",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurvivingSpouse",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareFraction",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SharePercentage",
                table: "Beneficiaries",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareType",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WillClause",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BequestDescription",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "CashLegacyAmount",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "GuardianOrTrustee",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsMinor",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsPredeceased",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsSurvivingSpouse",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ShareFraction",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SharePercentage",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ShareType",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "WillClause",
                table: "Beneficiaries");
        }
    }
}
