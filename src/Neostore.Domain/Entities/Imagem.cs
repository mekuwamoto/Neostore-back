namespace Neostore.Domain.Entities;

public class Imagem
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string ChaveS3 { get; set; } = string.Empty;
    public string? TipoConteudo { get; set; }
    public long TamanhoBytes { get; set; }
    public Guid IdProduto { get; set; }
    public DateTime DataCriacao { get; set; }

    public string ObterUrlS3(string bucketUrl)
    {
        if (string.IsNullOrWhiteSpace(bucketUrl))
            throw new ArgumentException("URL do bucket não pode ser vazia.", nameof(bucketUrl));

        return $"{bucketUrl.TrimEnd('/')}/{ChaveS3}";
    }
}
