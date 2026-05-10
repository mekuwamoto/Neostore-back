using AwesomeAssertions;
using Moq;
using Neostore.Application.Handlers.Produto;
using Neostore.Application.Queries.Produto;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class ObterProdutosPaginadoQueryHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly ObterProdutosPaginadoQueryHandler _handler;

    public ObterProdutosPaginadoQueryHandlerTests()
    {
        _handler = new ObterProdutosPaginadoQueryHandler(_produtoRepo.Object);
    }

    private static Produto CriarProduto(string nome, string sku) => new()
    {
        Id = Guid.NewGuid(),
        Nome = nome,
        SKU = sku,
        Preço = 100m,
        IdCategoria = Guid.NewGuid(),
        Descrição = "Desc",
        Estoque = 5,
        Imagens = new List<Imagem>()
    };

    [Fact]
    public async Task Handle_ListaComItens_RetornaProdutosPaginadoDto()
    {
        var produtos = new List<Produto>
        {
            CriarProduto("Notebook", "NB001"),
            CriarProduto("Mouse", "MS001")
        };
        var query = new ObterProdutosPaginadoQuery(1, 10);

        _produtoRepo.Setup(r => r.ObterPaginadoAsync(1, 10, null, null, null)).ReturnsAsync(produtos);
        _produtoRepo.Setup(r => r.ContarTotalAsync(null, null, null)).ReturnsAsync(2);

        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Dados.Should().HaveCount(2);
        resultado.Total.Should().Be(2);
        resultado.Pagina.Should().Be(1);
        resultado.Tamanho.Should().Be(10);
    }

    [Fact]
    public async Task Handle_SemItens_RetornaListaVazia()
    {
        var query = new ObterProdutosPaginadoQuery(1, 10);

        _produtoRepo.Setup(r => r.ObterPaginadoAsync(1, 10, null, null, null)).ReturnsAsync(new List<Produto>());
        _produtoRepo.Setup(r => r.ContarTotalAsync(null, null, null)).ReturnsAsync(0);

        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Dados.Should().BeEmpty();
        resultado.Total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ComFiltros_RepassaFiltrosAoRepositorio()
    {
        var idCategoria = Guid.NewGuid();
        var query = new ObterProdutosPaginadoQuery(2, 5, idCategoria, "Note", "NB");

        _produtoRepo.Setup(r => r.ObterPaginadoAsync(2, 5, idCategoria, "Note", "NB")).ReturnsAsync(new List<Produto>());
        _produtoRepo.Setup(r => r.ContarTotalAsync(idCategoria, "Note", "NB")).ReturnsAsync(0);

        await _handler.Handle(query, CancellationToken.None);

        _produtoRepo.Verify(r => r.ObterPaginadoAsync(2, 5, idCategoria, "Note", "NB"), Times.Once);
        _produtoRepo.Verify(r => r.ContarTotalAsync(idCategoria, "Note", "NB"), Times.Once);
    }

    [Fact]
    public async Task Handle_TotalPaginas_CalculadoCorretamente()
    {
        var query = new ObterProdutosPaginadoQuery(1, 5);

        _produtoRepo.Setup(r => r.ObterPaginadoAsync(1, 5, null, null, null)).ReturnsAsync(new List<Produto>());
        _produtoRepo.Setup(r => r.ContarTotalAsync(null, null, null)).ReturnsAsync(12);

        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.TotalPaginas.Should().Be(3);
    }
}
