using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.UsuarioAdmin;

public record ObterTodosUsuariosAdminQuery : IRequest<List<UsuarioAdminDto>>;
