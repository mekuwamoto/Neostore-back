using Microsoft.EntityFrameworkCore;
using Neostore.Domain.Entities;

namespace Neostore.Persistence.Context;

public class NeostoreDbContext : DbContext
{
    public NeostoreDbContext(DbContextOptions<NeostoreDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Produto> Produtos { get; set; } = null!;
    public DbSet<Imagem> Imagens { get; set; } = null!;
    public DbSet<UsuarioAdmin> UsuariosAdmin { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NeostoreDbContext).Assembly);
    }
}
