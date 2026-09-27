using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "talhao",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    propriedade_id = table.Column<int>(type: "INTEGER", nullable: false),
                    nome = table.Column<string>(type: "TEXT", nullable: false),
                    area = table.Column<double>(type: "REAL", nullable: false),
                    tipo_solo = table.Column<string>(type: "TEXT", nullable: true),
                    observacao = table.Column<string>(type: "TEXT", nullable: true),
                    ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_talhao", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "talhao");
        }
    }
}
