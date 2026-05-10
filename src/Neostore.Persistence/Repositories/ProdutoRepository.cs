using Microsoft.EntityFrameworkCore;
using Neostore.Domain.Entities;
using Neostore.Persistence.Context;

namespace Neostore.Persistence.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(NeostoreDbContext context) : base(context) { }

    public async Task<Produto?> ObterPorSkuAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<List<Produto>> ObterPaginadoAsync(int pagina, int tamanho, Guid? idCategoria = null, string? nome = null, string? sku = null)
    {
        var query = _dbSet.AsQueryable();

        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => p.Nome.Contains(nome));

        if (!string.IsNullOrWhiteSpace(sku))
            query = query.Where(p => p.SKU.Contains(sku));

        return await query
            .Include(p => p.Imagens)
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();
    }

    public async Task<int> ContarTotalAsync(Guid? idCategoria = null, string? nome = null, string? sku = null)
    {
        var query = _dbSet.AsQueryable();

        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => p.Nome.Contains(nome));

        if (!string.IsNullOrWhiteSpace(sku))
            query = query.Where(p => p.SKU.Contains(sku));

        return await query.CountAsync();
    }

    public async Task<bool> ExistePorSkuAsync(string sku, Guid? idExcluir = null)
    {
        var query = _dbSet.Where(p => p.SKU == sku);
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);

        return await query.AnyAsync();
    }

    public async Task<Produto?> ObterComImagensAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Imagens)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<bool> DeletarAsync(Guid id)
    {
        var produto = await _context.Produtos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null) return false;

        produto.Ativo = false;
        produto.DeletadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Produto?> ObterPorIdIncluindoInativoAsync(Guid id)
    {
        return await _context.Produtos
            .IgnoreQueryFilters()
            .Include(p => p.Imagens)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
