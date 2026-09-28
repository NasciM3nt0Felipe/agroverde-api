using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estoque_insumo",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    safra_id = table.Column<int>(type: "INTEGER", nullable: false),
                    estoque_item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    quantidade_utilizada = table.Column<double>(type: "REAL", nullable: false),
                    valor_total = table.Column<double>(type: "REAL", nullable: false),
                    data_movimentacao = table.Column<string>(type: "TEXT", nullable: false),
                    observacao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoque_insumo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estoque_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    propriedade_id = table.Column<int>(type: "INTEGER", nullable: false),
                    nome = table.Column<string>(type: "TEXT", nullable: false),
                    categoria = table.Column<string>(type: "TEXT", nullable: false),
                    quantidade_inicial = table.Column<double>(type: "REAL", nullable: false),
                    quantidade_atual = table.Column<double>(type: "REAL", nullable: false),
                    unidade_medida = table.Column<string>(type: "TEXT", nullable: false),
                    preco_medio_unitario = table.Column<double>(type: "REAL", nullable: false),
                    estoque_minimo = table.Column<double>(type: "REAL", nullable: false),
                    fornecedor = table.Column<string>(type: "TEXT", nullable: true),
                    observacao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoque_item", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estoque_insumo");

            migrationBuilder.DropTable(
                name: "estoque_item");
        }
    }
}
