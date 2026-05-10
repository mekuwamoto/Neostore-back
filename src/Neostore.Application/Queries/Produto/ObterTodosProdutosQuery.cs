using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.Produto;

public record ObterTodosProdutosQuery : IRequest<List<ProdutoDto>>;
