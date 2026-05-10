using MediatR;
using Neostore.Application.DTOs;

namespace Neostore.Application.Queries.UsuarioAdmin;

public record ObterUsuarioAdminPorIdQuery(Guid Id) : IRequest<UsuarioAdminDto?>;
