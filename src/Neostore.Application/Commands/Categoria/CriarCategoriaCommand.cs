using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.Categoria;

public record CriarCategoriaCommand(
    string Nome,
    Guid? IdCategoriaPai
) : IRequest<CategoriaDto>;

public class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, CategoriaDto>
{
    private readonly ICategoriaRepository _repository;
    private readonly IMapper _mapper;

    public CriarCategoriaCommandHandler(ICategoriaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CategoriaDto> Handle(CriarCategoriaCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistePorNomeAsync(request.Nome))
            throw new InvalidOperationException($"Categoria com nome '{request.Nome}' já existe.");

        if (request.IdCategoriaPai.HasValue)
        {
            var categoriaPai = await _repository.ObterPorIdAsync(request.IdCategoriaPai.Value);
            if (categoriaPai == null)
                throw new InvalidOperationException("Categoria pai não encontrada.");
        }

        var categoria = new Domain.Entities.Categoria
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Slug = Domain.Entities.Categoria.GerarSlug(request.Nome),
            IdCategoriaPai = request.IdCategoriaPai
        };

        await _repository.CriarAsync(categoria);

        return _mapper.Map<Domain.Entities.Categoria, CategoriaDto>(categoria);
    }
}
