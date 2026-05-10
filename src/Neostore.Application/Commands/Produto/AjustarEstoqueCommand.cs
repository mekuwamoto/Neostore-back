using MediatR;

namespace Neostore.Application.Commands.Produto;

public record AjustarEstoqueCommand(Guid IdProduto, int Quantidade) : IRequest<int>;
