using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Carmen.ShowModel.Migrations.MySqlMigrations
{
    public partial class AddAllowedConsecutives : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedConsecutives",
                columns: table => new
                {
                    AllowedConsecutiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedConsecutives", x => x.AllowedConsecutiveId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AllowedConsecutiveApplicant",
                columns: table => new
                {
                    AllowedConsecutivesAllowedConsecutiveId = table.Column<int>(type: "int", nullable: false),
                    CastApplicantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedConsecutiveApplicant", x => new { x.AllowedConsecutivesAllowedConsecutiveId, x.CastApplicantId });
                    table.ForeignKey(
                        name: "FK_AllowedConsecutiveApplicant_AllowedConsecutives_AllowedConse~",
                        column: x => x.AllowedConsecutivesAllowedConsecutiveId,
                        principalTable: "AllowedConsecutives",
                        principalColumn: "AllowedConsecutiveId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllowedConsecutiveApplicant_Applicants_CastApplicantId",
                        column: x => x.CastApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AllowedConsecutiveItem",
                columns: table => new
                {
                    AllowedConsecutivesAllowedConsecutiveId = table.Column<int>(type: "int", nullable: false),
                    ItemsNodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedConsecutiveItem", x => new { x.AllowedConsecutivesAllowedConsecutiveId, x.ItemsNodeId });
                    table.ForeignKey(
                        name: "FK_AllowedConsecutiveItem_AllowedConsecutives_AllowedConsecutiv~",
                        column: x => x.AllowedConsecutivesAllowedConsecutiveId,
                        principalTable: "AllowedConsecutives",
                        principalColumn: "AllowedConsecutiveId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllowedConsecutiveItem_Nodes_ItemsNodeId",
                        column: x => x.ItemsNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedConsecutiveApplicant_CastApplicantId",
                table: "AllowedConsecutiveApplicant",
                column: "CastApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedConsecutiveItem_ItemsNodeId",
                table: "AllowedConsecutiveItem",
                column: "ItemsNodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedConsecutiveApplicant");

            migrationBuilder.DropTable(
                name: "AllowedConsecutiveItem");

            migrationBuilder.DropTable(
                name: "AllowedConsecutives");
        }
    }
}
