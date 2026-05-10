namespace Neostore.Persistence.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<List<T>> ObterTodosAsync();
    Task<T> CriarAsync(T entidade);
    Task<T> AtualizarAsync(T entidade);
    Task<bool> DeletarAsync(Guid id);
    Task SaveChangesAsync();
}
