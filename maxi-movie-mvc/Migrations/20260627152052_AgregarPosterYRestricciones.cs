using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maxi_movie_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPosterYRestricciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PosterUrlImagen",
                table: "Peliculas",
                newName: "PosterUrlPortada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PosterUrlPortada",
                table: "Peliculas",
                newName: "PosterUrlImagen");
        }
    }
}
