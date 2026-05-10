using MediatR;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class CriarProdutoCommandHandler : IRequestHandler<CriarProdutoCommand, ProdutoDto>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public CriarProdutoCommandHandler(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<ProdutoDto> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        if (await _produtoRepository.ExistePorSkuAsync(request.SKU))
            throw new InvalidOperationException($"SKU '{request.SKU}' já existe.");

        var categoria = await _categoriaRepository.ObterPorIdAsync(request.IdCategoria);
        if (categoria == null)
            throw new InvalidOperationException("Categoria não encontrada.");

        var produto = new Domain.Entities.Produto
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            SKU = request.SKU,
            Preço = request.Preço,
            IdCategoria = request.IdCategoria,
            Descrição = request.Descrição,
            Estoque = request.Estoque,
            Imagens = request.Imagens
                .Select(img => new Imagem
                {
                    Id = Guid.NewGuid(),
                    NomeArquivo = img.NomeArquivo,
                    ChaveS3 = img.ChaveS3,
                    TipoConteudo = img.TipoConteudo,
                    TamanhoBytes = img.TamanhoBytes,
                    DataCriacao = DateTime.UtcNow
                })
                .ToList()
        };

        await _produtoRepository.CriarAsync(produto);

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
