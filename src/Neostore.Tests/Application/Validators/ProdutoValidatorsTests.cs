using AwesomeAssertions;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Application.Validators;

namespace Neostore.Tests.Application.Validators;

public class CriarProdutoCommandValidatorTests
{
    private readonly CriarProdutoCommandValidator _validator = new();

    private static CriarProdutoCommand ComandoValido() => new(
        "Notebook",
        "NB001",
        3500m,
        Guid.NewGuid(),
        "Descrição válida",
        new List<ImagemInputDto>(),
        10
    );

    [Fact]
    public void Validate_ComDadosValidos_RetornaValido()
    {
        var result = _validator.Validate(ComandoValido());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NomeVazio_RetornaInvalido(string nome)
    {
        var command = ComandoValido() with { Nome = nome };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_NomeMenorQue3Chars_RetornaInvalido()
    {
        var command = ComandoValido() with { Nome = "AB" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_SkuVazio_RetornaInvalido(string sku)
    {
        var command = ComandoValido() with { SKU = sku };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

    [Fact]
    public void Validate_SkuMenorQue2Chars_RetornaInvalido()
    {
        var command = ComandoValido() with { SKU = "A" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PrecoMenorOuIgualZero_RetornaInvalido(decimal preco)
    {
        var command = ComandoValido() with { Preço = preco };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preço");
    }

    [Fact]
    public void Validate_IdCategoriaVazio_RetornaInvalido()
    {
        var command = ComandoValido() with { IdCategoria = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdCategoria");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_DescricaoVazia_RetornaInvalido(string descricao)
    {
        var command = ComandoValido() with { Descrição = descricao };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descrição");
    }

    [Fact]
    public void Validate_EstoqueNegativo_RetornaInvalido()
    {
        var command = ComandoValido() with { Estoque = -1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estoque");
    }

    [Fact]
    public void Validate_ImagemComChaveS3Vazia_RetornaInvalido()
    {
        var imagens = new List<ImagemInputDto> { new() { ChaveS3 = "", NomeArquivo = "foto.jpg" } };
        var command = ComandoValido() with { Imagens = imagens };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}

public class AtualizarProdutoCommandValidatorTests
{
    private readonly AtualizarProdutoCommandValidator _validator = new();

    private static AtualizarProdutoCommand ComandoValido() => new(
        Guid.NewGuid(),
        "Notebook",
        "NB001",
        3500m,
        Guid.NewGuid(),
        "Descrição válida",
        new List<ImagemInputDto>(),
        10
    );

    [Fact]
    public void Validate_ComDadosValidos_RetornaValido()
    {
        var result = _validator.Validate(ComandoValido());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_IdVazio_RetornaInvalido()
    {
        var command = ComandoValido() with { Id = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Validate_NomeMenorQue3Chars_RetornaInvalido()
    {
        var command = ComandoValido() with { Nome = "AB" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_PrecoInvalido_RetornaInvalido(decimal preco)
    {
        var command = ComandoValido() with { Preço = preco };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preço");
    }

    [Fact]
    public void Validate_EstoqueNegativo_RetornaInvalido()
    {
        var command = ComandoValido() with { Estoque = -1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estoque");
    }
}
