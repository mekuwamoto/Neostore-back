using MediatR;
using Neostore.Application.Commands.Categoria;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Categoria;

public class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, CategoriaDto>
{
    private readonly ICategoriaRepository _repository;

    public CriarCategoriaCommandHandler(ICategoriaRepository repository)
    {
        _repository = repository;
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

        return new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Slug = categoria.Slug,
            IdCategoriaPai = categoria.IdCategoriaPai
        };
    }
}
