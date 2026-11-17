using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicoesPlataforma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_MED_PLATAFORMA_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PlataformaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MedidaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Origem = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_MED_PLATAFORMA_2TDSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_MED_PLATAFORMA_2TDSPR_Plataforma_PlataformaId",
                        column: x => x.PlataformaId,
                        principalTable: "Plataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_PLATAFORMA_2TDSPR_PlataformaId_MedidaEm",
                table: "TBL_MED_PLATAFORMA_2TDSPR",
                columns: new[] { "PlataformaId", "MedidaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_MED_PLATAFORMA_2TDSPR");
        }
    }
}
