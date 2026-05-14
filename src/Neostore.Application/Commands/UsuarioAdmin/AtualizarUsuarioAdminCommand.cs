using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record AtualizarUsuarioAdminCommand(
    Guid Id,
    string Email,
    string Role
) : IRequest<UsuarioAdminDto>;

public class AtualizarUsuarioAdminCommandHandler : IRequestHandler<AtualizarUsuarioAdminCommand, UsuarioAdminDto>
{
    private readonly IUsuarioAdminRepository _repository;
    private readonly IMapper _mapper;

    public AtualizarUsuarioAdminCommandHandler(IUsuarioAdminRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UsuarioAdminDto> Handle(AtualizarUsuarioAdminCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.UsuarioAdmin? usuario = await _repository.ObterPorIdAsync(request.Id);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (usuario.Email != request.Email)
        {
            Domain.Entities.UsuarioAdmin? emailExistente = await _repository.ObterPorEmailAsync(request.Email);
            if (emailExistente != null)
                throw new InvalidOperationException($"Email '{request.Email}' já está em uso.");
        }

        usuario.Email = request.Email;
        usuario.Role = request.Role;

        await _repository.AtualizarAsync(usuario);

        return _mapper.Map<Domain.Entities.UsuarioAdmin, UsuarioAdminDto>(usuario);
    }
}
