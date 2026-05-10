using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Handlers.Categoria;
using Neostore.Application.Queries.Categoria;
using Neostore.Domain.Entities;
using Categoria = Neostore.Domain.Entities.Categoria;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.CategoriaHandlers;

public class ObterCategoriaPorIdQueryHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly ObterCategoriaPorIdQueryHandler _handler;

    public ObterCategoriaPorIdQueryHandlerTests()
    {
        _handler = new ObterCategoriaPorIdQueryHandler(_categoriaRepo.Object, _mapper);
    }

    [Fact]
    public async Task Handle_CategoriaEncontrada_RetornaCategoriaDto()
    {
        var id = Guid.NewGuid();
        var idPai = Guid.NewGuid();
        var categoria = new Categoria
        {
            Id = id,
            Nome = "Eletrônicos",
            Slug = "eletronicos",
            IdCategoriaPai = idPai
        };

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);

        var resultado = await _handler.Handle(new ObterCategoriaPorIdQuery(id), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Nome.Should().Be("Eletrônicos");
        resultado.Slug.Should().Be("eletronicos");
        resultado.IdCategoriaPai.Should().Be(idPai);
    }

    [Fact]
    public async Task Handle_CategoriaNaoEncontrada_RetornaNull()
    {
        var id = Guid.NewGuid();

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Categoria?)null);

        var resultado = await _handler.Handle(new ObterCategoriaPorIdQuery(id), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CategoriaSemPai_IdCategoriaPaiNulo()
    {
        var id = Guid.NewGuid();
        var categoria = new Categoria
        {
            Id = id,
            Nome = "Raiz",
            Slug = "raiz",
            IdCategoriaPai = null
        };

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);

        var resultado = await _handler.Handle(new ObterCategoriaPorIdQuery(id), CancellationToken.None);

        resultado!.IdCategoriaPai.Should().BeNull();
    }
}
