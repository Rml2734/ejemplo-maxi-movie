using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maxi_movie_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCacheIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumenIaCache",
                table: "Peliculas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpoilerIaCache",
                table: "Peliculas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumenIaCache",
                table: "Peliculas");

            migrationBuilder.DropColumn(
                name: "SpoilerIaCache",
                table: "Peliculas");
        }
    }
}
