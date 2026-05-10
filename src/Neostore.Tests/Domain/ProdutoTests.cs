using AwesomeAssertions;
using Neostore.Domain.Entities;

namespace Neostore.Tests.Domain;

public class ProdutoTests
{
    [Fact]
    public void CriarProduto_ComDadosValidos_DeveDefinirPropriedadesCorretas()
    {
        var id = Guid.NewGuid();
        var nome = "Notebook";
        var sku = "NB001";
        var preço = 3500.00m;
        var idCategoria = Guid.NewGuid();
        var descrição = "Notebook potente";
        var estoque = 10;

        var produto = new Produto
        {
            Id = id,
            Nome = nome,
            SKU = sku,
            Preço = preço,
            IdCategoria = idCategoria,
            Descrição = descrição,
            Estoque = estoque
        };

        produto.Id.Should().Be(id);
        produto.Nome.Should().Be(nome);
        produto.SKU.Should().Be(sku);
        produto.Preço.Should().Be(preço);
        produto.IdCategoria.Should().Be(idCategoria);
        produto.Descrição.Should().Be(descrição);
        produto.Estoque.Should().Be(estoque);
        produto.Imagens.Should().BeEmpty();
    }

    [Fact]
    public void AjustarEstoque_Incremento_DeveAumentarEstoque()
    {
        var produto = new Produto { Estoque = 10 };

        produto.AjustarEstoque(5);

        produto.Estoque.Should().Be(15);
    }

    [Fact]
    public void AjustarEstoque_Decremento_DeveReduzirEstoque()
    {
        var produto = new Produto { Estoque = 10 };

        produto.AjustarEstoque(-3);

        produto.Estoque.Should().Be(7);
    }

    [Fact]
    public void AjustarEstoque_DecrementoNegativoDisponivel_DeveResultarEmZero()
    {
        var produto = new Produto { Estoque = 5 };

        produto.AjustarEstoque(-5);

        produto.Estoque.Should().Be(0);
    }

    [Fact]
    public void AjustarEstoque_DecrementoAlemDoDisponivel_DeveLançarExceção()
    {
        var produto = new Produto { Estoque = 5 };

        Action action = () => produto.AjustarEstoque(-10);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Estoque não pode ser negativo.");
    }

    [Fact]
    public void AdicionarImagem_ComUrlValida_DeveAdicionarAoLista()
    {
        var produto = new Produto();
        var url = "https://example.com/imagem.jpg";

        produto.AdicionarImagem(url);

        produto.Imagens.Should().HaveCount(1);
        produto.Imagens[0].Should().Be(url);
    }

    [Fact]
    public void AdicionarImagem_ComUrlVazia_DeveLançarExceção()
    {
        var produto = new Produto();

        Action action = () => produto.AdicionarImagem("");

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL de imagem não pode ser vazia. (Parameter 'url')");
    }

    [Fact]
    public void AdicionarImagem_ComUrlNula_DeveLançarExceção()
    {
        var produto = new Produto();

        Action action = () => produto.AdicionarImagem(null!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL de imagem não pode ser vazia. (Parameter 'url')");
    }

    [Fact]
    public void AdicionarImagem_MultiplasChamadas_DeveAdicionarTodasAsUrls()
    {
        var produto = new Produto();
        var urls = new[] { "url1.jpg", "url2.jpg", "url3.jpg" };

        foreach (var url in urls)
        {
            produto.AdicionarImagem(url);
        }

        produto.Imagens.Should().HaveCount(3);
        produto.Imagens.Should().BeEquivalentTo(urls);
    }

    [Fact]
    public void RemoverImagem_ComUrlExistente_DeveRemoverDaLista()
    {
        var produto = new Produto();
        var url = "https://example.com/imagem.jpg";
        produto.Imagens.Add(url);

        produto.RemoverImagem(url);

        produto.Imagens.Should().BeEmpty();
    }

    [Fact]
    public void RemoverImagem_ComUrlNaoExistente_NaoDeveLançarExceção()
    {
        var produto = new Produto();

        Action action = () => produto.RemoverImagem("url-inexistente.jpg");

        action.Should().NotThrow();
        produto.Imagens.Should().BeEmpty();
    }

    [Fact]
    public void RemoverImagem_DeLista_DeveRemoverApenasEspecificada()
    {
        var produto = new Produto();
        produto.Imagens.AddRange(new[] { "url1.jpg", "url2.jpg", "url3.jpg" });

        produto.RemoverImagem("url2.jpg");

        produto.Imagens.Should().HaveCount(2);
        produto.Imagens.Should().NotContain("url2.jpg");
        produto.Imagens.Should().Contain("url1.jpg");
        produto.Imagens.Should().Contain("url3.jpg");
    }
}
