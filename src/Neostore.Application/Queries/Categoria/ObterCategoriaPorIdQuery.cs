using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.Categoria;

public record ObterCategoriaPorIdQuery(Guid Id) : IRequest<CategoriaDto?>;
