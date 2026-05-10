namespace Neostore.Domain.Entities;

public class Categoria
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? IdCategoriaPai { get; set; }

    public static string GerarSlug(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

        var normalized = nome.Normalize(System.Text.NormalizationForm.FormD);
        var result = new System.Text.StringBuilder();

        foreach (char c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        return result
            .ToString()
            .Normalize(System.Text.NormalizationForm.FormC)
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-");
    }

    public void ValidarHierarquia(Categoria? categoriaPai)
    {
        if (IdCategoriaPai == null)
            return;

        if (categoriaPai == null)
            throw new InvalidOperationException("Categoria pai não encontrada.");

        if (categoriaPai.IdCategoriaPai == Id)
            throw new InvalidOperationException("Não é permitido circularidade na hierarquia de categorias.");
    }
}
