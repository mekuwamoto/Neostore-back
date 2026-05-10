using AwesomeAssertions;
using Neostore.Application.Commands.Categoria;
using Neostore.Application.Validators;

namespace Neostore.Tests.Application.Validators;

public class CriarCategoriaCommandValidatorTests
{
    private readonly CriarCategoriaCommandValidator _validator = new();

    [Fact]
    public void Validate_ComNomeValido_RetornaValido()
    {
        var command = new CriarCategoriaCommand("Eletrônicos", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NomeVazio_RetornaInvalido(string nome)
    {
        var command = new CriarCategoriaCommand(nome, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_NomeMenorQue2Chars_RetornaInvalido()
    {
        var command = new CriarCategoriaCommand("A", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_NomeComExatamente2Chars_RetornaValido()
    {
        var command = new CriarCategoriaCommand("AB", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ComCategoriaPaiInformada_RetornaValido()
    {
        var command = new CriarCategoriaCommand("Notebooks", Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}

public class AtualizarCategoriaCommandValidatorTests
{
    private readonly AtualizarCategoriaCommandValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_RetornaValido()
    {
        var command = new AtualizarCategoriaCommand(Guid.NewGuid(), "Eletrônicos", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_IdVazio_RetornaInvalido()
    {
        var command = new AtualizarCategoriaCommand(Guid.Empty, "Eletrônicos", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_NomeVazio_RetornaInvalido(string nome)
    {
        var command = new AtualizarCategoriaCommand(Guid.NewGuid(), nome, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_NomeMenorQue2Chars_RetornaInvalido()
    {
        var command = new AtualizarCategoriaCommand(Guid.NewGuid(), "A", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }
}
