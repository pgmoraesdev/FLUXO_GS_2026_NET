using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddAtributosTremEstacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fabricante",
                table: "TBL_TRENS_2TDSPR",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modelo",
                table: "TBL_TRENS_2TDSPR",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "TBL_ESTACOES_2TDSPR",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "TBL_ESTACOES_2TDSPR",
                type: "NVARCHAR2(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fabricante",
                table: "TBL_TRENS_2TDSPR");

            migrationBuilder.DropColumn(
                name: "Modelo",
                table: "TBL_TRENS_2TDSPR");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "TBL_ESTACOES_2TDSPR");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "TBL_ESTACOES_2TDSPR");
        }
    }
}
