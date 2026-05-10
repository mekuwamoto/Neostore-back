using Neostore.Domain.Interfaces;

namespace Neostore.Domain.Entities;

public class Produto : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Preço { get; set; }
    public Guid IdCategoria { get; set; }
    public string Descrição { get; set; } = string.Empty;
    public List<Imagem> Imagens { get; set; } = new();
    public int Estoque { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? DeletadoEm { get; set; }

    public void AjustarEstoque(int quantidade)
    {
        if (Estoque + quantidade < 0)
            throw new InvalidOperationException("Estoque não pode ser negativo.");

        Estoque += quantidade;
    }

    public void AdicionarImagem(Imagem imagem)
    {
        if (imagem == null)
            throw new ArgumentNullException(nameof(imagem), "Imagem não pode ser nula.");

        if (string.IsNullOrWhiteSpace(imagem.ChaveS3))
            throw new ArgumentException("ChaveS3 da imagem não pode ser vazia.", nameof(imagem));

        imagem.IdProduto = Id;
        Imagens.Add(imagem);
    }

    public void RemoverImagem(Guid idImagem)
    {
        var imagem = Imagens.FirstOrDefault(x => x.Id == idImagem);
        if (imagem != null)
        {
            Imagens.Remove(imagem);
        }
    }
}
