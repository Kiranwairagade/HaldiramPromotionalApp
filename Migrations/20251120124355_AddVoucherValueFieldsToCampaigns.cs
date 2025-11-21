using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherValueFieldsToCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistributorVoucherValue",
                table: "SessionDurationRewardCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesVoucherValue",
                table: "SessionDurationRewardCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributorVoucherValue",
                table: "PointsToCashCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesVoucherValue",
                table: "PointsToCashCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributorVoucherValue",
                table: "PointsRewardCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesVoucherValue",
                table: "PointsRewardCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributorVoucherValue",
                table: "FreeProductCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesVoucherValue",
                table: "FreeProductCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributorVoucherValue",
                table: "AmountReachGoalCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesVoucherValue",
                table: "AmountReachGoalCampaigns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistributorVoucherValue",
                table: "SessionDurationRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "SalesVoucherValue",
                table: "SessionDurationRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "DistributorVoucherValue",
                table: "PointsToCashCampaigns");

            migrationBuilder.DropColumn(
                name: "SalesVoucherValue",
                table: "PointsToCashCampaigns");

            migrationBuilder.DropColumn(
                name: "DistributorVoucherValue",
                table: "PointsRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "SalesVoucherValue",
                table: "PointsRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "DistributorVoucherValue",
                table: "FreeProductCampaigns");

            migrationBuilder.DropColumn(
                name: "SalesVoucherValue",
                table: "FreeProductCampaigns");

            migrationBuilder.DropColumn(
                name: "DistributorVoucherValue",
                table: "AmountReachGoalCampaigns");

            migrationBuilder.DropColumn(
                name: "SalesVoucherValue",
                table: "AmountReachGoalCampaigns");
        }
    }
}
