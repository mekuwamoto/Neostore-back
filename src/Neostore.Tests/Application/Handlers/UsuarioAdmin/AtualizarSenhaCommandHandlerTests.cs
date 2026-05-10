using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Application.Handlers.UsuarioAdmin;
using Neostore.Persistence.Repositories;
using UsuarioAdminEntidade = Neostore.Domain.Entities.UsuarioAdmin;

namespace Neostore.Tests.Application.Handlers.UsuarioAdmin;

public class AtualizarSenhaCommandHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly AtualizarSenhaCommandHandler _handler;

    public AtualizarSenhaCommandHandlerTests()
    {
        _handler = new AtualizarSenhaCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_SenhaAtualCorreta_AtualizaERetornaTrue()
    {
        Guid id = Guid.NewGuid();
        string hashAtual = BCrypt.Net.BCrypt.HashPassword("SenhaAntiga123!");
        UsuarioAdminEntidade usuario = new() { Id = id, Email = "admin@neostore.com", SenhaHash = hashAtual, Role = "Admin" };
        AtualizarSenhaCommand command = new(id, "SenhaAntiga123!", "NovaSenha123!");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repository.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        bool resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("NovaSenha123!", usuario.SenhaHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SenhaAtualIncorreta_LancaInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        string hashAtual = BCrypt.Net.BCrypt.HashPassword("SenhaCorreta123!");
        UsuarioAdminEntidade usuario = new() { Id = id, SenhaHash = hashAtual, Role = "Admin" };
        AtualizarSenhaCommand command = new(id, "SenhaErrada123!", "NovaSenha123!");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);

        Func<Task> acao = () => _handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Senha atual incorreta*");
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_LancaInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        AtualizarSenhaCommand command = new(id, "SenhaAntiga123!", "NovaSenha123!");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((UsuarioAdminEntidade?)null);

        Func<Task> acao = () => _handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task Handle_SenhaAtualCorreta_NovaSenhaArmazenadaComoHash()
    {
        Guid id = Guid.NewGuid();
        string hashAtual = BCrypt.Net.BCrypt.HashPassword("SenhaAntiga123!");
        UsuarioAdminEntidade usuario = new() { Id = id, SenhaHash = hashAtual, Role = "Admin" };
        AtualizarSenhaCommand command = new(id, "SenhaAntiga123!", "NovaSenha123!");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repository.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        await _handler.Handle(command, CancellationToken.None);

        usuario.SenhaHash.Should().NotBe("NovaSenha123!");
        BCrypt.Net.BCrypt.Verify("NovaSenha123!", usuario.SenhaHash).Should().BeTrue();
    }
}
