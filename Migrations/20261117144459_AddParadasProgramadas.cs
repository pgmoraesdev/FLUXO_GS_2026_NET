using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddParadasProgramadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_PARADAS_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TremId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PlataformaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ChegadaPrevistaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ChegadaRealEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_PARADAS_2TDSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_PARADAS_2TDSPR_Plataforma_PlataformaId",
                        column: x => x.PlataformaId,
                        principalTable: "Plataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_PARADAS_2TDSPR_TBL_TRENS_2TDSPR_TremId",
                        column: x => x.TremId,
                        principalTable: "TBL_TRENS_2TDSPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PARADAS_2TDSPR_PlataformaId_ChegadaPrevistaEm",
                table: "TBL_PARADAS_2TDSPR",
                columns: new[] { "PlataformaId", "ChegadaPrevistaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PARADAS_2TDSPR_TremId",
                table: "TBL_PARADAS_2TDSPR",
                column: "TremId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_PARADAS_2TDSPR");
        }
    }
}
