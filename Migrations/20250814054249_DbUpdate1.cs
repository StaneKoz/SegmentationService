using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SegmentationService.Migrations
{
    /// <inheritdoc />
    public partial class DbUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Segments_Name",
                table: "Segments",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Segments_Name",
                table: "Segments");
        }
    }
}
