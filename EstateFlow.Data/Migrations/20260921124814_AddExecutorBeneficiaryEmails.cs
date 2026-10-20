using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutorBeneficiaryEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Beneficiaries");
        }
    }
}
