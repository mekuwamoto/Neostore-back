using AutoMapper;
using MediatR;
using Neostore.Application.DTOs;
using Neostore.Persistence.Repositories;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record CriarUsuarioAdminCommand(
    string Email,
    string Senha,
    string Role
) : IRequest<UsuarioAdminDto>;

public class CriarUsuarioAdminCommandHandler : IRequestHandler<CriarUsuarioAdminCommand, UsuarioAdminDto>
{
    private readonly IUsuarioAdminRepository _repository;
    private readonly IMapper _mapper;

    public CriarUsuarioAdminCommandHandler(IUsuarioAdminRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UsuarioAdminDto> Handle(CriarUsuarioAdminCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.UsuarioAdmin? existente = await _repository.ObterPorEmailAsync(request.Email);
        if (existente != null)
            throw new InvalidOperationException($"Usuário com email '{request.Email}' já existe.");

        string senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        Domain.Entities.UsuarioAdmin usuario = new()
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            SenhaHash = senhaHash,
            Role = request.Role
        };

        await _repository.CriarAsync(usuario);

        return _mapper.Map<Domain.Entities.UsuarioAdmin, UsuarioAdminDto>(usuario);
    }
}
