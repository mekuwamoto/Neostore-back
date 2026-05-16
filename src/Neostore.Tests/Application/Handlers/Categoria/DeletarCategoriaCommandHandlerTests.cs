using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Categoria;
using Neostore.Domain.Entities;
using Categoria = Neostore.Domain.Entities.Categoria;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.CategoriaHandlers;

public class DeletarCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly DeletarCategoriaCommandHandler _handler;

    public DeletarCategoriaCommandHandlerTests()
    {
        _handler = new DeletarCategoriaCommandHandler(_categoriaRepo.Object);
    }

    [Fact]
    public async Task Handle_CategoriaSemDependencias_RetornaTrue()
    {
        var id = Guid.NewGuid();
        var categoria = new Categoria { Id = id, Nome = "Vazia" };
        var command = new DeletarCategoriaCommand(id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);
        _categoriaRepo.Setup(r => r.ContarSubcategoriasAsync(id)).ReturnsAsync(0);
        _categoriaRepo.Setup(r => r.ContarProdutosAsync(id)).ReturnsAsync(0);
        _categoriaRepo.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CategoriaNaoEncontrada_RetornaFalse()
    {
        var id = Guid.NewGuid();
        var command = new DeletarCategoriaCommand(id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Categoria?)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CategoriaComSubcategorias_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var categoria = new Categoria { Id = id, Nome = "Com Filhos" };
        var command = new DeletarCategoriaCommand(id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);
        _categoriaRepo.Setup(r => r.ContarSubcategoriasAsync(id)).ReturnsAsync(2);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*subcategorias*");
    }

    [Fact]
    public async Task Handle_CategoriaComProdutos_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var categoria = new Categoria { Id = id, Nome = "Com Produtos" };
        var command = new DeletarCategoriaCommand(id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);
        _categoriaRepo.Setup(r => r.ContarSubcategoriasAsync(id)).ReturnsAsync(0);
        _categoriaRepo.Setup(r => r.ContarProdutosAsync(id)).ReturnsAsync(5);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*produtos*");
    }

    [Fact]
    public async Task Handle_DelecaoComSucesso_ChamnaDeletarAsyncUmaVez()
    {
        var id = Guid.NewGuid();
        var categoria = new Categoria { Id = id, Nome = "Vazia" };
        var command = new DeletarCategoriaCommand(id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoria);
        _categoriaRepo.Setup(r => r.ContarSubcategoriasAsync(id)).ReturnsAsync(0);
        _categoriaRepo.Setup(r => r.ContarProdutosAsync(id)).ReturnsAsync(0);
        _categoriaRepo.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _categoriaRepo.Verify(r => r.DeletarAsync(id), Times.Once);
    }
}
