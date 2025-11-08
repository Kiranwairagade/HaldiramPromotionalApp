using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithoutMaterialMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmountReachGoalCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherValidity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountReachGoalCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FreeProductCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeProducts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeQuantities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeProductCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointsToCashCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherGenerationThreshold = table.Column<int>(type: "int", nullable: false),
                    VoucherValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherValidity = table.Column<int>(type: "int", nullable: false),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialPoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsToCashCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionDurationRewardCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionDuration = table.Column<int>(type: "int", nullable: false),
                    VoucherValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherValidity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDurationRewardCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointsRewardCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherGenerationThreshold = table.Column<int>(type: "int", nullable: false),
                    VoucherValidity = table.Column<int>(type: "int", nullable: false),
                    Materials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialPoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewardProductId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsRewardCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsRewardCampaigns_Products_RewardProductId",
                        column: x => x.RewardProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointsRewardCampaigns_RewardProductId",
                table: "PointsRewardCampaigns",
                column: "RewardProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountReachGoalCampaigns");

            migrationBuilder.DropTable(
                name: "FreeProductCampaigns");

            migrationBuilder.DropTable(
                name: "PointsRewardCampaigns");

            migrationBuilder.DropTable(
                name: "PointsToCashCampaigns");

            migrationBuilder.DropTable(
                name: "SessionDurationRewardCampaigns");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}