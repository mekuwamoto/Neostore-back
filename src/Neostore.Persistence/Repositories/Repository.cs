using Microsoft.EntityFrameworkCore;
using Neostore.Persistence.Context;

namespace Neostore.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly NeostoreDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(NeostoreDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> ObterTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> CriarAsync(T entidade)
    {
        _dbSet.Add(entidade);
        await _context.SaveChangesAsync();
        return entidade;
    }

    public virtual async Task<T> AtualizarAsync(T entidade)
    {
        _dbSet.Update(entidade);
        await _context.SaveChangesAsync();
        return entidade;
    }

    public virtual async Task<bool> DeletarAsync(Guid id)
    {
        var entidade = await ObterPorIdAsync(id);
        if (entidade == null)
            return false;

        _dbSet.Remove(entidade);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
