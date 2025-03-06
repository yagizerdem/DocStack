using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Authors = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<string>(type: "TEXT", nullable: true),
                    FileData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Papers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Authors = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    DOI = table.Column<string>(type: "TEXT", nullable: true),
                    FullTextLink = table.Column<string>(type: "TEXT", nullable: true),
                    Abstract = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<string>(type: "TEXT", nullable: true),
                    StarredEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Papers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Starred",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ColorFilter = table.Column<int>(type: "INTEGER", nullable: false),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starred", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Starred_Papers_PaperEntityId",
                        column: x => x.PaperEntityId,
                        principalTable: "Papers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Starred_PaperEntityId",
                table: "Starred",
                column: "PaperEntityId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyDocuments");

            migrationBuilder.DropTable(
                name: "Starred");

            migrationBuilder.DropTable(
                name: "Papers");
        }
    }
}
