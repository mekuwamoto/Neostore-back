using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.UsuarioAdmin;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Handlers.UsuarioAdmin;

public class ObterUsuarioAdminPorIdQueryHandler : IRequestHandler<ObterUsuarioAdminPorIdQuery, UsuarioAdminDto?>
{
    private readonly IUsuarioAdminRepository _repository;
    private readonly IMapper _mapper;

    public ObterUsuarioAdminPorIdQueryHandler(IUsuarioAdminRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UsuarioAdminDto?> Handle(ObterUsuarioAdminPorIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.UsuarioAdmin? usuario = await _repository.ObterPorIdAsync(request.Id);
        if (usuario == null)
            return null;

        return _mapper.Map<Domain.Entities.UsuarioAdmin, UsuarioAdminDto>(usuario);
    }
}
