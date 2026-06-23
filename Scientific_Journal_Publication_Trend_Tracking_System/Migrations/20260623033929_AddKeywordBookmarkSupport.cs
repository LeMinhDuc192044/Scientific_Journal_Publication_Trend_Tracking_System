using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scientific_Journal_Publication_Trend_Tracking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordBookmarkSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId",
                table: "Bookmarks");

            migrationBuilder.AddColumn<Guid>(
                name: "KeywordId",
                table: "Bookmarks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPapers_Doi",
                table: "ResearchPapers",
                column: "Doi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_KeywordId",
                table: "Bookmarks",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_Type_KeywordId",
                table: "Bookmarks",
                columns: new[] { "UserId", "Type", "KeywordId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_Type_ResearchPaperId",
                table: "Bookmarks",
                columns: new[] { "UserId", "Type", "ResearchPaperId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_Keywords_KeywordId",
                table: "Bookmarks",
                column: "KeywordId",
                principalTable: "Keywords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_Keywords_KeywordId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_ResearchPapers_Doi",
                table: "ResearchPapers");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_KeywordId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_Type_KeywordId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_UserId_Type_ResearchPaperId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "KeywordId",
                table: "Bookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId",
                table: "Bookmarks",
                column: "UserId");
        }
    }
}
