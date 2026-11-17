using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertasOperacionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_ALERTAS_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ParadaProgramadaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DemandaProjetada = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CapacidadeDisponivelEstimada = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    AcaoOperador = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ALERTAS_2TDSPR_ParadaProgramadaId",
                table: "TBL_ALERTAS_2TDSPR",
                column: "ParadaProgramadaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_ALERTAS_2TDSPR");
        }
    }
}
