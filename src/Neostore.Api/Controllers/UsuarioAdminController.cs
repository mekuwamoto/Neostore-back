using MediatR;
using Microsoft.AspNetCore.Mvc;
using Neostore.Application.Commands.UsuarioAdmin;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.UsuarioAdmin;

namespace Neostore.Api.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
public class UsuarioAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuarioAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UsuarioAdminDto>> Criar([FromBody] CriarUsuarioAdminCommand command)
    {
        UsuarioAdminDto resultado = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioAdminDto>>> ObterTodos()
    {
        List<UsuarioAdminDto> resultado = await _mediator.Send(new ObterTodosUsuariosAdminQuery());
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioAdminDto>> ObterPorId(Guid id)
    {
        UsuarioAdminDto? resultado = await _mediator.Send(new ObterUsuarioAdminPorIdQuery(id));
        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioAdminDto>> Atualizar(Guid id, [FromBody] AtualizarUsuarioAdminCommand command)
    {
        AtualizarUsuarioAdminCommand commandComId = new(id, command.Email, command.Role);
        UsuarioAdminDto resultado = await _mediator.Send(commandComId);
        return Ok(resultado);
    }

    [HttpPatch("{id}/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaRequest request)
    {
        await _mediator.Send(new AtualizarSenhaCommand(id, request.SenhaAtual, request.NovaSenha));
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        bool resultado = await _mediator.Send(new DeletarUsuarioAdminCommand(id));
        if (!resultado)
            return NotFound();

        return NoContent();
    }
}

public record AtualizarSenhaRequest(string SenhaAtual, string NovaSenha);
