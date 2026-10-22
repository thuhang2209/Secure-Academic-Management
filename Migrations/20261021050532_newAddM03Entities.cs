using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Secure_Academic_Management.Migrations
{
    /// <inheritdoc />
    public partial class newAddM03Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_Email",
                table: "SinhViens",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SinhViens_Email",
                table: "SinhViens");
        }
    }
}
