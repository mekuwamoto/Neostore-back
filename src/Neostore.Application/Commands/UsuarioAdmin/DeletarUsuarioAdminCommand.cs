using MediatR;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record DeletarUsuarioAdminCommand(Guid Id) : IRequest<bool>;

public class DeletarUsuarioAdminCommandHandler : IRequestHandler<DeletarUsuarioAdminCommand, bool>
{
    private readonly IUsuarioAdminRepository _repository;

    public DeletarUsuarioAdminCommandHandler(IUsuarioAdminRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeletarUsuarioAdminCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeletarAsync(request.Id);
    }
}
