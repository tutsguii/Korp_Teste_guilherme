using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faturamento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixItemNotaFiscalRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensagens_processadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_processamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensagens_processadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_fiscais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "itens_nota_fiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    nota_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_nota_fiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_nota_fiscal_notas_fiscais_nota_fiscal_id",
                        column: x => x.nota_fiscal_id,
                        principalTable: "notas_fiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_nota_fiscal_nota_fiscal_id",
                table: "itens_nota_fiscal",
                column: "nota_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "IX_mensagens_processadas_message_id",
                table: "mensagens_processadas",
                column: "message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notas_fiscais_numero",
                table: "notas_fiscais",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_nota_fiscal");

            migrationBuilder.DropTable(
                name: "mensagens_processadas");

            migrationBuilder.DropTable(
                name: "notas_fiscais");
        }
    }
}
