using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucherModelForEntityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_DealerMasters_DealerId",
                table: "Vouchers");

            migrationBuilder.AlterColumn<int>(
                name: "DealerId",
                table: "Vouchers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "EntityIdentifier",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Customer_Masters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shortname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    accounttype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneno = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer_Masters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpToCustMaps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    empl = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    phoneno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpToCustMaps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Cust2EmpMaps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    empt2custid = table.Column<int>(type: "int", nullable: false),
                    customer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cust2EmpMaps", x => x.id);
                    table.ForeignKey(
                        name: "FK_Cust2EmpMaps_EmpToCustMaps_empt2custid",
                        column: x => x.empt2custid,
                        principalTable: "EmpToCustMaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cust2EmpMaps_empt2custid",
                table: "Cust2EmpMaps",
                column: "empt2custid");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_DealerMasters_DealerId",
                table: "Vouchers",
                column: "DealerId",
                principalTable: "DealerMasters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_DealerMasters_DealerId",
                table: "Vouchers");

            migrationBuilder.DropTable(
                name: "Cust2EmpMaps");

            migrationBuilder.DropTable(
                name: "Customer_Masters");

            migrationBuilder.DropTable(
                name: "EmpToCustMaps");

            migrationBuilder.DropColumn(
                name: "EntityIdentifier",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Vouchers");

            migrationBuilder.AlterColumn<int>(
                name: "DealerId",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_DealerMasters_DealerId",
                table: "Vouchers",
                column: "DealerId",
                principalTable: "DealerMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
