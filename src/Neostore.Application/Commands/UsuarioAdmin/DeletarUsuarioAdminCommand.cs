using MediatR;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record DeletarUsuarioAdminCommand(Guid Id) : IRequest<bool>;
