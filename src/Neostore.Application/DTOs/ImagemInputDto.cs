namespace Neostore.Application.DTOs;

public class ImagemInputDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public string ChaveS3 { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
}
