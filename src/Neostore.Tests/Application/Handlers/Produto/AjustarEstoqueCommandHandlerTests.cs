using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Produto;
using Neostore.Application.Handlers.Produto;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class AjustarEstoqueCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly AjustarEstoqueCommandHandler _handler;

    public AjustarEstoqueCommandHandlerTests()
    {
        _handler = new AjustarEstoqueCommandHandler(_produtoRepo.Object);
    }

    [Fact]
    public async Task Handle_Incremento_RetornaNovoEstoque()
    {
        var id = Guid.NewGuid();
        var produto = new Produto { Id = id, Estoque = 10 };
        var command = new AjustarEstoqueCommand(id, 5);

        _produtoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
        _produtoRepo.Setup(r => r.AtualizarAsync(produto)).ReturnsAsync(produto);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().Be(15);
    }

    [Fact]
    public async Task Handle_Decremento_RetornaNovoEstoque()
    {
        var id = Guid.NewGuid();
        var produto = new Produto { Id = id, Estoque = 10 };
        var command = new AjustarEstoqueCommand(id, -3);

        _produtoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
        _produtoRepo.Setup(r => r.AtualizarAsync(produto)).ReturnsAsync(produto);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().Be(7);
    }

    [Fact]
    public async Task Handle_ProdutoNaoEncontrado_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var command = new AjustarEstoqueCommand(id, 5);

        _produtoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Produto?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Produto não encontrado*");
    }

    [Fact]
    public async Task Handle_DecrementoAlemDoEstoque_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var produto = new Produto { Id = id, Estoque = 5 };
        var command = new AjustarEstoqueCommand(id, -10);

        _produtoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Estoque não pode ser negativo*");
    }

    [Fact]
    public async Task Handle_AjusteComSucesso_ChamaAtualizarAsync()
    {
        var id = Guid.NewGuid();
        var produto = new Produto { Id = id, Estoque = 10 };
        var command = new AjustarEstoqueCommand(id, 5);

        _produtoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
        _produtoRepo.Setup(r => r.AtualizarAsync(produto)).ReturnsAsync(produto);

        await _handler.Handle(command, CancellationToken.None);

        _produtoRepo.Verify(r => r.AtualizarAsync(produto), Times.Once);
    }
}
