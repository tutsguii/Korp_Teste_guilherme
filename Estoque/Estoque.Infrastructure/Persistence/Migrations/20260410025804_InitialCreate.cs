using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estoque.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                name: "produtos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    saldo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mensagens_processadas_message_id",
                table: "mensagens_processadas",
                column: "message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_produtos_codigo",
                table: "produtos",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensagens_processadas");

            migrationBuilder.DropTable(
                name: "produtos");
        }
    }
}
