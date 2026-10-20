using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class Part3StakeholderFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdministrationExpenses",
                table: "Estates",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AgentEmail",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentPhone",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentReference",
                table: "Estates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FirmActsAsAgent",
                table: "Estates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdministrationExpenses",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "AgentEmail",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "AgentPhone",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "AgentReference",
                table: "Estates");

            migrationBuilder.DropColumn(
                name: "FirmActsAsAgent",
                table: "Estates");
        }
    }
}
