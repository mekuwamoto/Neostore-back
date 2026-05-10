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

public class ObterUsuarioAdminPorIdQueryHandlerTests
{
    private readonly Mock<IUsuarioAdminRepository> _repository = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly ObterUsuarioAdminPorIdQueryHandler _handler;

    public ObterUsuarioAdminPorIdQueryHandlerTests()
    {
        _handler = new ObterUsuarioAdminPorIdQueryHandler(_repository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_UsuarioExistente_RetornaDto()
    {
        Guid id = Guid.NewGuid();
        UsuarioAdminEntidade usuario = new() { Id = id, Email = "admin@neostore.com", Role = "Admin" };

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);

        UsuarioAdminDto? resultado = await _handler.Handle(new ObterUsuarioAdminPorIdQuery(id), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Email.Should().Be("admin@neostore.com");
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_RetornaNull()
    {
        Guid id = Guid.NewGuid();

        _repository.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((UsuarioAdminEntidade?)null);

        UsuarioAdminDto? resultado = await _handler.Handle(new ObterUsuarioAdminPorIdQuery(id), CancellationToken.None);

        resultado.Should().BeNull();
    }
}
