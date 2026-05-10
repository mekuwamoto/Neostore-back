using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Application.Handlers.Produto;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class AtualizarProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly AtualizarProdutoCommandHandler _handler;

    public AtualizarProdutoCommandHandlerTests()
    {
        _handler = new AtualizarProdutoCommandHandler(_produtoRepo.Object, _categoriaRepo.Object, _mapper);
    }

    private static Produto CriarProdutoExistente(Guid id, string sku = "NB001")
    {
        return new Produto
        {
            Id = id,
            Nome = "Produto Antigo",
            SKU = sku,
            Preço = 1000m,
            IdCategoria = Guid.NewGuid(),
            Descrição = "Desc antiga",
            Estoque = 5,
            Imagens = new List<Imagem>()
        };
    }

    [Fact]
    public async Task Handle_ComDadosValidos_RetornaProdutoDtoAtualizado()
    {
        var id = Guid.NewGuid();
        var idCategoria = Guid.NewGuid();
        var produtoExistente = CriarProdutoExistente(id, "NB001");
        var command = new AtualizarProdutoCommand(id, "Notebook Novo", "NB001", 4000m, idCategoria, "Nova desc", new List<ImagemInputDto>(), 20);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produtoExistente);
        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", id)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria });
        _produtoRepo.Setup(r => r.AtualizarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be("Notebook Novo");
        resultado.Preço.Should().Be(4000m);
        resultado.Estoque.Should().Be(20);
    }

    [Fact]
    public async Task Handle_ProdutoNaoEncontrado_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarProdutoCommand(id, "Nome", "SKU", 100m, Guid.NewGuid(), "Desc", new List<ImagemInputDto>(), 0);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync((Produto?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Produto não encontrado*");
    }

    [Fact]
    public async Task Handle_SkuJaExisteEmOutroProduto_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var produtoExistente = CriarProdutoExistente(id, "NB001");
        var command = new AtualizarProdutoCommand(id, "Nome", "NB002", 100m, Guid.NewGuid(), "Desc", new List<ImagemInputDto>(), 0);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produtoExistente);
        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB002", id)).ReturnsAsync(true);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NB002*");
    }

    [Fact]
    public async Task Handle_MesmoSku_NaoVerificaUnicidade()
    {
        var id = Guid.NewGuid();
        var idCategoria = Guid.NewGuid();
        var produtoExistente = CriarProdutoExistente(id, "NB001");
        var command = new AtualizarProdutoCommand(id, "Nome Atualizado", "NB001", 100m, idCategoria, "Desc", new List<ImagemInputDto>(), 0);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produtoExistente);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria });
        _produtoRepo.Setup(r => r.AtualizarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        _produtoRepo.Verify(r => r.ExistePorSkuAsync(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CategoriaInexistente_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var idCategoria = Guid.NewGuid();
        var produtoExistente = CriarProdutoExistente(id, "NB001");
        var command = new AtualizarProdutoCommand(id, "Nome", "NB001", 100m, idCategoria, "Desc", new List<ImagemInputDto>(), 0);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produtoExistente);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria não encontrada*");
    }

    [Fact]
    public async Task Handle_AtualizacaoComSucesso_ChamaAtualizarAsyncUmaVez()
    {
        var id = Guid.NewGuid();
        var idCategoria = Guid.NewGuid();
        var produtoExistente = CriarProdutoExistente(id, "NB001");
        var command = new AtualizarProdutoCommand(id, "Nome", "NB001", 100m, idCategoria, "Desc", new List<ImagemInputDto>(), 0);

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produtoExistente);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria });
        _produtoRepo.Setup(r => r.AtualizarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        await _handler.Handle(command, CancellationToken.None);

        _produtoRepo.Verify(r => r.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
    }
}
