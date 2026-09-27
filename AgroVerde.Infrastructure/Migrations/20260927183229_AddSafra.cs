using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSafra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "safra",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    talhao_id = table.Column<int>(type: "INTEGER", nullable: false),
                    nome = table.Column<string>(type: "TEXT", nullable: false),
                    cultura = table.Column<string>(type: "TEXT", nullable: false),
                    variedade = table.Column<string>(type: "TEXT", nullable: true),
                    data_plantio = table.Column<string>(type: "TEXT", nullable: false),
                    data_colheita_prevista = table.Column<string>(type: "TEXT", nullable: true),
                    data_colheita_real = table.Column<string>(type: "TEXT", nullable: true),
                    producao_estimada = table.Column<double>(type: "REAL", nullable: true),
                    producao_obtida = table.Column<double>(type: "REAL", nullable: true),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    observacao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_safra", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "safra");
        }
    }
}
