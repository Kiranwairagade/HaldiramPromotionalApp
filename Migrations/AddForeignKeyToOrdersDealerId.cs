using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyToOrdersDealerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any Orders with DealerId that don't exist in DealerMasters
            migrationBuilder.Sql(@"
                DELETE FROM Orders 
                WHERE DealerId IS NOT NULL 
                AND DealerId NOT IN (SELECT Id FROM DealerMasters WHERE Id IS NOT NULL)
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DealerId",
                table: "Orders",
                column: "DealerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DealerMasters_DealerId",
                table: "Orders",
                column: "DealerId",
                principalTable: "DealerMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DealerMasters_DealerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DealerId",
                table: "Orders");
        }
    }
}