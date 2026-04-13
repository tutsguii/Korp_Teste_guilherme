using Estoque.Domain.Entities;
using Estoque.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Configuration;

public static class DbInitializer
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.Produtos.AnyAsync())
        {
            context.Produtos.Add(new Produto("P001", "Produto A", 10));
            context.Produtos.Add(new Produto("P002", "Produto B", 5));
            context.Produtos.Add(new Produto("P003", "Produto C", 1));

            await context.SaveChangesAsync();
        }
    }
}
