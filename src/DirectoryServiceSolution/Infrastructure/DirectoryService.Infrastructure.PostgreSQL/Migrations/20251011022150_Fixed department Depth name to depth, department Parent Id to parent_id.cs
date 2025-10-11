using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class FixeddepartmentDepthnametodepthdepartmentParentIdtoparent_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "departments",
                newName: "depth");

            migrationBuilder.RenameColumn(
                name: "Parent",
                table: "departments",
                newName: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "depth",
                table: "departments",
                newName: "Depth");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "departments",
                newName: "Parent");
        }
    }
}
