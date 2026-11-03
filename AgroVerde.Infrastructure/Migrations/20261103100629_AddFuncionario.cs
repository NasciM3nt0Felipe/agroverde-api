using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funcionario",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    pessoa_id = table.Column<int>(type: "INTEGER", nullable: false),
                    propriedade_id = table.Column<int>(type: "INTEGER", nullable: false),
                    cargo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    salario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    data_contratacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    data_desligamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "FK_funcionario_Pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_funcionario_propriedade_propriedade_id",
                        column: x => x.propriedade_id,
                        principalTable: "propriedade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_funcionario_pessoa_id_propriedade_id",
                table: "funcionario",
                columns: new[] { "pessoa_id", "propriedade_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_funcionario_propriedade_id",
                table: "funcionario",
                column: "propriedade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "funcionario");
        }
    }
}
