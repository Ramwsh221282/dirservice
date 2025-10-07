using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Addeddeparmentattachmentshistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachments",
                table: "departments",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "childrens_count",
                table: "departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attachments",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "childrens_count",
                table: "departments");
        }
    }
}
