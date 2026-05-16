using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class DeletarProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly DeletarProdutoCommandHandler _handler;

    public DeletarProdutoCommandHandlerTests()
    {
        _handler = new DeletarProdutoCommandHandler(_produtoRepo.Object);
    }

    [Fact]
    public async Task Handle_ProdutoExistente_RetornaTrue()
    {
        var id = Guid.NewGuid();
        var command = new DeletarProdutoCommand(id);

        _produtoRepo.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ProdutoNaoEncontrado_RetornaFalse()
    {
        var id = Guid.NewGuid();
        var command = new DeletarProdutoCommand(id);

        _produtoRepo.Setup(r => r.DeletarAsync(id)).ReturnsAsync(false);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CharnaDeletarAsyncComIdCorreto()
    {
        var id = Guid.NewGuid();
        var command = new DeletarProdutoCommand(id);

        _produtoRepo.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _produtoRepo.Verify(r => r.DeletarAsync(id), Times.Once);
    }
}
