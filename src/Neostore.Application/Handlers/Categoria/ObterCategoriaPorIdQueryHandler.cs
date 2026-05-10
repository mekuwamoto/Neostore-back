using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class ObterCategoriaPorIdQueryHandler : IRequestHandler<ObterCategoriaPorIdQuery, CategoriaDto?>
{
    private readonly ICategoriaRepository _repository;
    private readonly IMapper _mapper;

    public ObterCategoriaPorIdQueryHandler(ICategoriaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CategoriaDto?> Handle(ObterCategoriaPorIdQuery request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.ObterPorIdAsync(request.Id);
        if (categoria == null)
            return null;

        return _mapper.Map<Domain.Entities.Categoria, CategoriaDto>(categoria);
    }
}
