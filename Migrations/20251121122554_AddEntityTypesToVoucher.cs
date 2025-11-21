using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityTypesToVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MiddileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Segment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmployeeType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeMasters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeMasters");
        }
    }
}
