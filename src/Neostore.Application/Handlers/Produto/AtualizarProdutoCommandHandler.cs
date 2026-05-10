using MediatR;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class AtualizarProdutoCommandHandler : IRequestHandler<AtualizarProdutoCommand, ProdutoDto>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public AtualizarProdutoCommandHandler(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<ProdutoDto> Handle(AtualizarProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterComImagensAsync(request.Id);
        if (produto == null)
            throw new InvalidOperationException("Produto não encontrado.");

        if (produto.SKU != request.SKU && await _produtoRepository.ExistePorSkuAsync(request.SKU, request.Id))
            throw new InvalidOperationException($"SKU '{request.SKU}' já existe.");

        var categoria = await _categoriaRepository.ObterPorIdAsync(request.IdCategoria);
        if (categoria == null)
            throw new InvalidOperationException("Categoria não encontrada.");

        produto.Nome = request.Nome;
        produto.SKU = request.SKU;
        produto.Preço = request.Preço;
        produto.IdCategoria = request.IdCategoria;
        produto.Descrição = request.Descrição;
        produto.Estoque = request.Estoque;

        produto.Imagens = request.Imagens
            .Select(img => new Imagem
            {
                Id = Guid.NewGuid(),
                NomeArquivo = img.NomeArquivo,
                ChaveS3 = img.ChaveS3,
                TipoConteudo = img.TipoConteudo,
                TamanhoBytes = img.TamanhoBytes,
                IdProduto = request.Id,
                DataCriacao = DateTime.UtcNow
            })
            .ToList();

        await _produtoRepository.AtualizarAsync(produto);

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
