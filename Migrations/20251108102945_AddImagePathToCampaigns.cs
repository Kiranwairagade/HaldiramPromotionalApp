using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "SessionDurationRewardCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "PointsToCashCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "PointsRewardCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FreeProductCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "AmountReachGoalCampaigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "SessionDurationRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "PointsToCashCampaigns");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "PointsRewardCampaigns");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FreeProductCampaigns");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "AmountReachGoalCampaigns");
        }
    }
}
