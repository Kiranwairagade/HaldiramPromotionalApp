using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaterialTypeFromFreeProductCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "FreeProductCampaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialType",
                table: "FreeProductCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
