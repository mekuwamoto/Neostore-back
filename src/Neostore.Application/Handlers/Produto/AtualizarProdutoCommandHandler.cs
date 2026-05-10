using AutoMapper;
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
    private readonly IMapper _mapper;

    public AtualizarProdutoCommandHandler(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
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

        return _mapper.Map<Domain.Entities.Produto, ProdutoDto>(produto);
    }
}
