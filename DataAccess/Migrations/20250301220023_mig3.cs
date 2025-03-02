using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StarredEntity_Papers_PaperEntityId",
                table: "StarredEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StarredEntity",
                table: "StarredEntity");

            migrationBuilder.RenameTable(
                name: "StarredEntity",
                newName: "Starred");

            migrationBuilder.RenameIndex(
                name: "IX_StarredEntity_PaperEntityId",
                table: "Starred",
                newName: "IX_Starred_PaperEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Starred",
                table: "Starred",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Starred_Papers_PaperEntityId",
                table: "Starred",
                column: "PaperEntityId",
                principalTable: "Papers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Starred_Papers_PaperEntityId",
                table: "Starred");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Starred",
                table: "Starred");

            migrationBuilder.RenameTable(
                name: "Starred",
                newName: "StarredEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Starred_PaperEntityId",
                table: "StarredEntity",
                newName: "IX_StarredEntity_PaperEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StarredEntity",
                table: "StarredEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StarredEntity_Papers_PaperEntityId",
                table: "StarredEntity",
                column: "PaperEntityId",
                principalTable: "Papers",
                principalColumn: "Id");
        }
    }
}
