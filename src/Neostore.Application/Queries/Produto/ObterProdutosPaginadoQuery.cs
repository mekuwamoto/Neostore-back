using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

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

public class ObterProdutosPaginadoQueryHandler : IRequestHandler<ObterProdutosPaginadoQuery, ProdutosPaginadoDto>
{
    private readonly IProdutoRepository _repository;
    private readonly IMapper _mapper;

    public ObterProdutosPaginadoQueryHandler(IProdutoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProdutosPaginadoDto> Handle(ObterProdutosPaginadoQuery request, CancellationToken cancellationToken)
    {
        var produtos = await _repository.ObterPaginadoAsync(request.Pagina, request.Tamanho, request.IdCategoria, request.Nome, request.SKU);
        var total = await _repository.ContarTotalAsync(request.IdCategoria, request.Nome, request.SKU);

        return new ProdutosPaginadoDto
        {
            Dados = _mapper.Map<List<Domain.Entities.Produto>, List<ProdutoDto>>(produtos),
            Total = total,
            Pagina = request.Pagina,
            Tamanho = request.Tamanho
        };
    }
}
