using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeceasedDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatrimonialRegime",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfDeath",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpouseFullName",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpouseIdNumber",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WillDate",
                table: "Deceased",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WillLocation",
                table: "Deceased",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "MatrimonialRegime",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "PlaceOfDeath",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "SpouseFullName",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "SpouseIdNumber",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "WillDate",
                table: "Deceased");

            migrationBuilder.DropColumn(
                name: "WillLocation",
                table: "Deceased");
        }
    }
}
