using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.Categoria;

public record CriarCategoriaCommand(
    string Nome,
    Guid? IdCategoriaPai
) : IRequest<CategoriaDto>;
