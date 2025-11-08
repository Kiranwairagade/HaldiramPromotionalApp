using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaterialTypeFromCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "PointsToCashCampaigns");

            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "PointsRewardCampaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialType",
                table: "PointsToCashCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaterialType",
                table: "PointsRewardCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
