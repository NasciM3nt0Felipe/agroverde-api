using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRebanhoEFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Animais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brinco = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Raca = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sexo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlesSanitarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnimalId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiagnosticoTratamento = table.Column<string>(type: "TEXT", nullable: false),
                    Medicamento = table.Column<string>(type: "TEXT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlesSanitarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlesSanitarios_Animais_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pesagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnimalId = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoKg = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    DataPesagem = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pesagens_Animais_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosReproducao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnimalId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoEvento = table.Column<int>(type: "INTEGER", nullable: false),
                    DataEvento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdentificacaoMacho = table.Column<string>(type: "TEXT", nullable: true),
                    PrevisaoParto = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosReproducao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosReproducao_Animais_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransacoesFinanceiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AnimalId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacoesFinanceiras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransacoesFinanceiras_Animais_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vacinacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnimalId = table.Column<int>(type: "INTEGER", nullable: false),
                    NomeVacina = table.Column<string>(type: "TEXT", nullable: false),
                    Lote = table.Column<string>(type: "TEXT", nullable: true),
                    Dose = table.Column<string>(type: "TEXT", nullable: true),
                    DataAplicacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProximaDose = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacinacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacinacoes_Animais_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlesSanitarios_AnimalId",
                table: "ControlesSanitarios",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Pesagens_AnimalId",
                table: "Pesagens",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosReproducao_AnimalId",
                table: "RegistrosReproducao",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesFinanceiras_AnimalId",
                table: "TransacoesFinanceiras",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacinacoes_AnimalId",
                table: "Vacinacoes",
                column: "AnimalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlesSanitarios");

            migrationBuilder.DropTable(
                name: "Pesagens");

            migrationBuilder.DropTable(
                name: "RegistrosReproducao");

            migrationBuilder.DropTable(
                name: "TransacoesFinanceiras");

            migrationBuilder.DropTable(
                name: "Vacinacoes");

            migrationBuilder.DropTable(
                name: "Animais");
        }
    }
}
