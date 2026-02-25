using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_department_identifier",
                table: "departments",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_name",
                table: "departments",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_department_identifier",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "ix_department_name",
                table: "departments");
        }
    }
}
