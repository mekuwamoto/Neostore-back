using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class ObterProdutoPorIdQueryHandler : IRequestHandler<ObterProdutoPorIdQuery, ProdutoDto?>
{
    private readonly IProdutoRepository _repository;

    public ObterProdutoPorIdQueryHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProdutoDto?> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await _repository.ObterComImagensAsync(request.Id);
        if (produto == null)
            return null;

        return MapToProdutoDto(produto);
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
