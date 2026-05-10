using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.Produto;

public record ObterProdutosPaginadoQuery(
    int Pagina,
    int Tamanho,
    Guid? IdCategoria = null,
    string? Nome = null,
    string? SKU = null
) : IRequest<ProdutosPaginadoDto>;

public class ProdutosPaginadoDto
{
    public List<ProdutoDto> Dados { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int TotalPaginas => (Total + Tamanho - 1) / Tamanho;
}
