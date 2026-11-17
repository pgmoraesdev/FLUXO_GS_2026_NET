using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicoesVagao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_MED_VAGAO_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VagaoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ParadaProgramadaId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    OcupacaoInicial = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Embarques = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Desembarques = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OcupacaoEstimada = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MedidaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Origem = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_MED_VAGAO_2TDSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_MED_VAGAO_2TDSPR_TBL_PARADAS_2TDSPR_ParadaProgramadaId",
                        column: x => x.ParadaProgramadaId,
                        principalTable: "TBL_PARADAS_2TDSPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_MED_VAGAO_2TDSPR_TBL_VAGOES_2TDSPR_VagaoId",
                        column: x => x.VagaoId,
                        principalTable: "TBL_VAGOES_2TDSPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_VAGAO_2TDSPR_ParadaProgramadaId",
                table: "TBL_MED_VAGAO_2TDSPR",
                column: "ParadaProgramadaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_VAGAO_2TDSPR_VagaoId_MedidaEm",
                table: "TBL_MED_VAGAO_2TDSPR",
                columns: new[] { "VagaoId", "MedidaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_MED_VAGAO_2TDSPR");
        }
    }
}
