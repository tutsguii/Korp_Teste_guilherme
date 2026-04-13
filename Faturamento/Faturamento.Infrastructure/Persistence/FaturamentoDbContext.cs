using Faturamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Infrastructure.Persistence;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options)
    {
    }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNotaFiscal> ItensNotaFiscal => Set<ItemNotaFiscal>();
    public DbSet<MensagemProcessada> MensagensProcessadas => Set<MensagemProcessada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(builder =>
        {
            builder.ToTable("notas_fiscais");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Numero)
                .HasColumnName("numero")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired();

            builder.Property(x => x.DataCriacao)
                .HasColumnName("data_criacao")
                .IsRequired();

            builder.Property(x => x.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.MensagemFalha)
                .HasColumnName("mensagem_falha")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasMany(x => x.Itens)
                .WithOne()
                .HasForeignKey(x => x.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Numero).IsUnique();
        });

        modelBuilder.Entity<ItemNotaFiscal>(builder =>
        {
            builder.ToTable("itens_nota_fiscal");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProdutoId)
                .HasColumnName("produto_id")
                .IsRequired();

            builder.Property(x => x.Quantidade)
                .HasColumnName("quantidade")
                .IsRequired();

            builder.Property(x => x.NotaFiscalId)
                .HasColumnName("nota_fiscal_id")
                .IsRequired();
        });

        modelBuilder.Entity<MensagemProcessada>(builder =>
        {
            builder.ToTable("mensagens_processadas");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageId)
                .HasColumnName("message_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DataProcessamento)
                .HasColumnName("data_processamento")
                .IsRequired();

            builder.HasIndex(x => x.MessageId).IsUnique();
        });
    }
}
