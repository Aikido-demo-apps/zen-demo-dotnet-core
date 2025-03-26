using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zen_demo_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToPets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Pets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Pets");
        }
    }
}
