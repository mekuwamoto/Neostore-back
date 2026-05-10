namespace Neostore.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Preço { get; set; }
    public Guid IdCategoria { get; set; }
    public string Descrição { get; set; } = string.Empty;
    public List<string> Imagens { get; set; } = new();
    public int Estoque { get; set; }

    public void AjustarEstoque(int quantidade)
    {
        if (Estoque + quantidade < 0)
            throw new InvalidOperationException("Estoque não pode ser negativo.");

        Estoque += quantidade;
    }

    public void AdicionarImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL de imagem não pode ser vazia.", nameof(url));

        Imagens.Add(url);
    }

    public void RemoverImagem(string url)
    {
        Imagens.Remove(url);
    }
}
