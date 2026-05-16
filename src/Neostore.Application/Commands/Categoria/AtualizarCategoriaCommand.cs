using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.Categoria;

public record AtualizarCategoriaCommand(
    Guid Id,
    string Nome,
    Guid? IdCategoriaPai
) : IRequest<CategoriaDto>;

public class AtualizarCategoriaCommandHandler : IRequestHandler<AtualizarCategoriaCommand, CategoriaDto>
{
    private readonly ICategoriaRepository _repository;
    private readonly IMapper _mapper;

    public AtualizarCategoriaCommandHandler(ICategoriaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CategoriaDto> Handle(AtualizarCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.ObterPorIdAsync(request.Id);
        if (categoria == null)
            throw new InvalidOperationException("Categoria não encontrada.");

        if (categoria.Nome != request.Nome && await _repository.ExistePorNomeAsync(request.Nome, request.Id))
            throw new InvalidOperationException($"Categoria com nome '{request.Nome}' já existe.");

        if (request.IdCategoriaPai == request.Id)
            throw new InvalidOperationException("Categoria não pode ser pai de si mesma.");

        if (request.IdCategoriaPai.HasValue)
        {
            var categoriaPai = await _repository.ObterPorIdAsync(request.IdCategoriaPai.Value);
            if (categoriaPai == null)
                throw new InvalidOperationException("Categoria pai não encontrada.");
        }

        categoria.Nome = request.Nome;
        categoria.Slug = Domain.Entities.Categoria.GerarSlug(request.Nome);
        categoria.IdCategoriaPai = request.IdCategoriaPai;

        await _repository.AtualizarAsync(categoria);

        return _mapper.Map<Domain.Entities.Categoria, CategoriaDto>(categoria);
    }
}
