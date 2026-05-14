using AutoMapper;
using AwesomeAssertions;
using Moq;
using Neostore.Application.Commands.Categoria;
using Neostore.Domain.Entities;
using Categoria = Neostore.Domain.Entities.Categoria;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.CategoriaHandlers;

public class AtualizarCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly AtualizarCategoriaCommandHandler _handler;

    public AtualizarCategoriaCommandHandlerTests()
    {
        _handler = new AtualizarCategoriaCommandHandler(_categoriaRepo.Object, _mapper);
    }

    private static Categoria CriarCategoria(Guid id, string nome = "Eletrônicos") => new()
    {
        Id = id,
        Nome = nome,
        Slug = Categoria.GerarSlug(nome),
        IdCategoriaPai = null
    };

    [Fact]
    public async Task Handle_ComDadosValidos_RetornaCategoriaAtualizada()
    {
        var id = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id, "Eletrônicos");
        var command = new AtualizarCategoriaCommand(id, "Tecnologia", null);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Tecnologia", id)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.AtualizarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Nome.Should().Be("Tecnologia");
        resultado.Slug.Should().Be("tecnologia");
    }

    [Fact]
    public async Task Handle_CategoriaNaoEncontrada_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarCategoriaCommand(id, "Novo Nome", null);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria não encontrada*");
    }

    [Fact]
    public async Task Handle_NomeJaExisteEmOutraCategoria_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id, "Eletrônicos");
        var command = new AtualizarCategoriaCommand(id, "Informática", null);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Informática", id)).ReturnsAsync(true);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Informática*");
    }

    [Fact]
    public async Task Handle_AutoReferencia_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id);
        var command = new AtualizarCategoriaCommand(id, "Eletrônicos", id);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.ExistePorNomeAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pai de si mesma*");
    }

    [Fact]
    public async Task Handle_CategoriaPaiNaoEncontrada_LancaInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var idPai = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id);
        var command = new AtualizarCategoriaCommand(id, "Eletrônicos", idPai);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Eletrônicos", id)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idPai)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria pai não encontrada*");
    }

    [Fact]
    public async Task Handle_MesmoNome_NaoVerificaUnicidade()
    {
        var id = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id, "Eletrônicos");
        var command = new AtualizarCategoriaCommand(id, "Eletrônicos", null);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.AtualizarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        _categoriaRepo.Verify(r => r.ExistePorNomeAsync(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AtualizacaoComSucesso_ChamaAtualizarAsyncUmaVez()
    {
        var id = Guid.NewGuid();
        var categoriaExistente = CriarCategoria(id, "Eletrônicos");
        var command = new AtualizarCategoriaCommand(id, "Tecnologia", null);

        _categoriaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(categoriaExistente);
        _categoriaRepo.Setup(r => r.ExistePorNomeAsync("Tecnologia", id)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.AtualizarAsync(It.IsAny<Categoria>())).ReturnsAsync((Categoria c) => c);

        await _handler.Handle(command, CancellationToken.None);

        _categoriaRepo.Verify(r => r.AtualizarAsync(It.IsAny<Categoria>()), Times.Once);
    }
}
