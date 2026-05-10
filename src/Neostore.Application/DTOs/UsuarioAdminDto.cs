namespace Neostore.Application.DTOs;

public class UsuarioAdminDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
