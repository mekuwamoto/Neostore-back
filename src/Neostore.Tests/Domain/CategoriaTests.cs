using AwesomeAssertions;
using Neostore.Domain.Entities;

namespace Neostore.Tests.Domain;

public class CategoriaTests
{
    [Fact]
    public void CriarCategoria_ComDadosValidos_DeveDefinirPropriedadesCorretas()
    {
        var id = Guid.NewGuid();
        var nome = "Eletrônicos";
        var slug = "eletronicos";
        var idCategoriaPai = Guid.NewGuid();

        var categoria = new Categoria
        {
            Id = id,
            Nome = nome,
            Slug = slug,
            IdCategoriaPai = idCategoriaPai
        };

        categoria.Id.Should().Be(id);
        categoria.Nome.Should().Be(nome);
        categoria.Slug.Should().Be(slug);
        categoria.IdCategoriaPai.Should().Be(idCategoriaPai);
    }

    [Fact]
    public void CriarCategoriaSemPai_DeveAceitarIdNulo()
    {
        var categoria = new Categoria
        {
            Nome = "Eletrônicos",
            Slug = "eletronicos"
        };

        categoria.IdCategoriaPai.Should().BeNull();
    }

    [Fact]
    public void GerarSlug_ComNomeValido_DeveRetornarSlugLowercase()
    {
        var nome = "Eletrônicos";

        var slug = Categoria.GerarSlug(nome);

        slug.Should().Be("eletronicos");
    }

    [Fact]
    public void GerarSlug_ComNomeComEspacos_DeveRetornarSlugComHifen()
    {
        var nome = "Eletrônicos em Geral";

        var slug = Categoria.GerarSlug(nome);

        slug.Should().Be("eletronicos-em-geral");
    }

    [Fact]
    public void GerarSlug_ComAcentos_DeveRemoverAcentosCorretos()
    {
        var nome = "São Paulo";

        var slug = Categoria.GerarSlug(nome);

        slug.Should().Be("sao-paulo");
    }

    [Theory]
    [InlineData("Açúcar", "acucar")]
    [InlineData("Café", "cafe")]
    [InlineData("Pão", "pao")]
    [InlineData("Coração", "coracao")]
    public void GerarSlug_ComDiferentesAcentos_DeveRemoverTodos(string nome, string esperado)
    {
        var slug = Categoria.GerarSlug(nome);

        slug.Should().Be(esperado);
    }

    [Fact]
    public void GerarSlug_ComEspacosNoInicio_DeveRemover()
    {
        var nome = "  Eletrônicos  ";

        var slug = Categoria.GerarSlug(nome);

        slug.Should().Be("eletronicos");
    }

    [Fact]
    public void GerarSlug_ComNomeVazio_DeveLançarExceção()
    {
        Action action = () => Categoria.GerarSlug("");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Nome não pode ser vazio. (Parameter 'nome')");
    }

    [Fact]
    public void GerarSlug_ComNomeNulo_DeveLançarExceção()
    {
        Action action = () => Categoria.GerarSlug(null!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Nome não pode ser vazio. (Parameter 'nome')");
    }

    [Fact]
    public void ValidarHierarquia_SemCategoriaPai_DeveRetornar()
    {
        var categoria = new Categoria { IdCategoriaPai = null };

        Action action = () => categoria.ValidarHierarquia(null);

        action.Should().NotThrow();
        categoria.IdCategoriaPai.Should().BeNull();
    }

    [Fact]
    public void ValidarHierarquia_ComCategoriaPaiValida_DeveRetornar()
    {
        var idCategoriaPai = Guid.NewGuid();
        var categoria = new Categoria { Id = Guid.NewGuid(), IdCategoriaPai = idCategoriaPai };
        var categoriaPai = new Categoria { Id = idCategoriaPai };

        Action action = () => categoria.ValidarHierarquia(categoriaPai);

        action.Should().NotThrow();
        categoria.IdCategoriaPai.Should().Be(idCategoriaPai);
    }

    [Fact]
    public void ValidarHierarquia_ComCategoriaPaiNula_DeveLançarExceção()
    {
        var categoria = new Categoria
        {
            IdCategoriaPai = Guid.NewGuid()
        };

        Action action = () => categoria.ValidarHierarquia(null);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Categoria pai não encontrada.");
    }

    [Fact]
    public void ValidarHierarquia_ComCircularidade_DeveLançarExceção()
    {
        var idCategoria = Guid.NewGuid();
        var idCategoriaPai = Guid.NewGuid();

        var categoria = new Categoria { Id = idCategoria, IdCategoriaPai = idCategoriaPai };
        var categoriaPai = new Categoria { Id = idCategoriaPai, IdCategoriaPai = idCategoria };

        Action action = () => categoria.ValidarHierarquia(categoriaPai);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é permitido circularidade na hierarquia de categorias.");
    }
}
