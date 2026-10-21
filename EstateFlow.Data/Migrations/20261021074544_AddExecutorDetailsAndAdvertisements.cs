using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutorDetailsAndAdvertisements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstateAdvertisements_Estates_EstateId",
                table: "EstateAdvertisements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstateAdvertisements",
                table: "EstateAdvertisements");

            migrationBuilder.DropIndex(
                name: "IX_EstateAdvertisements_EstateId",
                table: "EstateAdvertisements");

            migrationBuilder.RenameTable(
                name: "EstateAdvertisements",
                newName: "EstateAdvertisement");

            migrationBuilder.AlterColumn<string>(
                name: "AdvertisementType",
                table: "EstateAdvertisement",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstateAdvertisement",
                table: "EstateAdvertisement",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_EstateAdvertisement_EstateId_AdvertisementType",
                table: "EstateAdvertisement",
                columns: new[] { "EstateId", "AdvertisementType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EstateAdvertisement_Estates_EstateId",
                table: "EstateAdvertisement",
                column: "EstateId",
                principalTable: "Estates",
                principalColumn: "EstateId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstateAdvertisement_Estates_EstateId",
                table: "EstateAdvertisement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstateAdvertisement",
                table: "EstateAdvertisement");

            migrationBuilder.DropIndex(
                name: "IX_EstateAdvertisement_EstateId_AdvertisementType",
                table: "EstateAdvertisement");

            migrationBuilder.RenameTable(
                name: "EstateAdvertisement",
                newName: "EstateAdvertisements");

            migrationBuilder.AlterColumn<string>(
                name: "AdvertisementType",
                table: "EstateAdvertisements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstateAdvertisements",
                table: "EstateAdvertisements",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_EstateAdvertisements_EstateId",
                table: "EstateAdvertisements",
                column: "EstateId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstateAdvertisements_Estates_EstateId",
                table: "EstateAdvertisements",
                column: "EstateId",
                principalTable: "Estates",
                principalColumn: "EstateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
