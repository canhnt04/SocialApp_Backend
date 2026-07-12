using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialApp.PostService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalPostRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Posts_OriginalPostId",
                table: "Posts",
                column: "OriginalPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts",
                column: "OriginalPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_OriginalPostId",
                table: "Posts");
        }
    }
}
