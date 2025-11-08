using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialMasterId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialImages_MaterialMaster_MaterialMasterId",
                        column: x => x.MaterialMasterId,
                        principalTable: "MaterialMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialImages_MaterialMasterId",
                table: "MaterialImages",
                column: "MaterialMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialImages");
        }
    }
}
