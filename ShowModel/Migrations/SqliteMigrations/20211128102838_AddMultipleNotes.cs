using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Carmen.ShowModel.Migrations.SqliteMigrations
{
    public partial class AddMultipleNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    NoteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_Notes_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ApplicantId",
                table: "Notes",
                column: "ApplicantId");

            migrationBuilder.Sql("INSERT INTO Notes(ApplicantId, Text, Author, Timestamp) SELECT ApplicantId, Notes AS Text, 'Migration' AS Author, DATETIME('now') AS Timestamp FROM Applicants WHERE Text NOTNULL AND Text <> ''");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Applicants");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Applicants",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
