using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartProductAiAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiConversations_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiConversations_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiConversations_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AiConversations_ProductionExperiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "ProductionExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AiConversations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AiToolInvocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    SafeErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ArgumentsSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiToolInvocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiToolInvocations_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiToolInvocations_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAttachments_AiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiAttachments_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiAttachments_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: true),
                    OutputTokens = table.Column<int>(type: "int", nullable: true),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StructuredContentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiMessages_AiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiMessages_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiMessages_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiDocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PageNumber = table.Column<int>(type: "int", nullable: true),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiDocumentChunks_AiAttachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "AiAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiDocumentChunks_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiDocumentChunks_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiFeedback_AiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "AiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiFeedback_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiFeedback_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiMessageSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InternalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Excerpt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessageSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiMessageSources_AiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "AiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiMessageSources_ApplicationUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiMessageSources_ApplicationUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAttachments_ConversationId",
                table: "AiAttachments",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiAttachments_CreatedAt",
                table: "AiAttachments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiAttachments_CreatedBy",
                table: "AiAttachments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiAttachments_UpdatedBy",
                table: "AiAttachments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiAttachments_UserId_ConversationId_CreatedAt",
                table: "AiAttachments",
                columns: new[] { "UserId", "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_CreatedAt",
                table: "AiConversations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_CreatedBy",
                table: "AiConversations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_ExperimentId",
                table: "AiConversations",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_ProductId",
                table: "AiConversations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_UpdatedBy",
                table: "AiConversations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_UserId_IsArchived_LastMessageAt",
                table: "AiConversations",
                columns: new[] { "UserId", "IsArchived", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_AttachmentId_ChunkIndex",
                table: "AiDocumentChunks",
                columns: new[] { "AttachmentId", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_CreatedAt",
                table: "AiDocumentChunks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_CreatedBy",
                table: "AiDocumentChunks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_ExperimentId",
                table: "AiDocumentChunks",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_ProductId",
                table: "AiDocumentChunks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_UpdatedBy",
                table: "AiDocumentChunks",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_UserId_ConversationId",
                table: "AiDocumentChunks",
                columns: new[] { "UserId", "ConversationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedback_CreatedAt",
                table: "AiFeedback",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedback_CreatedBy",
                table: "AiFeedback",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedback_MessageId_UserId",
                table: "AiFeedback",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedback_UpdatedBy",
                table: "AiFeedback",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_ConversationId_CreatedAt",
                table: "AiMessages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_CreatedAt",
                table: "AiMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_CreatedBy",
                table: "AiMessages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_UpdatedBy",
                table: "AiMessages",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessageSources_CreatedAt",
                table: "AiMessageSources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessageSources_CreatedBy",
                table: "AiMessageSources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessageSources_MessageId_Kind",
                table: "AiMessageSources",
                columns: new[] { "MessageId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_AiMessageSources_UpdatedBy",
                table: "AiMessageSources",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiToolInvocations_ConversationId_MessageId",
                table: "AiToolInvocations",
                columns: new[] { "ConversationId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiToolInvocations_CreatedAt",
                table: "AiToolInvocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiToolInvocations_CreatedBy",
                table: "AiToolInvocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiToolInvocations_UpdatedBy",
                table: "AiToolInvocations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AiToolInvocations_UserId_CreatedAt",
                table: "AiToolInvocations",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiDocumentChunks");

            migrationBuilder.DropTable(
                name: "AiFeedback");

            migrationBuilder.DropTable(
                name: "AiMessageSources");

            migrationBuilder.DropTable(
                name: "AiToolInvocations");

            migrationBuilder.DropTable(
                name: "AiAttachments");

            migrationBuilder.DropTable(
                name: "AiMessages");

            migrationBuilder.DropTable(
                name: "AiConversations");
        }
    }
}
