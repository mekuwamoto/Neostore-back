using MediatR;
using Neostore.Application.Commands.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class AjustarEstoqueCommandHandler : IRequestHandler<AjustarEstoqueCommand, int>
{
    private readonly IProdutoRepository _repository;

    public AjustarEstoqueCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(AjustarEstoqueCommand request, CancellationToken cancellationToken)
    {
        var produto = await _repository.ObterPorIdAsync(request.IdProduto);
        if (produto == null)
            throw new InvalidOperationException("Produto não encontrado.");

        produto.AjustarEstoque(request.Quantidade);
        await _repository.AtualizarAsync(produto);

        return produto.Estoque;
    }
}
