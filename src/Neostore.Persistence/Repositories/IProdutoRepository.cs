using Neostore.Domain.Entities;

namespace Neostore.Persistence.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> ObterPorSkuAsync(string sku);
    Task<List<Produto>> ObterPaginadoAsync(int pagina, int tamanho, Guid? idCategoria = null, string? nome = null, string? sku = null);
    Task<int> ContarTotalAsync(Guid? idCategoria = null, string? nome = null, string? sku = null);
    Task<bool> ExistePorSkuAsync(string sku, Guid? idExcluir = null);
    Task<Produto?> ObterComImagensAsync(Guid id);
    Task<Produto?> ObterPorIdIncluindoInativoAsync(Guid id);
}
