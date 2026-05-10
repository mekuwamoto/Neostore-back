using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Application.Handlers.Produto;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class CriarProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly CriarProdutoCommandHandler _handler;

    public CriarProdutoCommandHandlerTests()
    {
        _handler = new CriarProdutoCommandHandler(_produtoRepo.Object, _categoriaRepo.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_RetornaProdutoDto()
    {
        var idCategoria = Guid.NewGuid();
        var command = new CriarProdutoCommand("Notebook", "NB001", 3500m, idCategoria, "Desc", new List<ImagemInputDto>(), 10);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria, Nome = "Tech" });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Notebook");
        resultado.SKU.Should().Be("NB001");
        resultado.Preço.Should().Be(3500m);
        resultado.IdCategoria.Should().Be(idCategoria);
        resultado.Estoque.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ComSkuDuplicado_LancaInvalidOperationException()
    {
        var command = new CriarProdutoCommand("Notebook", "NB001", 3500m, Guid.NewGuid(), "Desc", new List<ImagemInputDto>(), 10);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(true);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NB001*");
    }

    [Fact]
    public async Task Handle_ComCategoriaInexistente_LancaInvalidOperationException()
    {
        var idCategoria = Guid.NewGuid();
        var command = new CriarProdutoCommand("Notebook", "NB001", 3500m, idCategoria, "Desc", new List<ImagemInputDto>(), 10);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria não encontrada*");
    }

    [Fact]
    public async Task Handle_ComImagens_MapeimagemCorretamente()
    {
        var idCategoria = Guid.NewGuid();
        var imagens = new List<ImagemInputDto>
        {
            new() { NomeArquivo = "foto.jpg", ChaveS3 = "produtos/foto.jpg", TipoConteudo = "image/jpeg", TamanhoBytes = 1024 }
        };
        var command = new CriarProdutoCommand("Notebook", "NB001", 3500m, idCategoria, "Desc", imagens, 10);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria, Nome = "Tech" });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Imagens.Should().HaveCount(1);
        resultado.Imagens[0].ChaveS3.Should().Be("produtos/foto.jpg");
        resultado.Imagens[0].NomeArquivo.Should().Be("foto.jpg");
    }

    [Fact]
    public async Task Handle_CriacaoComSucesso_ChamaCriarAsyncUmaVez()
    {
        var idCategoria = Guid.NewGuid();
        var command = new CriarProdutoCommand("Notebook", "NB001", 3500m, idCategoria, "Desc", new List<ImagemInputDto>(), 10);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        await _handler.Handle(command, CancellationToken.None);

        _produtoRepo.Verify(r => r.CriarAsync(It.IsAny<Produto>()), Times.Once);
    }
}
