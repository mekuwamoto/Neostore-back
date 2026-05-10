using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class ObterCategoriaPorIdQueryHandler : IRequestHandler<ObterCategoriaPorIdQuery, CategoriaDto?>
{
    private readonly ICategoriaRepository _repository;

    public ObterCategoriaPorIdQueryHandler(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoriaDto?> Handle(ObterCategoriaPorIdQuery request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.ObterPorIdAsync(request.Id);
        if (categoria == null)
            return null;

        return new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Slug = categoria.Slug,
            IdCategoriaPai = categoria.IdCategoriaPai
        };
    }
}
