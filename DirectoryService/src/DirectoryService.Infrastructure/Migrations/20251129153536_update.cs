using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "department_positions",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_positions",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_positions_PositionId",
                table: "department_positions",
                newName: "IX_department_positions_position_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_positions_DepartmentId",
                table: "department_positions",
                newName: "IX_department_positions_department_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "department_positions",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_positions",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions",
                newName: "IX_department_positions_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions",
                newName: "IX_department_positions_DepartmentId");
        }
    }
}
