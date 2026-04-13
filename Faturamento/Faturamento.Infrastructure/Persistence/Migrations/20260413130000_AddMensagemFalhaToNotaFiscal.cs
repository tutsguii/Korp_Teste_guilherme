using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faturamento.Infrastructure.Persistence.Migrations
{
    public partial class AddMensagemFalhaToNotaFiscal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mensagem_falha",
                table: "notas_fiscais",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mensagem_falha",
                table: "notas_fiscais");
        }
    }
}
