using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddOcupacaoAtual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "TBL_VAGOES_2TDSPR",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadePessoas",
                table: "TBL_VAGOES_2TDSPR",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "Plataforma",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadePessoas",
                table: "Plataforma",
                type: "NUMBER(10)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "TBL_VAGOES_2TDSPR");

            migrationBuilder.DropColumn(
                name: "QuantidadePessoas",
                table: "TBL_VAGOES_2TDSPR");

            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "Plataforma");

            migrationBuilder.DropColumn(
                name: "QuantidadePessoas",
                table: "Plataforma");
        }
    }
}
