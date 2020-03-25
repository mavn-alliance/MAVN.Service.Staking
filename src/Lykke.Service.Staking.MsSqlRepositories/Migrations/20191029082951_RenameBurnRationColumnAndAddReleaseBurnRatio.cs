using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.Staking.MsSqlRepositories.Migrations
{
    public partial class RenameBurnRationColumnAndAddReleaseBurnRatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "burn_ration",
                schema: "staking",
                table: "referral_stakes",
                newName: "expiration_burn_ratio");

            migrationBuilder.AddColumn<decimal>(
                name: "release_burn_ratio",
                schema: "staking",
                table: "referral_stakes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "release_burn_ratio",
                schema: "staking",
                table: "referral_stakes");

            migrationBuilder.RenameColumn(
                name: "expiration_burn_ratio",
                schema: "staking",
                table: "referral_stakes",
                newName: "burn_ration");
        }
    }
}
