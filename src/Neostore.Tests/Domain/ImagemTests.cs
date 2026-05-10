using AwesomeAssertions;
using Neostore.Domain.Entities;

namespace Neostore.Tests.Domain;

public class ImagemTests
{
    [Fact]
    public void CriarImagem_ComDadosValidos_DeveDefinirPropriedadesCorretas()
    {
        var id = Guid.NewGuid();
        var nomeArquivo = "notebook.jpg";
        var chaveS3 = "produtos/notebook/imagem1.jpg";
        var tipoConteudo = "image/jpeg";
        var tamanhoBytes = 1024000L;
        var idProduto = Guid.NewGuid();
        var dataCriacao = DateTime.UtcNow;

        var imagem = new Imagem
        {
            Id = id,
            NomeArquivo = nomeArquivo,
            ChaveS3 = chaveS3,
            TipoConteudo = tipoConteudo,
            TamanhoBytes = tamanhoBytes,
            IdProduto = idProduto,
            DataCriacao = dataCriacao
        };

        imagem.Id.Should().Be(id);
        imagem.NomeArquivo.Should().Be(nomeArquivo);
        imagem.ChaveS3.Should().Be(chaveS3);
        imagem.TipoConteudo.Should().Be(tipoConteudo);
        imagem.TamanhoBytes.Should().Be(tamanhoBytes);
        imagem.IdProduto.Should().Be(idProduto);
        imagem.DataCriacao.Should().Be(dataCriacao);
    }

    [Fact]
    public void ObterUrlS3_ComBucketValido_DeveRetornarUrlCompleta()
    {
        var imagem = new Imagem { ChaveS3 = "produtos/notebook/imagem1.jpg" };
        var bucketUrl = "https://s3.amazonaws.com/meu-bucket";

        var url = imagem.ObterUrlS3(bucketUrl);

        url.Should().Be("https://s3.amazonaws.com/meu-bucket/produtos/notebook/imagem1.jpg");
    }

    [Fact]
    public void ObterUrlS3_ComBucketComBarraNoFinal_DeveRemoverBarraExtra()
    {
        var imagem = new Imagem { ChaveS3 = "produtos/notebook/imagem1.jpg" };
        var bucketUrl = "https://s3.amazonaws.com/meu-bucket/";

        var url = imagem.ObterUrlS3(bucketUrl);

        url.Should().Be("https://s3.amazonaws.com/meu-bucket/produtos/notebook/imagem1.jpg");
    }

    [Fact]
    public void ObterUrlS3_ComBucketVazio_DeveLançarExceção()
    {
        var imagem = new Imagem { ChaveS3 = "produtos/notebook/imagem1.jpg" };

        Action action = () => imagem.ObterUrlS3("");

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL do bucket não pode ser vazia. (Parameter 'bucketUrl')");
    }

    [Fact]
    public void ObterUrlS3_ComBucketNulo_DeveLançarExceção()
    {
        var imagem = new Imagem { ChaveS3 = "produtos/notebook/imagem1.jpg" };

        Action action = () => imagem.ObterUrlS3(null!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL do bucket não pode ser vazia. (Parameter 'bucketUrl')");
    }

    [Fact]
    public void ObterUrlS3_ComBucketEmBranco_DeveLançarExceção()
    {
        var imagem = new Imagem { ChaveS3 = "produtos/notebook/imagem1.jpg" };

        Action action = () => imagem.ObterUrlS3("   ");

        action.Should().Throw<ArgumentException>()
            .WithMessage("URL do bucket não pode ser vazia. (Parameter 'bucketUrl')");
    }

    [Theory]
    [InlineData("https://s3.amazonaws.com/bucket", "chave.jpg", "https://s3.amazonaws.com/bucket/chave.jpg")]
    [InlineData("https://cdn.cloudfront.net/assets", "imagens/produto/1.png", "https://cdn.cloudfront.net/assets/imagens/produto/1.png")]
    [InlineData("https://storage.cloud.google.com/meu-bucket", "fotos/item.webp", "https://storage.cloud.google.com/meu-bucket/fotos/item.webp")]
    public void ObterUrlS3_ComDiferentesBuckets_DeveRetornarUrlsCorretas(string bucketUrl, string chaveS3, string esperado)
    {
        var imagem = new Imagem { ChaveS3 = chaveS3 };

        var url = imagem.ObterUrlS3(bucketUrl);

        url.Should().Be(esperado);
    }

    [Fact]
    public void Imagem_DefaultValues_DeveSerVazios()
    {
        var imagem = new Imagem();

        imagem.Id.Should().Be(Guid.Empty);
        imagem.NomeArquivo.Should().BeEmpty();
        imagem.ChaveS3.Should().BeEmpty();
        imagem.TipoConteudo.Should().BeNull();
        imagem.TamanhoBytes.Should().Be(0);
        imagem.IdProduto.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Imagem_TamanhoNegativo_DeveAceitarQualquerValor()
    {
        var imagem = new Imagem { TamanhoBytes = -100 };

        imagem.TamanhoBytes.Should().Be(-100);
    }
}
