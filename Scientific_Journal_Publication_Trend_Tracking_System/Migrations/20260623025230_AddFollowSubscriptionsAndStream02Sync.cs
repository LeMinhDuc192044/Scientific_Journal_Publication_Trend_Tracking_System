using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Scientific_Journal_Publication_Trend_Tracking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowSubscriptionsAndStream02Sync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResearchTopics_Name",
                table: "ResearchTopics");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "NotificationsCreated",
                table: "SyncLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordsCreated",
                table: "SyncLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordsUpdated",
                table: "SyncLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BatchSize",
                table: "SyncJobs",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "SearchQuery",
                table: "SyncJobs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedResearchTopicId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FollowSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResearchTopicId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowSubscriptions", x => x.Id);
                    table.CheckConstraint("CK_FollowSubscriptions_TargetMatchesType", "(\"Type\" = 0 AND \"JournalId\" IS NOT NULL AND \"ResearchTopicId\" IS NULL) OR (\"Type\" = 1 AND \"JournalId\" IS NULL AND \"ResearchTopicId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FollowSubscriptions_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowSubscriptions_ResearchTopics_ResearchTopicId",
                        column: x => x.ResearchTopicId,
                        principalTable: "ResearchTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ApiDataSources",
                columns: new[] { "Id", "ApiKey", "BaseUrl", "CreatedAt", "IsActive", "LastSyncTime", "Name", "RequestsPerMinute", "SourceType", "UpdatedAt" },
                values: new object[] { new Guid("654c4415-64de-48f7-a0ea-094c00491f27"), null, "https://api.openalex.org", new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "OpenAlex", 100, 1, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "ResearchTopics",
                columns: new[] { "Id", "CreatedAt", "Description", "Domain", "Name", "PapersCount", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0bdaedb8-f741-4cf6-87c6-b82951db7d31"), new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Research related to computing, software, algorithms, and information systems.", 0, "Computer Science", 0, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8698ace8-d52d-48d6-b750-319684d0ea30"), new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Research related to artificial intelligence and intelligent systems.", 1, "Artificial Intelligence", 0, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "SyncJobs",
                columns: new[] { "Id", "ApiDataSourceId", "BatchSize", "CreatedAt", "CronExpression", "IsActive", "JobName", "LastRunTime", "NextScheduledRun", "RetryAttempts", "SearchQuery", "Status", "UpdatedAt" },
                values: new object[] { new Guid("8f95e2d4-86c3-41e2-ae16-7dba70d82747"), new Guid("654c4415-64de-48f7-a0ea-094c00491f27"), 100, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc), "0 0 * * 0", true, "Weekly Artificial Intelligence Paper Sync", null, null, 3, "Artificial Intelligence", 0, new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTopics_Name",
                table: "ResearchTopics",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedJournalId",
                table: "Notifications",
                column: "RelatedJournalId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedPaperId",
                table: "Notifications",
                column: "RelatedPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedResearchTopicId",
                table: "Notifications",
                column: "RelatedResearchTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_RelatedPaperId",
                table: "Notifications",
                columns: new[] { "UserId", "RelatedPaperId" },
                unique: true,
                filter: "\"RelatedPaperId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FollowSubscriptions_JournalId",
                table: "FollowSubscriptions",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowSubscriptions_ResearchTopicId",
                table: "FollowSubscriptions",
                column: "ResearchTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowSubscriptions_UserId_IsActive",
                table: "FollowSubscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowSubscriptions_UserId_JournalId",
                table: "FollowSubscriptions",
                columns: new[] { "UserId", "JournalId" },
                unique: true,
                filter: "\"JournalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FollowSubscriptions_UserId_ResearchTopicId",
                table: "FollowSubscriptions",
                columns: new[] { "UserId", "ResearchTopicId" },
                unique: true,
                filter: "\"ResearchTopicId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Journals_RelatedJournalId",
                table: "Notifications",
                column: "RelatedJournalId",
                principalTable: "Journals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ResearchPapers_RelatedPaperId",
                table: "Notifications",
                column: "RelatedPaperId",
                principalTable: "ResearchPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ResearchTopics_RelatedResearchTopicId",
                table: "Notifications",
                column: "RelatedResearchTopicId",
                principalTable: "ResearchTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Journals_RelatedJournalId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ResearchPapers_RelatedPaperId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ResearchTopics_RelatedResearchTopicId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "FollowSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ResearchTopics_Name",
                table: "ResearchTopics");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedJournalId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedPaperId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedResearchTopicId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_RelatedPaperId",
                table: "Notifications");

            migrationBuilder.DeleteData(
                table: "ResearchTopics",
                keyColumn: "Id",
                keyValue: new Guid("0bdaedb8-f741-4cf6-87c6-b82951db7d31"));

            migrationBuilder.DeleteData(
                table: "ResearchTopics",
                keyColumn: "Id",
                keyValue: new Guid("8698ace8-d52d-48d6-b750-319684d0ea30"));

            migrationBuilder.DeleteData(
                table: "SyncJobs",
                keyColumn: "Id",
                keyValue: new Guid("8f95e2d4-86c3-41e2-ae16-7dba70d82747"));

            migrationBuilder.DeleteData(
                table: "ApiDataSources",
                keyColumn: "Id",
                keyValue: new Guid("654c4415-64de-48f7-a0ea-094c00491f27"));

            migrationBuilder.DropColumn(
                name: "NotificationsCreated",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "RecordsCreated",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "RecordsUpdated",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "BatchSize",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "SearchQuery",
                table: "SyncJobs");

            migrationBuilder.DropColumn(
                name: "RelatedResearchTopicId",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTopics_Name",
                table: "ResearchTopics",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });
        }
    }
}
