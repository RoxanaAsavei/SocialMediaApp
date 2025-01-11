using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class adaugnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupModerators_AspNetUsers_UserId",
                table: "GroupModerators");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupModerators_Groups_GroupId",
                table: "GroupModerators");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupModerators",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupModerators",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupModerators_AspNetUsers_UserId",
                table: "GroupModerators",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupModerators_Groups_GroupId",
                table: "GroupModerators",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupModerators_AspNetUsers_UserId",
                table: "GroupModerators");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupModerators_Groups_GroupId",
                table: "GroupModerators");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupModerators",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "GroupModerators",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupModerators_AspNetUsers_UserId",
                table: "GroupModerators",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupModerators_Groups_GroupId",
                table: "GroupModerators",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
