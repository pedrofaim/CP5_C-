using EstoqueFacil.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var produto = modelBuilder.Entity<Produto>();
        produto.ToTable("Produtos");
        produto.HasKey(p => p.Id);
        produto.Property(p => p.Nome).IsRequired().HasMaxLength(100);
        produto.Property(p => p.Categoria).IsRequired().HasMaxLength(60);
        produto.Property(p => p.Preco).HasPrecision(8, 2);
    }
}
