using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropriedadeEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "propriedade",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "propriedade",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "propriedade",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cidade",
                table: "propriedade");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "propriedade");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "propriedade");
        }
    }
}
