using AwesomeAssertions;
using Neostore.Domain.Entities;

namespace Neostore.Tests.Domain;

public class UsuarioAdminTests
{
    [Fact]
    public void CriarUsuarioAdmin_ComDadosValidos_DeveDefinirPropriedadesCorretas()
    {
        var id = Guid.NewGuid();
        var email = "admin@example.com";
        var senhaHash = "hash_seguro_123";
        var role = "Admin";

        var usuario = new UsuarioAdmin
        {
            Id = id,
            Email = email,
            SenhaHash = senhaHash,
            Role = role
        };

        usuario.Id.Should().Be(id);
        usuario.Email.Should().Be(email);
        usuario.SenhaHash.Should().Be(senhaHash);
        usuario.Role.Should().Be(role);
    }

    [Fact]
    public void AtualizarSenha_ComHashValido_DeveAtualizarSenhaHash()
    {
        var usuario = new UsuarioAdmin { SenhaHash = "hash_antigo" };
        var novoHash = "hash_novo_123";

        usuario.AtualizarSenha(novoHash);

        usuario.SenhaHash.Should().Be(novoHash);
    }

    [Fact]
    public void AtualizarSenha_ComHashVazio_DeveLançarExceção()
    {
        var usuario = new UsuarioAdmin { SenhaHash = "hash_antigo" };

        Action action = () => usuario.AtualizarSenha("");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Hash de senha não pode ser vazio. (Parameter 'novoHash')");
    }

    [Fact]
    public void AtualizarSenha_ComHashNulo_DeveLançarExceção()
    {
        var usuario = new UsuarioAdmin { SenhaHash = "hash_antigo" };

        Action action = () => usuario.AtualizarSenha(null!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Hash de senha não pode ser vazio. (Parameter 'novoHash')");
    }

    [Fact]
    public void AtualizarSenha_ComHashEmBranco_DeveLançarExceção()
    {
        var usuario = new UsuarioAdmin { SenhaHash = "hash_antigo" };

        Action action = () => usuario.AtualizarSenha("   ");

        action.Should().Throw<ArgumentException>()
            .WithMessage("Hash de senha não pode ser vazio. (Parameter 'novoHash')");
    }

    [Fact]
    public void AtualizarSenha_MultiplasChamadas_DeveAtualizarAosSucessivamente()
    {
        var usuario = new UsuarioAdmin { SenhaHash = "hash_0" };

        usuario.AtualizarSenha("hash_1");
        usuario.SenhaHash.Should().Be("hash_1");

        usuario.AtualizarSenha("hash_2");
        usuario.SenhaHash.Should().Be("hash_2");

        usuario.AtualizarSenha("hash_3");
        usuario.SenhaHash.Should().Be("hash_3");
    }

    [Fact]
    public void UsuarioAdmin_DefaultValues_DeveSerVazios()
    {
        var usuario = new UsuarioAdmin();

        usuario.Id.Should().Be(Guid.Empty);
        usuario.Email.Should().BeEmpty();
        usuario.SenhaHash.Should().BeEmpty();
        usuario.Role.Should().BeEmpty();
    }

    [Theory]
    [InlineData("admin@example.com")]
    [InlineData("user@domain.org")]
    [InlineData("test.email@company.co.uk")]
    public void CriarUsuarioAdmin_ComDiferentesEmails_DeveAceitarTodos(string email)
    {
        var usuario = new UsuarioAdmin { Email = email };

        usuario.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Gerente")]
    [InlineData("Editor")]
    public void CriarUsuarioAdmin_ComDiferentesRoles_DeveAceitarTodos(string role)
    {
        var usuario = new UsuarioAdmin { Role = role };

        usuario.Role.Should().Be(role);
    }
}
