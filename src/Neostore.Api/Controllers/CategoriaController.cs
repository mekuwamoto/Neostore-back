using MediatR;
using Microsoft.AspNetCore.Mvc;
using Neostore.Application.Commands.Categoria;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Categoria;

namespace Neostore.Api.Controllers;

[ApiController]
[Route("api/admin/categorias")]
public class CategoriaController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CategoriaDto>> Criar([FromBody] CriarCategoriaCommand command)
    {
        CategoriaDto resultado = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoriaDto>>> ObterArvore()
    {
        var resultado = await _mediator.Send(new ObterTodasCategoriasQuery());
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> ObterPorId(Guid id)
    {
        var resultado = await _mediator.Send(new ObterCategoriaPorIdQuery(id));
        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> Atualizar(Guid id, [FromBody] AtualizarCategoriaCommand command)
    {
        AtualizarCategoriaCommand commandComId = new AtualizarCategoriaCommand(id, command.Nome, command.IdCategoriaPai);
        CategoriaDto resultado = await _mediator.Send(commandComId);
        return Ok(resultado);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        bool resultado = await _mediator.Send(new DeletarCategoriaCommand(id));
        if (!resultado)
            return NotFound();

        return NoContent();
    }
}
