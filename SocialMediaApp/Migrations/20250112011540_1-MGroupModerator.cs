using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class _1MGroupModerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupModerators");

            migrationBuilder.AddColumn<string>(
                name: "ModeratorId",
                table: "Groups",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ModeratorId",
                table: "Groups",
                column: "ModeratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_ModeratorId",
                table: "Groups",
                column: "ModeratorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_ModeratorId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ModeratorId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ModeratorId",
                table: "Groups");

            migrationBuilder.CreateTable(
                name: "GroupModerators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupModerators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupModerators_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupModerators_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupModerators_GroupId",
                table: "GroupModerators",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupModerators_UserId",
                table: "GroupModerators",
                column: "UserId");
        }
    }
}
