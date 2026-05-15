using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
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
        10,
        new List<IFormFile> { Mock.Of<IFormFile>() }
    );

    [Fact]
    public void Validate_ComDadosValidos_RetornaValido()
    {
        CriarProdutoCommand command = ComandoValido();

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NomeVazio_RetornaInvalido(string nome)
    {
        CriarProdutoCommand command = ComandoValido() with { Nome = nome };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_NomeMenorQue3Chars_RetornaInvalido()
    {
        CriarProdutoCommand command = ComandoValido() with { Nome = "AB" };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_SkuVazio_RetornaInvalido(string sku)
    {
        CriarProdutoCommand command = ComandoValido() with { SKU = sku };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

    [Fact]
    public void Validate_SkuMenorQue2Chars_RetornaInvalido()
    {
        CriarProdutoCommand command = ComandoValido() with { SKU = "A" };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PrecoMenorOuIgualZero_RetornaInvalido(decimal preco)
    {
        CriarProdutoCommand command = ComandoValido() with { Preço = preco };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preço");
    }

    [Fact]
    public void Validate_IdCategoriaVazio_RetornaInvalido()
    {
        CriarProdutoCommand command = ComandoValido() with { IdCategoria = Guid.Empty };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdCategoria");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_DescricaoVazia_RetornaInvalido(string descricao)
    {
        CriarProdutoCommand command = ComandoValido() with { Descrição = descricao };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descrição");
    }

    [Fact]
    public void Validate_EstoqueNegativo_RetornaInvalido()
    {
        CriarProdutoCommand command = ComandoValido() with { Estoque = -1 };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estoque");
    }

    [Fact]
    public void Validate_ListaImagensVazia_RetornaInvalido()
    {
        CriarProdutoCommand command = ComandoValido() with { Imagens = new List<IFormFile>() };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Imagens");
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
        FluentValidation.Results.ValidationResult result = _validator.Validate(ComandoValido());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_IdVazio_RetornaInvalido()
    {
        AtualizarProdutoCommand command = ComandoValido() with { Id = Guid.Empty };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Validate_NomeMenorQue3Chars_RetornaInvalido()
    {
        AtualizarProdutoCommand command = ComandoValido() with { Nome = "AB" };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_PrecoInvalido_RetornaInvalido(decimal preco)
    {
        AtualizarProdutoCommand command = ComandoValido() with { Preço = preco };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Preço");
    }

    [Fact]
    public void Validate_EstoqueNegativo_RetornaInvalido()
    {
        AtualizarProdutoCommand command = ComandoValido() with { Estoque = -1 };

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estoque");
    }
}
