using MediatR;

namespace Neostore.Application.Commands.Produto;

public record DeletarProdutoCommand(Guid Id) : IRequest<bool>;
