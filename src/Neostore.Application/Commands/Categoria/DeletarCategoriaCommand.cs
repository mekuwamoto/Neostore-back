using MediatR;

namespace Neostore.Application.Commands.Categoria;

public record DeletarCategoriaCommand(Guid Id) : IRequest<bool>;
