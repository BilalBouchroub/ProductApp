using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                    table.CheckConstraint("CK_ApplicationUsers_Email", "\"NormalizedEmail\" <> ''");
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permissions_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductCategories_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RevokedByIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceCategories_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourceCategories_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suppliers_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SapCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    TargetSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ProductionUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_BatchQuantity", "\"BatchQuantity\" IS NULL OR \"BatchQuantity\" > 0");
                    table.CheckConstraint("CK_Products_TargetSalePrice", "\"TargetSalePrice\" IS NULL OR \"TargetSalePrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_Products_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HourlyCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityStatus = table.Column<int>(type: "int", nullable: false),
                    ResourceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.CheckConstraint("CK_Equipment_HourlyCost", "\"HourlyCost\" >= 0");
                    table.ForeignKey(
                        name: "FK_Equipment_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Equipment_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Equipment_ResourceCategories_ResourceCategoryId",
                        column: x => x.ResourceCategoryId,
                        principalTable: "ResourceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SapCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvailableStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ResourceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterials", x => x.Id);
                    table.CheckConstraint("CK_RawMaterials_Stock", "\"AvailableStock\" >= 0 AND \"MinimumStock\" >= 0");
                    table.CheckConstraint("CK_RawMaterials_UnitCost", "\"UnitCost\" >= 0");
                    table.ForeignKey(
                        name: "FK_RawMaterials_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RawMaterials_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RawMaterials_ResourceCategories_ResourceCategoryId",
                        column: x => x.ResourceCategoryId,
                        principalTable: "ResourceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterials_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MarketAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EquipmentCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalProductionCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TargetSellingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossMargin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MarginRate = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    TotalCycleTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    MaterialAvailabilityRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    FeasibilityScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAnalyses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TargetSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ProductionUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVersions", x => x.Id);
                    table.UniqueConstraint("AK_ProductVersions_ProductId_VersionNumber", x => new { x.ProductId, x.VersionNumber });
                    table.CheckConstraint("CK_ProductVersions_Prices", "(\"TargetSalePrice\" IS NULL OR \"TargetSalePrice\" >= 0) AND (\"BatchQuantity\" IS NULL OR \"BatchQuantity\" > 0)");
                    table.CheckConstraint("CK_ProductVersions_Version", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductVersions_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductVersions_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductVersions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CapacityPerHour = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CapacityUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EnergyConsumptionPerHour = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LastMaintenanceAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.CheckConstraint("CK_Machines_Capacity", "\"CapacityPerHour\" > 0");
                    table.CheckConstraint("CK_Machines_Energy", "\"EnergyConsumptionPerHour\" >= 0");
                    table.ForeignKey(
                        name: "FK_Machines_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Machines_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Machines_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketStudies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetMarket = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    GeographicArea = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CustomerSegment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StudyDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EstimatedMarketSize = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualGrowthRate = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    ProductionCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProposedSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageMarketPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculatedMargin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MarginRate = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    MonthlySalesVolume = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductionScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MarketScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinancialScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    GlobalScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketStudies", x => x.Id);
                    table.CheckConstraint("CK_MarketStudies_Financials", "\"ProductionCost\" >= 0 AND \"ProposedSalePrice\" >= 0 AND \"AverageMarketPrice\" >= 0");
                    table.CheckConstraint("CK_MarketStudies_Scores", "\"ProductionScore\" BETWEEN 0 AND 100 AND \"MarketScore\" BETWEEN 0 AND 100 AND \"FinancialScore\" BETWEEN 0 AND 100 AND \"RiskScore\" BETWEEN 0 AND 100 AND \"GlobalScore\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_MarketStudies_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketStudies_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketStudies_ProductVersions_ProductVersionId",
                        column: x => x.ProductVersionId,
                        principalTable: "ProductVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionExperiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Hypothesis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    PlannedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PlannedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    WasteRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Conclusion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionExperiments", x => x.Id);
                    table.CheckConstraint("CK_ProductionExperiments_Costs", "\"PlannedCost\" >= 0 AND (\"ActualCost\" IS NULL OR \"ActualCost\" >= 0)");
                    table.CheckConstraint("CK_ProductionExperiments_Quantities", "\"PlannedQuantity\" > 0 AND (\"ActualQuantity\" IS NULL OR \"ActualQuantity\" >= 0)");
                    table.CheckConstraint("CK_ProductionExperiments_Waste", "\"WasteRate\" IS NULL OR (\"WasteRate\" >= 0 AND \"WasteRate\" <= 100)");
                    table.ForeignKey(
                        name: "FK_ProductionExperiments_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionExperiments_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionExperiments_ProductVersions_ProductVersionId",
                        column: x => x.ProductVersionId,
                        principalTable: "ProductVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVersionNumber = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Temperature = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EquipmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlannedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Pressure = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    Humidity = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PlannedOutputQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ActualOutputQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WasteQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    OperatorCount = table.Column<int>(type: "int", nullable: false),
                    EnergyConsumption = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ValidationCriteria = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionSteps_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionSteps_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionSteps_ProductVersions_ProductId_ProductVersionNumber",
                        columns: x => new { x.ProductId, x.ProductVersionNumber },
                        principalTable: "ProductVersions",
                        principalColumns: new[] { "ProductId", "VersionNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Competitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketStudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    EstimatedQualityScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MarketShare = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SalesChannels = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CustomerRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitors", x => x.Id);
                    table.CheckConstraint("CK_Competitors_PriceQuantity", "\"Price\" >= 0 AND \"Quantity\" > 0");
                    table.CheckConstraint("CK_Competitors_Scores", "(\"EstimatedQualityScore\" IS NULL OR \"EstimatedQualityScore\" BETWEEN 0 AND 100) AND (\"MarketShare\" IS NULL OR \"MarketShare\" BETWEEN 0 AND 100) AND (\"CustomerRating\" IS NULL OR \"CustomerRating\" BETWEEN 0 AND 5)");
                    table.ForeignKey(
                        name: "FK_Competitors_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Competitors_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Competitors_MarketStudies_MarketStudyId",
                        column: x => x.MarketStudyId,
                        principalTable: "MarketStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketStudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RequestedChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationRequests_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OptimizationRequests_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OptimizationRequests_MarketStudies_MarketStudyId",
                        column: x => x.MarketStudyId,
                        principalTable: "MarketStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketStudyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Probability = table.Column<int>(type: "int", nullable: false),
                    Impact = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MitigationAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Id);
                    table.CheckConstraint("CK_Risks_Impact", "\"Impact\" BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Risks_Probability", "\"Probability\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Risks_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Risks_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Risks_MarketStudies_MarketStudyId",
                        column: x => x.MarketStudyId,
                        principalTable: "MarketStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PlannedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PlannedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    PlannedOutputQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ActualOutputQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentSteps", x => x.Id);
                    table.CheckConstraint("CK_ExperimentSteps_Costs", "\"PlannedCost\" >= 0 AND (\"ActualCost\" IS NULL OR \"ActualCost\" >= 0)");
                    table.CheckConstraint("CK_ExperimentSteps_Order", "\"Order\" > 0");
                    table.ForeignKey(
                        name: "FK_ExperimentSteps_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExperimentSteps_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExperimentSteps_ProductionExperiments_ProductionExperimentId",
                        column: x => x.ProductionExperimentId,
                        principalTable: "ProductionExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExperimentSteps_ProductionSteps_ProductionStepId",
                        column: x => x.ProductionStepId,
                        principalTable: "ProductionSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StepResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "date", nullable: true),
                    AvailabilityStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepResources", x => x.Id);
                    table.CheckConstraint("CK_StepResources_Costs", "\"UnitCost\" >= 0 AND \"TotalCost\" >= 0");
                    table.CheckConstraint("CK_StepResources_Quantities", "\"PlannedQuantity\" >= 0 AND (\"ActualQuantity\" IS NULL OR \"ActualQuantity\" >= 0)");
                    table.ForeignKey(
                        name: "FK_StepResources_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StepResources_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StepResources_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepResources_ProductionSteps_ProductionStepId",
                        column: x => x.ProductionStepId,
                        principalTable: "ProductionSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StepResources_RawMaterials_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "Module", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "users.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gérer les utilisateurs", "Users", "Gérer les utilisateurs", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "audit.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Consulter les audits", "Audit", "Consulter les audits", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "products.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gérer les produits", "Products", "Gérer les produits", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "experiments.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gérer les expériences", "Experiments", "Gérer les expériences", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "studies.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gérer les études de marché", "MarketStudies", "Gérer les études de marché", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("30000000-0000-0000-0000-000000000001"), "BISCUITS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Produits biscuitiers", true, "Biscuits", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "ResourceCategories",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "ResourceType", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "RAW_MATERIAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Matières intégrées au produit", true, "Matières premières", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "EQUIPMENT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Machines et équipements industriels", true, "Équipements", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "Description", "Name", "NormalizedName", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "role-admin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Administration complète de la plateforme", "Administrator", "ADMINISTRATOR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "role-production", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gestion des produits et de la production", "ProductionManager", "PRODUCTIONMANAGER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "role-commercial", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Études commerciales et décisions de marché", "CommercialManager", "COMMERCIALMANAGER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "ApplicationUsers",
                columns: new[] { "Id", "AccessFailedCount", "AvatarUrl", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "Email", "EmailConfirmed", "FirstName", "LastLoginAt", "LastName", "LockoutEnabled", "LockoutEnd", "MustChangePassword", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RoleId", "SecurityStamp", "Status", "TwoFactorEnabled", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), 0, null, "admin-concurrency", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@productapp.local", true, "Admin", null, "ProductApp", true, null, true, "ADMIN@PRODUCTAPP.LOCAL", "ADMIN@PRODUCTAPP.LOCAL", "AQAAAAIAAYagAAAAEJGBIicEG6k57BLipKy6/y2Eg7ApXjqDTF4hhxmZCCQVu2zxKbb+NVK0DthqUvGOWg==", null, false, new Guid("10000000-0000-0000-0000-000000000001"), "admin-security", 1, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@productapp.local" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), 0, null, "production-concurrency", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "production@productapp.local", true, "Responsable", null, "Production", true, null, true, "PRODUCTION@PRODUCTAPP.LOCAL", "PRODUCTION@PRODUCTAPP.LOCAL", "AQAAAAIAAYagAAAAELO6DeEckMvLmIJh6FpsWsMdyaEirSCzloiWaM7Ia3gTNAcSFYYqAk5Wv1cu9O/4mA==", null, false, new Guid("10000000-0000-0000-0000-000000000002"), "production-security", 1, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "production@productapp.local" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), 0, null, "commercial-concurrency", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "commercial@productapp.local", true, "Responsable", null, "Commercial", true, null, true, "COMMERCIAL@PRODUCTAPP.LOCAL", "COMMERCIAL@PRODUCTAPP.LOCAL", "AQAAAAIAAYagAAAAEAyALAU+9GPoV+klxaFQm8O/tLB8qmQkgPH4TqdBRpgRkgsCzqN6wlmcppz/skZo4w==", null, false, new Guid("10000000-0000-0000-0000-000000000003"), "commercial-security", 1, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "commercial@productapp.local" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_CreatedAt",
                table: "ApplicationUsers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_CreatedBy",
                table: "ApplicationUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_NormalizedEmail",
                table: "ApplicationUsers",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_NormalizedUserName",
                table: "ApplicationUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_RoleId_Status",
                table: "ApplicationUsers",
                columns: new[] { "RoleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UpdatedBy",
                table: "ApplicationUsers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedBy",
                table: "AuditLogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module_Level",
                table: "AuditLogs",
                columns: new[] { "Module", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UpdatedBy",
                table: "AuditLogs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_CreatedAt",
                table: "Competitors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_CreatedBy",
                table: "Competitors",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_MarketStudyId_Name",
                table: "Competitors",
                columns: new[] { "MarketStudyId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Competitors_UpdatedBy",
                table: "Competitors",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Code",
                table: "Equipment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CreatedAt",
                table: "Equipment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CreatedBy",
                table: "Equipment",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_ResourceCategoryId_AvailabilityStatus",
                table: "Equipment",
                columns: new[] { "ResourceCategoryId", "AvailabilityStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_UpdatedBy",
                table: "Equipment",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_CreatedAt",
                table: "ExperimentSteps",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_CreatedBy",
                table: "ExperimentSteps",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_ProductionExperimentId_Order",
                table: "ExperimentSteps",
                columns: new[] { "ProductionExperimentId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_ProductionExperimentId_ProductionStepId",
                table: "ExperimentSteps",
                columns: new[] { "ProductionExperimentId", "ProductionStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_ProductionStepId",
                table: "ExperimentSteps",
                column: "ProductionStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSteps_UpdatedBy",
                table: "ExperimentSteps",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CreatedAt",
                table: "Machines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CreatedBy",
                table: "Machines",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_EquipmentId",
                table: "Machines",
                column: "EquipmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_SerialNumber",
                table: "Machines",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_UpdatedBy",
                table: "Machines",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAnalyses_ProductId_CreatedAtUtc",
                table: "MarketAnalyses",
                columns: new[] { "ProductId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketStudies_CreatedAt",
                table: "MarketStudies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketStudies_CreatedBy",
                table: "MarketStudies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MarketStudies_ProductVersionId_StudyDate",
                table: "MarketStudies",
                columns: new[] { "ProductVersionId", "StudyDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketStudies_Status_Recommendation",
                table: "MarketStudies",
                columns: new[] { "Status", "Recommendation" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketStudies_UpdatedBy",
                table: "MarketStudies",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UpdatedBy",
                table: "Notifications",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationRequests_CreatedAt",
                table: "OptimizationRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationRequests_CreatedBy",
                table: "OptimizationRequests",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationRequests_MarketStudyId_Status",
                table: "OptimizationRequests",
                columns: new[] { "MarketStudyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationRequests_Priority_CreatedAt",
                table: "OptimizationRequests",
                columns: new[] { "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationRequests_UpdatedBy",
                table: "OptimizationRequests",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CreatedAt",
                table: "Permissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CreatedBy",
                table: "Permissions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module",
                table: "Permissions",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UpdatedBy",
                table: "Permissions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Code",
                table: "ProductCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CreatedAt",
                table: "ProductCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CreatedBy",
                table: "ProductCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_UpdatedBy",
                table: "ProductCategories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionExperiments_CreatedAt",
                table: "ProductionExperiments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionExperiments_CreatedBy",
                table: "ProductionExperiments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionExperiments_ProductVersionId_StartDate",
                table: "ProductionExperiments",
                columns: new[] { "ProductVersionId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionExperiments_Result",
                table: "ProductionExperiments",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionExperiments_UpdatedBy",
                table: "ProductionExperiments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSteps_CreatedAt",
                table: "ProductionSteps",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSteps_CreatedBy",
                table: "ProductionSteps",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSteps_ProductId_ProductVersionNumber_Order",
                table: "ProductionSteps",
                columns: new[] { "ProductId", "ProductVersionNumber", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSteps_UpdatedBy",
                table: "ProductionSteps",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedBy",
                table: "Products",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SapCode",
                table: "Products",
                column: "SapCode",
                unique: true,
                filter: "[SapCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UpdatedBy",
                table: "Products",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_CreatedAt",
                table: "ProductVersions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_CreatedBy",
                table: "ProductVersions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_ProductId_Status",
                table: "ProductVersions",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_UpdatedBy",
                table: "ProductVersions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_CreatedAt",
                table: "RawMaterials",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_CreatedBy",
                table: "RawMaterials",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_ResourceCategoryId_IsActive",
                table: "RawMaterials",
                columns: new[] { "ResourceCategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_SapCode",
                table: "RawMaterials",
                column: "SapCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_SupplierId",
                table: "RawMaterials",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterials_UpdatedBy",
                table: "RawMaterials",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreatedAt",
                table: "RefreshTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreatedBy",
                table: "RefreshTokens",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UpdatedBy",
                table: "RefreshTokens",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_Code",
                table: "ResourceCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_CreatedAt",
                table: "ResourceCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_CreatedBy",
                table: "ResourceCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_ResourceType",
                table: "ResourceCategories",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_UpdatedBy",
                table: "ResourceCategories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_CreatedAt",
                table: "Risks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_CreatedBy",
                table: "Risks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_MarketStudyId_Probability_Impact",
                table: "Risks",
                columns: new[] { "MarketStudyId", "Probability", "Impact" });

            migrationBuilder.CreateIndex(
                name: "IX_Risks_UpdatedBy",
                table: "Risks",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedAt",
                table: "Roles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedBy",
                table: "Roles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NormalizedName",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UpdatedBy",
                table: "Roles",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_CreatedAt",
                table: "StepResources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_CreatedBy",
                table: "StepResources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_EquipmentId",
                table: "StepResources",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_ProductionStepId_ResourceType",
                table: "StepResources",
                columns: new[] { "ProductionStepId", "ResourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_RawMaterialId",
                table: "StepResources",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_StepResources_UpdatedBy",
                table: "StepResources",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedAt",
                table: "Suppliers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedBy",
                table: "Suppliers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_UpdatedBy",
                table: "Suppliers",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Roles_RoleId",
                table: "ApplicationUsers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Roles_RoleId",
                table: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Competitors");

            migrationBuilder.DropTable(
                name: "ExperimentSteps");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "MarketAnalyses");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OptimizationRequests");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StepResources");

            migrationBuilder.DropTable(
                name: "ProductionExperiments");

            migrationBuilder.DropTable(
                name: "MarketStudies");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "ProductionSteps");

            migrationBuilder.DropTable(
                name: "RawMaterials");

            migrationBuilder.DropTable(
                name: "ProductVersions");

            migrationBuilder.DropTable(
                name: "ResourceCategories");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
