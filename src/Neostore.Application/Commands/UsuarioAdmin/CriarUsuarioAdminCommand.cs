using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record CriarUsuarioAdminCommand(
    string Email,
    string Senha,
    string Role
) : IRequest<UsuarioAdminDto>;
