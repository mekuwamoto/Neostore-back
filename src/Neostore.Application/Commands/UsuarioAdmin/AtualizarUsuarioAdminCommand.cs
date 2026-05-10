using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Commands.UsuarioAdmin;

public record AtualizarUsuarioAdminCommand(
    Guid Id,
    string Email,
    string Role
) : IRequest<UsuarioAdminDto>;
