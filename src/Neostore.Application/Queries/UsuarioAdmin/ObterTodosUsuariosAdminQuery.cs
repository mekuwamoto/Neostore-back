using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Queries.UsuarioAdmin;

public record ObterTodosUsuariosAdminQuery : IRequest<List<UsuarioAdminDto>>;

public class ObterTodosUsuariosAdminQueryHandler : IRequestHandler<ObterTodosUsuariosAdminQuery, List<UsuarioAdminDto>>
{
    private readonly IUsuarioAdminRepository _repository;
    private readonly IMapper _mapper;

    public ObterTodosUsuariosAdminQueryHandler(IUsuarioAdminRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<UsuarioAdminDto>> Handle(ObterTodosUsuariosAdminQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.UsuarioAdmin> usuarios = await _repository.ObterTodosAsync();
        return _mapper.Map<List<Domain.Entities.UsuarioAdmin>, List<UsuarioAdminDto>>(usuarios);
    }
}
