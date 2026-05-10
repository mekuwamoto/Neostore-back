using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.Categoria;

public record AtualizarCategoriaCommand(
    Guid Id,
    string Nome,
    Guid? IdCategoriaPai
) : IRequest<CategoriaDto>;
