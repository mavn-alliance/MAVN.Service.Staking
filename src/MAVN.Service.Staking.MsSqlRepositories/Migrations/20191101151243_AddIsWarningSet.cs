using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.Staking.MsSqlRepositories.Migrations
{
    public partial class AddIsWarningSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_warning_sent",
                schema: "staking",
                table: "referral_stakes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_warning_sent",
                schema: "staking",
                table: "referral_stakes");
        }
    }
}
