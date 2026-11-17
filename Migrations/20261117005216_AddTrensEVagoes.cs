using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluxo_GS_2026_2TDSPR.Migrations
{
    /// <inheritdoc />
    public partial class AddTrensEVagoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_TRENS_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Codigo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_TRENS_2TDSPR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_VAGOES_2TDSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TremId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Posicao = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CapacidadePessoas = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_VAGOES_2TDSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_VAGOES_2TDSPR_TBL_TRENS_2TDSPR_TremId",
                        column: x => x.TremId,
                        principalTable: "TBL_TRENS_2TDSPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_TRENS_2TDSPR_Codigo",
                table: "TBL_TRENS_2TDSPR",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_VAGOES_2TDSPR_TremId_Posicao",
                table: "TBL_VAGOES_2TDSPR",
                columns: new[] { "TremId", "Posicao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_VAGOES_2TDSPR");

            migrationBuilder.DropTable(
                name: "TBL_TRENS_2TDSPR");
        }
    }
}
