using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntidadesHistoricas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_ALERTAS_2TDSPR");

            migrationBuilder.DropTable(
                name: "TBL_MED_PLATAFORMA_2TDSPR");

            migrationBuilder.DropTable(
                name: "TBL_MED_VAGAO_2TDSPR");

            migrationBuilder.DropTable(
                name: "TBL_PARADAS_2TDSPR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_MED_PLATAFORMA_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MedidaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Origem = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    PlataformaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "NUMBER(10)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TBL_PARADAS_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PlataformaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TremId = table.Column<int>(type: "NUMBER(10)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TBL_ALERTAS_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    AcaoOperador = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CapacidadeDisponivelEstimada = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DemandaProjetada = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ParadaProgramadaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ALERTAS_2TDSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_ALERTAS_2TDSPR_TBL_PARADAS_2TDSPR_ParadaProgramadaId",
                        column: x => x.ParadaProgramadaId,
                        principalTable: "TBL_PARADAS_2TDSPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_MED_VAGAO_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Desembarques = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Embarques = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MedidaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    OcupacaoEstimada = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OcupacaoInicial = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Origem = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ParadaProgramadaId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    VagaoId = table.Column<int>(type: "NUMBER(10)", nullable: false)
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
                name: "IX_TBL_ALERTAS_2TDSPR_ParadaProgramadaId",
                table: "TBL_ALERTAS_2TDSPR",
                column: "ParadaProgramadaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_PLATAFORMA_2TDSPR_PlataformaId_MedidaEm",
                table: "TBL_MED_PLATAFORMA_2TDSPR",
                columns: new[] { "PlataformaId", "MedidaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_VAGAO_2TDSPR_ParadaProgramadaId",
                table: "TBL_MED_VAGAO_2TDSPR",
                column: "ParadaProgramadaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MED_VAGAO_2TDSPR_VagaoId_MedidaEm",
                table: "TBL_MED_VAGAO_2TDSPR",
                columns: new[] { "VagaoId", "MedidaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PARADAS_2TDSPR_PlataformaId_ChegadaPrevistaEm",
                table: "TBL_PARADAS_2TDSPR",
                columns: new[] { "PlataformaId", "ChegadaPrevistaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PARADAS_2TDSPR_TremId",
                table: "TBL_PARADAS_2TDSPR",
                column: "TremId");
        }
    }
}
