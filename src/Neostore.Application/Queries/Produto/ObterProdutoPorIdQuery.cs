using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Queries.Produto;

public record ObterProdutoPorIdQuery(Guid Id) : IRequest<ProdutoDto?>;

public class ObterProdutoPorIdQueryHandler : IRequestHandler<ObterProdutoPorIdQuery, ProdutoDto?>
{
    private readonly IProdutoRepository _repository;
    private readonly IMapper _mapper;

    public ObterProdutoPorIdQueryHandler(IProdutoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProdutoDto?> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await _repository.ObterComImagensAsync(request.Id);
        if (produto == null)
            return null;

        return _mapper.Map<Domain.Entities.Produto, ProdutoDto>(produto);
    }
}
