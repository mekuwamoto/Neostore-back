using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Application.DTOs;
using Neostore.Application.Handlers.UsuarioAdmin;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;
using UsuarioAdminEntidade = Neostore.Domain.Entities.UsuarioAdmin;

namespace Neostore.Tests.Application.Handlers.UsuarioAdmin;

public class AtualizarUsuarioAdminCommandHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly AtualizarUsuarioAdminCommandHandler _handler;

    public AtualizarUsuarioAdminCommandHandlerTests()
    {
        _handler = new AtualizarUsuarioAdminCommandHandler(_repository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_AtualizaERetornaDto()
    {
        Guid id = Guid.NewGuid();
        UsuarioAdminEntidade usuario = new() { Id = id, Email = "antigo@neostore.com", Role = "Gerente" };
        AtualizarUsuarioAdminCommand command = new(id, "novo@neostore.com", "Admin");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repository.Setup(r => r.ObterPorEmailAsync("novo@neostore.com")).ReturnsAsync((UsuarioAdminEntidade?)null);
        _repository.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        UsuarioAdminDto resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Email.Should().Be("novo@neostore.com");
        resultado.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_LancaInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        AtualizarUsuarioAdminCommand command = new(id, "novo@neostore.com", "Admin");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((UsuarioAdminEntidade?)null);

        Func<Task> acao = () => _handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task Handle_EmailEmUsoOutroUsuario_LancaInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        UsuarioAdminEntidade usuario = new() { Id = id, Email = "antigo@neostore.com", Role = "Admin" };
        UsuarioAdminEntidade outro = new() { Id = Guid.NewGuid(), Email = "outro@neostore.com", Role = "Gerente" };
        AtualizarUsuarioAdminCommand command = new(id, "outro@neostore.com", "Admin");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repository.Setup(r => r.ObterPorEmailAsync("outro@neostore.com")).ReturnsAsync(outro);

        Func<Task> acao = () => _handler.Handle(command, CancellationToken.None);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*outro@neostore.com*");
    }

    [Fact]
    public async Task Handle_MesmoEmail_NaoVerificaDuplicidade()
    {
        Guid id = Guid.NewGuid();
        UsuarioAdminEntidade usuario = new() { Id = id, Email = "admin@neostore.com", Role = "Gerente" };
        AtualizarUsuarioAdminCommand command = new(id, "admin@neostore.com", "Admin");

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
        _repository.Setup(r => r.AtualizarAsync(It.IsAny<UsuarioAdminEntidade>())).ReturnsAsync((UsuarioAdminEntidade u) => u);

        await _handler.Handle(command, CancellationToken.None);

        _repository.Verify(r => r.ObterPorEmailAsync(It.IsAny<string>()), Times.Never);
    }
}
