using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class ObterTodasCategoriasQueryHandler : IRequestHandler<ObterTodasCategoriasQuery, List<CategoriaDto>>
{
    private readonly ICategoriaRepository _repository;
    private readonly IMapper _mapper;

    public ObterTodasCategoriasQueryHandler(ICategoriaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CategoriaDto>> Handle(ObterTodasCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _repository.ObterArvoreAsync();

        return _mapper.Map<List<Domain.Entities.Categoria>, List<CategoriaDto>>(categorias);
    }
}
