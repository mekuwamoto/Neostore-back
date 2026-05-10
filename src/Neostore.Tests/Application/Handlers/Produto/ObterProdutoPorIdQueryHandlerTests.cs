using AwesomeAssertions;
using Moq;
using Neostore.Application.Handlers.Produto;
using Neostore.Application.Queries.Produto;
using Neostore.Domain.Entities;
using Produto = Neostore.Domain.Entities.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Tests.Application.ProdutoHandlers;

public class ObterProdutoPorIdQueryHandlerTests
{
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly ObterProdutoPorIdQueryHandler _handler;

    public ObterProdutoPorIdQueryHandlerTests()
    {
        _handler = new ObterProdutoPorIdQueryHandler(_produtoRepo.Object);
    }

    [Fact]
    public async Task Handle_ProdutoEncontrado_RetornaProdutoDto()
    {
        var id = Guid.NewGuid();
        var idCategoria = Guid.NewGuid();
        var produto = new Produto
        {
            Id = id,
            Nome = "Notebook",
            SKU = "NB001",
            Preço = 3500m,
            IdCategoria = idCategoria,
            Descrição = "Desc",
            Estoque = 10,
            Imagens = new List<Imagem>()
        };

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produto);

        var resultado = await _handler.Handle(new ObterProdutoPorIdQuery(id), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Nome.Should().Be("Notebook");
        resultado.SKU.Should().Be("NB001");
        resultado.Preço.Should().Be(3500m);
        resultado.IdCategoria.Should().Be(idCategoria);
    }

    [Fact]
    public async Task Handle_ProdutoNaoEncontrado_RetornaNull()
    {
        var id = Guid.NewGuid();

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync((Produto?)null);

        var resultado = await _handler.Handle(new ObterProdutoPorIdQuery(id), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ProdutoComImagens_MapeimagensCorretamente()
    {
        var id = Guid.NewGuid();
        var imagem = new Imagem
        {
            Id = Guid.NewGuid(),
            NomeArquivo = "foto.jpg",
            ChaveS3 = "produtos/foto.jpg",
            TipoConteudo = "image/jpeg",
            TamanhoBytes = 1024,
            DataCriacao = DateTime.UtcNow
        };
        var produto = new Produto
        {
            Id = id,
            Nome = "Notebook",
            SKU = "NB001",
            Preço = 3500m,
            IdCategoria = Guid.NewGuid(),
            Descrição = "Desc",
            Estoque = 10,
            Imagens = new List<Imagem> { imagem }
        };

        _produtoRepo.Setup(r => r.ObterComImagensAsync(id)).ReturnsAsync(produto);

        var resultado = await _handler.Handle(new ObterProdutoPorIdQuery(id), CancellationToken.None);

        resultado!.Imagens.Should().HaveCount(1);
        resultado.Imagens[0].ChaveS3.Should().Be("produtos/foto.jpg");
        resultado.Imagens[0].TipoConteudo.Should().Be("image/jpeg");
    }
}
