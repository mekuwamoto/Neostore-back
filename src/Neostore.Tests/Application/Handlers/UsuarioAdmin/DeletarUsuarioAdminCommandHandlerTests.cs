using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Application.Handlers.UsuarioAdmin;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.Handlers.UsuarioAdmin;

public class DeletarUsuarioAdminCommandHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly DeletarUsuarioAdminCommandHandler _handler;

    public DeletarUsuarioAdminCommandHandlerTests()
    {
        _handler = new DeletarUsuarioAdminCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_RetornaTrue()
    {
        Guid id = Guid.NewGuid();
        DeletarUsuarioAdminCommand command = new(id);

        _repository.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        bool resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_RetornaFalse()
    {
        Guid id = Guid.NewGuid();
        DeletarUsuarioAdminCommand command = new(id);

        _repository.Setup(r => r.DeletarAsync(id)).ReturnsAsync(false);

        bool resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Deletar_ChamaDeletarAsyncUmaVez()
    {
        Guid id = Guid.NewGuid();
        DeletarUsuarioAdminCommand command = new(id);

        _repository.Setup(r => r.DeletarAsync(id)).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repository.Verify(r => r.DeletarAsync(id), Times.Once);
    }
}
