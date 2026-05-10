using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.DTOs;
using Neostore.Application.Handlers.UsuarioAdmin;
using Neostore.Application.Queries.UsuarioAdmin;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;
using UsuarioAdminEntidade = Neostore.Domain.Entities.UsuarioAdmin;

namespace Neostore.Tests.Application.Handlers.UsuarioAdmin;

public class ObterTodosUsuariosAdminQueryHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly ObterTodosUsuariosAdminQueryHandler _handler;

    public ObterTodosUsuariosAdminQueryHandlerTests()
    {
        _handler = new ObterTodosUsuariosAdminQueryHandler(_repository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ComUsuariosExistentes_RetornaListaDtos()
    {
        List<UsuarioAdminEntidade> usuarios =
        [
            new() { Id = Guid.NewGuid(), Email = "admin@neostore.com", Role = "Admin" },
            new() { Id = Guid.NewGuid(), Email = "gerente@neostore.com", Role = "Gerente" }
        ];

        _repository.Setup(r => r.ObterTodosAsync()).ReturnsAsync(usuarios);

        List<UsuarioAdminDto> resultado = await _handler.Handle(new ObterTodosUsuariosAdminQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado[0].Email.Should().Be("admin@neostore.com");
        resultado[1].Email.Should().Be("gerente@neostore.com");
    }

    [Fact]
    public async Task Handle_SemUsuarios_RetornaListaVazia()
    {
        _repository.Setup(r => r.ObterTodosAsync()).ReturnsAsync([]);

        List<UsuarioAdminDto> resultado = await _handler.Handle(new ObterTodosUsuariosAdminQuery(), CancellationToken.None);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NaoExpoeSenhaHash()
    {
        List<UsuarioAdminEntidade> usuarios =
        [
            new() { Id = Guid.NewGuid(), Email = "admin@neostore.com", SenhaHash = "hash_secreto", Role = "Admin" }
        ];

        _repository.Setup(r => r.ObterTodosAsync()).ReturnsAsync(usuarios);

        List<UsuarioAdminDto> resultado = await _handler.Handle(new ObterTodosUsuariosAdminQuery(), CancellationToken.None);

        typeof(UsuarioAdminDto).GetProperty("SenhaHash").Should().BeNull();
    }
}
