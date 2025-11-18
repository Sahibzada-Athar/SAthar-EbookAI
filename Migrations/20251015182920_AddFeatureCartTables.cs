using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EBookDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureCartTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create Features table if it doesn't exist (using MySQL IF NOT EXISTS)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Features` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Key` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
                    `Price` decimal(10,2) NOT NULL,
                    `Type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Features` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");

            migrationBuilder.CreateTable(
                name: "TemporaryFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemporaryFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    AuthorPlanId = table.Column<int>(type: "int", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFeatures_AuthorPlans_AuthorPlanId",
                        column: x => x.AuthorPlanId,
                        principalTable: "AuthorPlans",
                        principalColumn: "AuthorPlanId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Insert Features data only if records don't exist (using INSERT IGNORE)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `Features` (`Id`, `CreatedAt`, `Description`, `IsActive`, `Key`, `Name`, `Price`, `Type`)
                VALUES 
                    (1, '2025-10-15 18:29:18.466470', 'Create and format professional eBooks with our easy-to-use editor.', 1, 'ebook_creation', 'EBook Creation', 0.00, 'Basic'),
                    (2, '2025-10-15 18:29:18.4663004', 'Design stunning book covers with our AI-powered tool.', 1, 'cover_design', 'Cover Design', 19.99, 'Premium'),
                    (3, '2025-10-15 18:29:18.4663011', 'Convert your book into a professional audiobook.', 1, 'audio_book', 'Audio Book', 49.99, 'Premium'),
                    (4, '2025-10-15 18:29:18.4663012', 'Professional proofreading and editing services.', 1, 'proofreading', 'Proofreading', 29.99, 'Premium'),
                    (5, '2025-10-15 18:29:18.4663013', 'Track your sales and reader engagement with detailed analytics.', 1, 'analytics', 'Analytics', 14.99, 'Premium'),
                    (6, '2025-10-15 18:29:18.4663017', 'Promote your book with our integrated marketing suite.', 1, 'marketing_tools', 'Marketing Tools', 24.99, 'Marketing');
            ");

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 1,
                column: "CreateddAt",
                value: new DateTime(2025, 10, 15, 18, 29, 18, 464, DateTimeKind.Utc).AddTicks(9528));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 2,
                column: "CreateddAt",
                value: new DateTime(2025, 10, 15, 18, 29, 18, 465, DateTimeKind.Utc).AddTicks(1011));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 3,
                column: "CreateddAt",
                value: new DateTime(2025, 10, 15, 18, 29, 18, 465, DateTimeKind.Utc).AddTicks(1107));

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "PlanId", "AllowAnalytics", "AllowDownloads", "AllowFullDashboard", "AllowPublishing", "CreateddAt", "IsActive", "MaxChapters", "MaxEBooks", "MaxPages", "PlanDays", "PlanDescription", "PlanHours", "PlanName", "PlanRate" },
                values: new object[] { 4, false, false, false, false, new DateTime(2025, 10, 15, 18, 29, 18, 465, DateTimeKind.Utc).AddTicks(1108), false, 0, 0, 0, 365, "Premium yearly subscription with all features", 0, "Premium Plan", 199.99m });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "AllowAnalytics", "AllowDelete", "AllowDownloads", "AllowEdit", "AllowFullDashboard", "AllowPublishing", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, true, true, true, true, true, true, "Administrator with full access", "Admin" },
                    { 2, true, true, true, true, true, true, "Author with publishing access", "Author" },
                    { 3, false, false, false, false, false, false, "Reader with limited access", "Reader" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryFeatures_FeatureId",
                table: "TemporaryFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeatures_AuthorPlanId",
                table: "UserFeatures",
                column: "AuthorPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeatures_FeatureId",
                table: "UserFeatures",
                column: "FeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemporaryFeatures");

            migrationBuilder.DropTable(
                name: "UserFeatures");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 1,
                column: "CreateddAt",
                value: new DateTime(2025, 9, 30, 20, 7, 30, 280, DateTimeKind.Utc).AddTicks(4863));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 2,
                column: "CreateddAt",
                value: new DateTime(2025, 9, 30, 20, 7, 30, 280, DateTimeKind.Utc).AddTicks(6351));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "PlanId",
                keyValue: 3,
                column: "CreateddAt",
                value: new DateTime(2025, 9, 30, 20, 7, 30, 281, DateTimeKind.Utc).AddTicks(413));
        }
    }
}
