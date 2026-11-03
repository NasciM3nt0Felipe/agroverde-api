using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVeiculoPropriedadeRegistros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Veiculos_PlacaOuChassi",
                table: "Veiculos");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataVenda",
                table: "Veiculos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Veiculos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PropriedadeId",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ValorVenda",
                table: "Veiculos",
                type: "REAL",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AbastecimentosVeiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VeiculoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Litros = table.Column<double>(type: "REAL", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    HorimetroOdometro = table.Column<double>(type: "REAL", nullable: true),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbastecimentosVeiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbastecimentosVeiculo_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManutencoesVeiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VeiculoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    HorimetroOdometro = table.Column<double>(type: "REAL", nullable: true),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManutencoesVeiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManutencoesVeiculo_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_PropriedadeId",
                table: "Veiculos",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_PropriedadeId_PlacaOuChassi",
                table: "Veiculos",
                columns: new[] { "PropriedadeId", "PlacaOuChassi" },
                unique: true,
                filter: "\"PlacaOuChassi\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbastecimentosVeiculo_VeiculoId",
                table: "AbastecimentosVeiculo",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_ManutencoesVeiculo_VeiculoId",
                table: "ManutencoesVeiculo",
                column: "VeiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbastecimentosVeiculo");

            migrationBuilder.DropTable(
                name: "ManutencoesVeiculo");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_PropriedadeId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_PropriedadeId_PlacaOuChassi",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "DataVenda",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "PropriedadeId",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "ValorVenda",
                table: "Veiculos");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_PlacaOuChassi",
                table: "Veiculos",
                column: "PlacaOuChassi",
                unique: true,
                filter: "[PlacaOuChassi] IS NOT NULL");
        }
    }
}
