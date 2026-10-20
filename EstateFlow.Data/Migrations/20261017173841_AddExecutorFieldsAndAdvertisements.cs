using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutorFieldsAndAdvertisements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LettersOfExecutorshipDate",
                table: "Executors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LettersOfExecutorshipRef",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MastersOffice",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstateAdvertisements",
                columns: table => new
                {
                    AdvertisementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertisementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GazetteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GazetteNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewspaperName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewspaperDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClaimDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObjectionsReceived = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstateAdvertisements", x => x.AdvertisementId);
                    table.ForeignKey(
                        name: "FK_EstateAdvertisements_Estates_EstateId",
                        column: x => x.EstateId,
                        principalTable: "Estates",
                        principalColumn: "EstateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstateAdvertisements_EstateId",
                table: "EstateAdvertisements",
                column: "EstateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstateAdvertisements");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "LettersOfExecutorshipDate",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "LettersOfExecutorshipRef",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "MastersOffice",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Executors");
        }
    }
}
