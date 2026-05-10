using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.Produto;

public record ObterProdutoPorIdQuery(Guid Id) : IRequest<ProdutoDto?>;
