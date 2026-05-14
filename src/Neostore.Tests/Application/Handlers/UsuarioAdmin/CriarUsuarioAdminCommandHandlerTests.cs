using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;
using UsuarioAdminEntidade = Neostore.Domain.Entities.UsuarioAdmin;

namespace Neostore.Tests.Application.Handlers.UsuarioAdmin;

public class CriarUsuarioAdminCommandHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly CriarUsuarioAdminCommandHandler _handler;

    public CriarUsuarioAdminCommandHandlerTests()
    {
        _handler = new CriarUsuarioAdminCommandHandler(_repository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_CriaERetornaDto()
    {
        CriarUsuarioAdminCommand command = new("admin@neostore.com", "Senha123!", "Admin");

        _repository.Setup(r => r.ObterPorEmailAsync(command.Email)).ReturnsAsync((UsuarioAdminEntidade?)null);
        _repository.Setup(r => r.CriarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        UsuarioAdminDto resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Email.Should().Be("admin@neostore.com");
        resultado.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_EmailJaExiste_LancaInvalidOperationException()
    {
        CriarUsuarioAdminCommand command = new("admin@neostore.com", "Senha123!", "Admin");
        UsuarioAdminEntidade existente = new() { Id = Guid.NewGuid(), Email = command.Email, Role = "Admin" };

        _repository.Setup(r => r.ObterPorEmailAsync(command.Email)).ReturnsAsync(existente);

        Func<Task> acao = () => _handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*admin@neostore.com*");
    }

    [Fact]
    public async Task Handle_ComDadosValidos_NaoExpoeSenhaHash()
    {
        CriarUsuarioAdminCommand command = new("admin@neostore.com", "Senha123!", "Admin");

        _repository.Setup(r => r.ObterPorEmailAsync(command.Email)).ReturnsAsync((UsuarioAdminEntidade?)null);
        _repository.Setup(r => r.CriarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        UsuarioAdminDto resultado = await _handler.Handle(command, CancellationToken.None);

        typeof(UsuarioAdminDto).GetProperty("SenhaHash").Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComDadosValidos_ChamaCriarAsyncUmaVez()
    {
        CriarUsuarioAdminCommand command = new("admin@neostore.com", "Senha123!", "Admin");

        _repository.Setup(r => r.ObterPorEmailAsync(command.Email)).ReturnsAsync((UsuarioAdminEntidade?)null);
        _repository.Setup(r => r.CriarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        await _handler.Handle(command, CancellationToken.None);

        _repository.Verify(r => r.CriarAsync(It.IsAny<UsuarioAdminEntidade>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_SenhaArmazenadaComoHash()
    {
        CriarUsuarioAdminCommand command = new("admin@neostore.com", "Senha123!", "Admin");
        UsuarioAdminEntidade? capturado = null;

        _repository.Setup(r => r.ObterPorEmailAsync(command.Email)).ReturnsAsync((UsuarioAdminEntidade?)null);
        _repository.Setup(r => r.CriarAsync(It.IsAny<UsuarioAdminEntidade>()))
            .Callback<UsuarioAdminEntidade>(u => capturado = u)
            .ReturnsAsync((UsuarioAdminEntidade u) => u);

        await _handler.Handle(command, CancellationToken.None);

        capturado!.SenhaHash.Should().NotBe("Senha123!");
        BCrypt.Net.BCrypt.Verify("Senha123!", capturado.SenhaHash).Should().BeTrue();
    }
}
