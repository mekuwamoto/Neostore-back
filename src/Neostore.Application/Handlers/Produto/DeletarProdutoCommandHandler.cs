using MediatR;
using Neostore.Application.Commands.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class DeletarProdutoCommandHandler : IRequestHandler<DeletarProdutoCommand, bool>
{
    private readonly IProdutoRepository _repository;

    public DeletarProdutoCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeletarProdutoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeletarAsync(request.Id);
    }
}
