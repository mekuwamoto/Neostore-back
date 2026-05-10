using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.Produto;

public record AtualizarProdutoCommand(
    Guid Id,
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    List<ImagemInputDto> Imagens,
    int Estoque
) : IRequest<ProdutoDto>;
