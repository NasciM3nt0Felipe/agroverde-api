using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroVerde.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrigemTransacaoFinanceira : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrigemId",
                table: "TransacoesFinanceiras",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrigemTipo",
                table: "TransacoesFinanceiras",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesFinanceiras_OrigemTipo_OrigemId",
                table: "TransacoesFinanceiras",
                columns: new[] { "OrigemTipo", "OrigemId" },
                unique: true,
                filter: "\"OrigemTipo\" IS NOT NULL AND \"OrigemId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransacoesFinanceiras_OrigemTipo_OrigemId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "OrigemId",
                table: "TransacoesFinanceiras");

            migrationBuilder.DropColumn(
                name: "OrigemTipo",
                table: "TransacoesFinanceiras");
        }
    }
}
