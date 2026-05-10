namespace Neostore.Application.DTOs;

public class ProdutoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Preço { get; set; }
    public Guid IdCategoria { get; set; }
    public string Descrição { get; set; } = string.Empty;
    public List<ImagemDto> Imagens { get; set; } = new();
    public int Estoque { get; set; }
}

public class ImagemDto
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string ChaveS3 { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public DateTime DataCriacao { get; set; }
}
