using AutoMapper;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Application.Interfaces;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;
using Neostore.Tests.Factories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class CriarProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
    private readonly Mock<IS3Service> _s3Service = new();
    private readonly IMapper _mapper = AutoMapperFactory.Create();
    private readonly CriarProdutoCommandHandler _handler;

    public CriarProdutoCommandHandlerTests()
    {
        _handler = new CriarProdutoCommandHandler(
            _produtoRepo.Object,
            _categoriaRepo.Object,
            _s3Service.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_RetornaProdutoDto()
    {
        Guid idCategoria = Guid.NewGuid();
        CriarProdutoCommand command = new("Notebook", "NB001", 3500m, idCategoria, "Desc", 10, new List<IFormFile>());

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria, Nome = "Tech" });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        ProdutoDto resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Notebook");
        resultado.SKU.Should().Be("NB001");
        resultado.Preço.Should().Be(3500m);
        resultado.IdCategoria.Should().Be(idCategoria);
        resultado.Estoque.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ComSkuDuplicado_LancaInvalidOperationException()
    {
        CriarProdutoCommand command = new("Notebook", "NB001", 3500m, Guid.NewGuid(), "Desc", 10, new List<IFormFile>());

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(true);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NB001*");
    }

    [Fact]
    public async Task Handle_ComCategoriaInexistente_LancaInvalidOperationException()
    {
        Guid idCategoria = Guid.NewGuid();
        CriarProdutoCommand command = new("Notebook", "NB001", 3500m, idCategoria, "Desc", 10, new List<IFormFile>());

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync((Categoria?)null);

        Func<Task> action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Categoria não encontrada*");
    }

    [Fact]
    public async Task Handle_ComImagens_FazUploadEMapeimagemCorretamente()
    {
        Guid idCategoria = Guid.NewGuid();

        Mock<IFormFile> arquivoMock = new();
        arquivoMock.Setup(f => f.FileName).Returns("foto.jpg");
        arquivoMock.Setup(f => f.ContentType).Returns("image/jpeg");
        arquivoMock.Setup(f => f.Length).Returns(1024);

        List<IFormFile> imagens = [arquivoMock.Object];
        CriarProdutoCommand command = new("Notebook", "NB001", 3500m, idCategoria, "Desc", 10, imagens);

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria, Nome = "Tech" });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);
        _s3Service
            .Setup(s => s.UploadAsync(It.IsAny<IFormFile>(), "produtos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImagemUploadResultado("produtos/foto.jpg", "foto.jpg", "image/jpeg", 1024));

        ProdutoDto resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Imagens.Should().HaveCount(1);
        resultado.Imagens[0].ChaveS3.Should().Be("produtos/foto.jpg");
        resultado.Imagens[0].NomeArquivo.Should().Be("foto.jpg");
    }

    [Fact]
    public async Task Handle_CriacaoComSucesso_ChamaCriarAsyncUmaVez()
    {
        Guid idCategoria = Guid.NewGuid();
        CriarProdutoCommand command = new("Notebook", "NB001", 3500m, idCategoria, "Desc", 10, new List<IFormFile>());

        _produtoRepo.Setup(r => r.ExistePorSkuAsync("NB001", null)).ReturnsAsync(false);
        _categoriaRepo.Setup(r => r.ObterPorIdAsync(idCategoria)).ReturnsAsync(new Categoria { Id = idCategoria });
        _produtoRepo.Setup(r => r.CriarAsync(It.IsAny<Produto>())).ReturnsAsync((Produto p) => p);

        await _handler.Handle(command, CancellationToken.None);

        _produtoRepo.Verify(r => r.CriarAsync(It.IsAny<Produto>()), Times.Once);
    }
}
