using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Queries.Categoria;
using Neostore.Domain.Entities;
using Categoria = Neostore.Domain.Entities.Categoria;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.CategoriaHandlers;

public class ObterTodasCategoriasQueryHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly ObterTodasCategoriasQueryHandler _handler;

    public ObterTodasCategoriasQueryHandlerTests()
    {
        _handler = new ObterTodasCategoriasQueryHandler(_categoriaRepo.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ComCategorias_RetornaListaDeCategoriaDto()
    {
        var categorias = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), Nome = "Eletrônicos", Slug = "eletronicos", IdCategoriaPai = null },
            new() { Id = Guid.NewGuid(), Nome = "Informática", Slug = "informatica", IdCategoriaPai = null }
        };

        _categoriaRepo.Setup(r => r.ObterArvoreAsync()).ReturnsAsync(categorias);

        var resultado = await _handler.Handle(new ObterTodasCategoriasQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado[0].Nome.Should().Be("Eletrônicos");
        resultado[1].Nome.Should().Be("Informática");
    }

    [Fact]
    public async Task Handle_SemCategorias_RetornaListaVazia()
    {
        _categoriaRepo.Setup(r => r.ObterArvoreAsync()).ReturnsAsync(new List<Categoria>());

        var resultado = await _handler.Handle(new ObterTodasCategoriasQuery(), CancellationToken.None);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapeiaPropriedadesCorretamente()
    {
        var id = Guid.NewGuid();
        var idPai = Guid.NewGuid();
        var categorias = new List<Categoria>
        {
            new() { Id = id, Nome = "Notebooks", Slug = "notebooks", IdCategoriaPai = idPai }
        };

        _categoriaRepo.Setup(r => r.ObterArvoreAsync()).ReturnsAsync(categorias);

        var resultado = await _handler.Handle(new ObterTodasCategoriasQuery(), CancellationToken.None);

        resultado[0].Id.Should().Be(id);
        resultado[0].Slug.Should().Be("notebooks");
        resultado[0].IdCategoriaPai.Should().Be(idPai);
    }
}
