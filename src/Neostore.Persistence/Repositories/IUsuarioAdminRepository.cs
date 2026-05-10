using Neostore.Domain.Entities;

namespace Neostore.Persistence.Repositories;

public interface IUsuarioAdminRepository : IRepository<UsuarioAdmin>
{
    Task<UsuarioAdmin?> ObterPorEmailAsync(string email);
    Task<UsuarioAdmin?> ObterPorIdIncluindoInativoAsync(Guid id);
}
