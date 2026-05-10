using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class ObterProdutosPaginadoQueryHandler : IRequestHandler<ObterProdutosPaginadoQuery, ProdutosPaginadoDto>
{
    private readonly IProdutoRepository _repository;

    public ObterProdutosPaginadoQueryHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProdutosPaginadoDto> Handle(ObterProdutosPaginadoQuery request, CancellationToken cancellationToken)
    {
        var produtos = await _repository.ObterPaginadoAsync(request.Pagina, request.Tamanho, request.IdCategoria, request.Nome, request.SKU);
        var total = await _repository.ContarTotalAsync(request.IdCategoria, request.Nome, request.SKU);

        return new ProdutosPaginadoDto
        {
            Dados = produtos.Select(MapToProdutoDto).ToList(),
            Total = total,
            Pagina = request.Pagina,
            Tamanho = request.Tamanho
        };
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
