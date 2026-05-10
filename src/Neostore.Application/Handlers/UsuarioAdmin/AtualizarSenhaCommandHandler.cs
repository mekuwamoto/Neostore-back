using MediatR;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.UsuarioAdmin;

public class AtualizarSenhaCommandHandler : IRequestHandler<AtualizarSenhaCommand, bool>
{
    private readonly IUsuarioAdminRepository _repository;

    public AtualizarSenhaCommandHandler(IUsuarioAdminRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(AtualizarSenhaCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.UsuarioAdmin? usuario = await _repository.ObterPorIdAsync(request.Id);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        bool senhaCorreta = BCrypt.Net.BCrypt.Verify(request.SenhaAtual, usuario.SenhaHash);
        if (!senhaCorreta)
            throw new InvalidOperationException("Senha atual incorreta.");

        string novoHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        usuario.AtualizarSenha(novoHash);

        await _repository.AtualizarAsync(usuario);

        return true;
    }
}
