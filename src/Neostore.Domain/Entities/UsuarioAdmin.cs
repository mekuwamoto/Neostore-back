using Neostore.Domain.Interfaces;

namespace Neostore.Domain.Entities;

public class UsuarioAdmin : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime? DeletadoEm { get; set; }

    public void AtualizarSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new ArgumentException("Hash de senha não pode ser vazio.", nameof(novoHash));

        SenhaHash = novoHash;
    }
}
