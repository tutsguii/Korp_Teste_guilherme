using Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Persistence;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<MensagemProcessada> MensagensProcessadas => Set<MensagemProcessada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(builder =>
        {
            builder.ToTable("produtos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Codigo)
                .HasColumnName("codigo")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Saldo)
                .HasColumnName("saldo")
                .IsRequired();

          

            builder.HasIndex(x => x.Codigo).IsUnique();
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
