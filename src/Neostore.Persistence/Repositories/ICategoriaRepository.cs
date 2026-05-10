using Neostore.Domain.Entities;

namespace Neostore.Persistence.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<Categoria?> ObterPorSlugAsync(string slug);
    Task<List<Categoria>> ObterArvoreAsync();
    Task<List<Categoria>> ObterRaizAsync();
    Task<bool> ExistePorNomeAsync(string nome, Guid? idExcluir = null);
    Task<int> ContarProdutosAsync(Guid idCategoria);
    Task<int> ContarSubcategoriasAsync(Guid idCategoria);
}
