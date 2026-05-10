using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class ObterTodosProdutosQueryHandler : IRequestHandler<ObterTodosProdutosQuery, List<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public ObterTodosProdutosQueryHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProdutoDto>> Handle(ObterTodosProdutosQuery request, CancellationToken cancellationToken)
    {
        var produtos = await _repository.ObterTodosAsync();

        return produtos.Select(MapToProdutoDto).ToList();
    }

    private static ProdutoDto MapToProdutoDto(Domain.Entities.Produto produto)
    {
        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            SKU = produto.SKU,
            Preço = produto.Preço,
            IdCategoria = produto.IdCategoria,
            Descrição = produto.Descrição,
            Estoque = produto.Estoque,
            Imagens = produto.Imagens
                .Select(i => new ImagemDto
                {
                    Id = i.Id,
                    NomeArquivo = i.NomeArquivo,
                    ChaveS3 = i.ChaveS3,
                    TipoConteudo = i.TipoConteudo,
                    TamanhoBytes = i.TamanhoBytes,
                    DataCriacao = i.DataCriacao
                })
                .ToList()
        };
    }
}
