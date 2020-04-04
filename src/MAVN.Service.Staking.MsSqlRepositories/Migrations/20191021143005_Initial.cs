using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.Staking.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "staking");

            migrationBuilder.CreateTable(
                name: "referral_stakes",
                schema: "staking",
                columns: table => new
                {
                    referral_id = table.Column<string>(nullable: false),
                    customer_id = table.Column<string>(nullable: false),
                    campaign_id = table.Column<string>(nullable: false),
                    amount = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    staking_period_in_days = table.Column<int>(nullable: false),
                    warning_period_in_days = table.Column<int>(nullable: false),
                    burn_ration = table.Column<decimal>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_stakes", x => x.referral_id);
                });

            migrationBuilder.CreateTable(
                name: "stakes_blockchain_info",
                schema: "staking",
                columns: table => new
                {
                    stake_id = table.Column<string>(nullable: false),
                    last_operation_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stakes_blockchain_info", x => x.stake_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_stakes",
                schema: "staking");

            migrationBuilder.DropTable(
                name: "stakes_blockchain_info",
                schema: "staking");
        }
    }
}
