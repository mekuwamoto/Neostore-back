using MediatR;
using Neostore.Application.Commands.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class DeletarCategoriaCommandHandler : IRequestHandler<DeletarCategoriaCommand, bool>
{
    private readonly ICategoriaRepository _repository;

    public DeletarCategoriaCommandHandler(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeletarCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.ObterPorIdAsync(request.Id);
        if (categoria == null)
            return false;

        var contarSubcategorias = await _repository.ContarSubcategoriasAsync(request.Id);
        if (contarSubcategorias > 0)
            throw new InvalidOperationException("Não é possível deletar categoria que possui subcategorias.");

        var contarProdutos = await _repository.ContarProdutosAsync(request.Id);
        if (contarProdutos > 0)
            throw new InvalidOperationException("Não é possível deletar categoria que possui produtos.");

        return await _repository.DeletarAsync(request.Id);
    }
}
