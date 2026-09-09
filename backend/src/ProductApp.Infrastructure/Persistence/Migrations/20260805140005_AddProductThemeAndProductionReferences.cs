using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductThemeAndProductionReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Products",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#2563EB");

            migrationBuilder.InsertData(
                table: "Equipment",
                columns: new[] { "Id", "AvailabilityStatus", "Code", "CreatedAt", "CreatedBy", "HourlyCost", "Manufacturer", "Model", "Name", "ResourceCategoryId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), 1, "EQ-MIXER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 185m, null, null, "Melangeur industriel", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("60000000-0000-0000-0000-000000000002"), 1, "EQ-OVEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 520m, null, null, "Four industriel", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("60000000-0000-0000-0000-000000000003"), 1, "EQ-CONVEYOR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 130m, null, null, "Convoyeur de production", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("60000000-0000-0000-0000-000000000004"), 1, "EQ-PACK", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 275m, null, null, "Machine de conditionnement", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("60000000-0000-0000-0000-000000000005"), 1, "EQ-ASSEMBLY", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 310m, null, null, "Poste assemblage automatise", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("60000000-0000-0000-0000-000000000006"), 1, "EQ-TEST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 160m, null, null, "Banc de controle qualite", new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000002"), "ELECTRONICS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Produits electroniques", true, "Electronique", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "FOOD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Produits alimentaires", true, "Alimentaire", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "BEVERAGES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boissons et liquides", true, "Boissons", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "COSMETICS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Produits cosmetiques", true, "Cosmetiques", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "TEXTILES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Produits textiles", true, "Textile", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000007"), "OTHER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Autres produits industriels", true, "Autres", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Equipment",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000007"));

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Products");
        }
    }
}
