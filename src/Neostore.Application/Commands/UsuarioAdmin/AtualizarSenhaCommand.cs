using MediatR;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record AtualizarSenhaCommand(
    Guid Id,
    string SenhaAtual,
    string NovaSenha
) : IRequest<bool>;
