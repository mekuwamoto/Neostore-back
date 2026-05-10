using Microsoft.EntityFrameworkCore;
using Neostore.Domain.Entities;
using Neostore.Persistence.Context;

namespace Neostore.Persistence.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(NeostoreDbContext context) : base(context) { }

    public async Task<Categoria?> ObterPorSlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<List<Categoria>> ObterArvoreAsync()
    {
        return await _dbSet
            .OrderBy(c => c.IdCategoriaPai)
            .ThenBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<List<Categoria>> ObterRaizAsync()
    {
        return await _dbSet
            .Where(c => c.IdCategoriaPai == null)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<bool> ExistePorNomeAsync(string nome, Guid? idExcluir = null)
    {
        var query = _dbSet.Where(c => c.Nome == nome);
        if (idExcluir.HasValue)
            query = query.Where(c => c.Id != idExcluir.Value);

        return await query.AnyAsync();
    }

    public async Task<int> ContarProdutosAsync(Guid idCategoria)
    {
        var context = (NeostoreDbContext)_context;
        return await context.Produtos
            .Where(p => p.IdCategoria == idCategoria)
            .CountAsync();
    }

    public async Task<int> ContarSubcategoriasAsync(Guid idCategoria)
    {
        return await _dbSet
            .Where(c => c.IdCategoriaPai == idCategoria)
            .CountAsync();
    }
}
