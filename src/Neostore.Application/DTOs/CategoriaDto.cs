namespace Neostore.Application.DTOs;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? IdCategoriaPai { get; set; }
}
