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
    public void AdicionarImagem_ComImagemValida_DeveAdicionarAoLista()
    {
        var produto = new Produto { Id = Guid.NewGuid() };
        var imagem = new Imagem
        {
            Id = Guid.NewGuid(),
            NomeArquivo = "notebook.jpg",
            ChaveS3 = "produtos/notebook/imagem1.jpg",
            TipoConteudo = "image/jpeg",
            TamanhoBytes = 1024000,
            DataCriacao = DateTime.UtcNow
        };

        produto.AdicionarImagem(imagem);

        produto.Imagens.Should().HaveCount(1);
        produto.Imagens[0].Id.Should().Be(imagem.Id);
        produto.Imagens[0].IdProduto.Should().Be(produto.Id);
    }

    [Fact]
    public void AdicionarImagem_ComImagemNula_DeveLançarExceção()
    {
        var produto = new Produto();

        Action action = () => produto.AdicionarImagem(null!);

        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Imagem não pode ser nula. (Parameter 'imagem')");
    }

    [Fact]
    public void AdicionarImagem_ComChaveS3Vazia_DeveLançarExceção()
    {
        var produto = new Produto();
        var imagem = new Imagem { ChaveS3 = "" };

        Action action = () => produto.AdicionarImagem(imagem);

        action.Should().Throw<ArgumentException>()
            .WithMessage("ChaveS3 da imagem não pode ser vazia. (Parameter 'imagem')");
    }

    [Fact]
    public void AdicionarImagem_MultiplasChamadas_DeveAdicionarTodasAsImagens()
    {
        var produto = new Produto { Id = Guid.NewGuid() };
        var imagens = new[]
        {
            new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img1.jpg", TamanhoBytes = 1000, DataCriacao = DateTime.UtcNow },
            new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img2.jpg", TamanhoBytes = 2000, DataCriacao = DateTime.UtcNow },
            new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img3.jpg", TamanhoBytes = 3000, DataCriacao = DateTime.UtcNow }
        };

        foreach (var imagem in imagens)
        {
            produto.AdicionarImagem(imagem);
        }

        produto.Imagens.Should().HaveCount(3);
        produto.Imagens.Should().AllSatisfy(x => x.IdProduto.Should().Be(produto.Id));
    }

    [Fact]
    public void RemoverImagem_ComIdExistente_DeveRemoverDaLista()
    {
        var produto = new Produto { Id = Guid.NewGuid() };
        var imagem = new Imagem
        {
            Id = Guid.NewGuid(),
            ChaveS3 = "notebook.jpg",
            TamanhoBytes = 1024000,
            DataCriacao = DateTime.UtcNow
        };
        produto.AdicionarImagem(imagem);

        produto.RemoverImagem(imagem.Id);

        produto.Imagens.Should().BeEmpty();
    }

    [Fact]
    public void RemoverImagem_ComIdNaoExistente_NaoDeveLançarExceção()
    {
        var produto = new Produto();

        Action action = () => produto.RemoverImagem(Guid.NewGuid());

        action.Should().NotThrow();
        produto.Imagens.Should().BeEmpty();
    }

    [Fact]
    public void RemoverImagem_DeLista_DeveRemoverApenasEspecificada()
    {
        var produto = new Produto { Id = Guid.NewGuid() };
        var img1 = new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img1.jpg", TamanhoBytes = 1000, DataCriacao = DateTime.UtcNow };
        var img2 = new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img2.jpg", TamanhoBytes = 2000, DataCriacao = DateTime.UtcNow };
        var img3 = new Imagem { Id = Guid.NewGuid(), ChaveS3 = "img3.jpg", TamanhoBytes = 3000, DataCriacao = DateTime.UtcNow };

        produto.AdicionarImagem(img1);
        produto.AdicionarImagem(img2);
        produto.AdicionarImagem(img3);

        produto.RemoverImagem(img2.Id);

        produto.Imagens.Should().HaveCount(2);
        produto.Imagens.Should().NotContain(x => x.Id == img2.Id);
        produto.Imagens.Should().Contain(x => x.Id == img1.Id);
        produto.Imagens.Should().Contain(x => x.Id == img3.Id);
    }
}
