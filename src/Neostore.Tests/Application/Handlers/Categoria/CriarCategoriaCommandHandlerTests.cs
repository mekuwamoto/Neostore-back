using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Categoria;
using Neostore.Domain.Entities;
using Categoria = Neostore.Domain.Entities.Categoria;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.CategoriaHandlers;

public class CriarCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly CriarCategoriaCommandHandler _handler;

    public CriarCategoriaCommandHandlerTests()
    {
        _handler = new CriarCategoriaCommandHandler(_categoriaRepo.Object, _mapper);
    }

    [Fact]
    public async Task Handle_SemCategoriaPai_CriaERetornaCategoriaDto()
    {
        var command = new CriarCategoriaCommand("Eletrônicos", null);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Eletrônicos", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.CriarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Eletrônicos");
        resultado.Slug.Should().Be("eletronicos");
        resultado.IdCategoriaPai.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComCategoriaPaiValida_CriaCorretamente()
    {
        var idPai = Guid.NewGuid();
        var command = new CriarCategoriaCommand("Notebooks", idPai);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Notebooks", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idPai)).ReturnsAsync(new Categoria { Id = idPai, Nome = "Eletrônicos" });
        _categoriaRepo.Setup(r => r.CriarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IdCategoriaPai.Should().Be(idPai);
    }

    [Fact]
    public async Task Handle_NomeJaExiste_LancaInvalidOperationException()
    {
        var command = new CriarCategoriaCommand("Eletrônicos", null);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Eletrônicos", null)).ReturnsAsync(true);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Eletrônicos*");
    }

    [Fact]
    public async Task Handle_CategoriaPaiNaoEncontrada_LancaInvalidOperationException()
    {
        var idPai = Guid.NewGuid();
        var command = new CriarCategoriaCommand("Notebooks", idPai);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Notebooks", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idPai)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria pai não encontrada*");
    }

    [Fact]
    public async Task Handle_CriacaoComSucesso_ChamaCriarAsyncUmaVez()
    {
        var command = new CriarCategoriaCommand("Eletrônicos", null);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Eletrônicos", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.CriarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        await _handler.Handle(command, CancellationToken.None);

        _categoriaRepo.Verify(r => r.CriarAsync(It.IsAny<Categoria>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GeraSlugAutomatico_RemoveAcentos()
    {
        var command = new CriarCategoriaCommand("Câmeras Fotográficas", null);

        _categoriaRepo.Setup(r => r.ExistePorNomeAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.CriarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Slug.Should().Be("cameras-fotograficas");
    }
}
