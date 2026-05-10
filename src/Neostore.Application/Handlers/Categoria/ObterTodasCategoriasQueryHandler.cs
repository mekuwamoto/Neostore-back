using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class ObterTodasCategoriasQueryHandler : IRequestHandler<ObterTodasCategoriasQuery, List<CategoriaDto>>
{
    private readonly ICategoriaRepository _repository;

    public ObterTodasCategoriasQueryHandler(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoriaDto>> Handle(ObterTodasCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _repository.ObterArvoreAsync();

        return categorias.Select(c => new CategoriaDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Slug = c.Slug,
            IdCategoriaPai = c.IdCategoriaPai
        }).ToList();
    }
}
