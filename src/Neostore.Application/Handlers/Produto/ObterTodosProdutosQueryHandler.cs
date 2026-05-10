using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Produto;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.Produto;

public class ObterTodosProdutosQueryHandler : IRequestHandler<ObterTodosProdutosQuery, List<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;
    private readonly IMapper _mapper;

    public ObterTodosProdutosQueryHandler(IProdutoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ProdutoDto>> Handle(ObterTodosProdutosQuery request, CancellationToken cancellationToken)
    {
        var produtos = await _repository.ObterTodosAsync();

        return _mapper.Map<List<Domain.Entities.Produto>, List<ProdutoDto>>(produtos);
    }
}
