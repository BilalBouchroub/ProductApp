using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionStepIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "ProductionSteps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Automatic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "ProductionSteps");
        }
    }
}
