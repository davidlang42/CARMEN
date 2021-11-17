using Microsoft.EntityFrameworkCore.Migrations;

namespace Carmen.ShowModel.Migrations
{
    public partial class FixCommonExistingRoleCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Requirements SET ExistingRoleCost = AbilityExactRequirement_ExistingRoleCost WHERE AbilityExactRequirement_ExistingRoleCost IS NOT NULL");
            migrationBuilder.DropColumn(
                name: "AbilityExactRequirement_ExistingRoleCost",
                table: "Requirements");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AbilityExactRequirement_ExistingRoleCost",
                table: "Requirements",
                type: "REAL",
                nullable: true);
            migrationBuilder.Sql("UPDATE Requirements SET AbilityExactRequirement_ExistingRoleCost = ExistingRoleCost, ExistingRoleCost IS NULL WHERE Discriminator = 'AbilityExactRequirement'");
        }
    }
}
