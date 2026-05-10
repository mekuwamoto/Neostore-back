namespace Neostore.Domain.Entities;

public class UsuarioAdmin
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public void AtualizarSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new ArgumentException("Hash de senha não pode ser vazio.", nameof(novoHash));

        SenhaHash = novoHash;
    }
}
