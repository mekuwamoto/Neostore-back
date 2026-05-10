using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.Produto;

public record CriarProdutoCommand(
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    List<ImagemInputDto> Imagens,
    int Estoque
) : IRequest<ProdutoDto>;

public class ImagemInputDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public string ChaveS3 { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
}
