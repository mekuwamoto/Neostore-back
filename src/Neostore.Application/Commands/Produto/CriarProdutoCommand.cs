using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Neostore.Application.DTOs;
using Neostore.Application.Interfaces;
using Neostore.Domain.Entities;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.Produto;

public record CriarProdutoCommand(
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    int Estoque,
    List<IFormFile> Imagens
) : IRequest<ProdutoDto>;

public class CriarProdutoCommandHandler : IRequestHandler<CriarProdutoCommand, ProdutoDto>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IS3Service _s3Service;
    private readonly IMapper _mapper;

    public CriarProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        ICategoriaRepository categoriaRepository,
        IS3Service s3Service,
        IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
        _s3Service = s3Service;
        _mapper = mapper;
    }

    public async Task<ProdutoDto> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        if (await _produtoRepository.ExistePorSkuAsync(request.SKU))
            throw new InvalidOperationException($"SKU '{request.SKU}' já existe.");

        Neostore.Domain.Entities.Categoria? categoria = await _categoriaRepository.ObterPorIdAsync(request.IdCategoria);
        if (categoria == null)
            throw new InvalidOperationException("Categoria não encontrada.");

        Domain.Entities.Produto produto = new()
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            SKU = request.SKU,
            Preço = request.Preço,
            IdCategoria = request.IdCategoria,
            Descrição = request.Descrição,
            Estoque = request.Estoque
        };

        foreach (IFormFile arquivo in request.Imagens)
        {
            ImagemUploadResultado resultado = await _s3Service.UploadAsync(
                arquivo,
                prefixo: "produtos",
                cancellationToken);

            produto.AdicionarImagem(new Imagem
            {
                Id = Guid.NewGuid(),
                NomeArquivo = resultado.NomeArquivo,
                ChaveS3 = resultado.ChaveS3,
                TipoConteudo = resultado.TipoConteudo,
                TamanhoBytes = resultado.TamanhoBytes,
                DataCriacao = DateTime.UtcNow
            });
        }

        await _produtoRepository.CriarAsync(produto);

        return _mapper.Map<Domain.Entities.Produto, ProdutoDto>(produto);
    }
}
