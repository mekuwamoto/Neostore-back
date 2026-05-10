using Microsoft.EntityFrameworkCore;
using Neostore.Domain.Entities;
using Neostore.Persistence.Context;

namespace Neostore.Persistence.Repositories;

public class UsuarioAdminRepository : Repository<UsuarioAdmin>, IUsuarioAdminRepository
{
    public UsuarioAdminRepository(NeostoreDbContext context) : base(context) { }

    public async Task<UsuarioAdmin?> ObterPorEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public override async Task<bool> DeletarAsync(Guid id)
    {
        var usuario = await _context.UsuariosAdmin
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) return false;

        usuario.Ativo = false;
        usuario.DeletadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UsuarioAdmin?> ObterPorIdIncluindoInativoAsync(Guid id)
    {
        return await _context.UsuariosAdmin
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
