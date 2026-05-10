using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.Categoria;

public record ObterTodasCategoriasQuery : IRequest<List<CategoriaDto>>;
