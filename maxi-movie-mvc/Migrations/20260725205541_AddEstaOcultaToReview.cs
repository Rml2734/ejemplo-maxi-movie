using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace maxi_movie_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddEstaOcultaToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaOculta",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaOculta",
                table: "Reviews");
        }
    }
}
