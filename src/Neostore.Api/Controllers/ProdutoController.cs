using MediatR;
using Microsoft.AspNetCore.Mvc;
using Neostore.Application.Commands.Produto;
using Neostore.Application.DTOs;
using Neostore.Application.Queries.Produto;

namespace Neostore.Api.Controllers;

[ApiController]
[Route("api/admin/produtos")]
public class ProdutoController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProdutoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoCommand command)
    {
        try
        {
            var resultado = await _mediator.Send(command);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ProdutosPaginadoDto>> ObterPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        [FromQuery] Guid? idCategoria = null,
        [FromQuery] string? nome = null,
        [FromQuery] string? sku = null)
    {
        var query = new ObterProdutosPaginadoQuery(pagina, tamanho, idCategoria, nome, sku);
        var resultado = await _mediator.Send(query);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
    {
        var resultado = await _mediator.Send(new ObterProdutoPorIdQuery(id));
        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, [FromBody] AtualizarProdutoCommand command)
    {
        try
        {
            var commandComId = new AtualizarProdutoCommand(
                id,
                command.Nome,
                command.SKU,
                command.Preço,
                command.IdCategoria,
                command.Descrição,
                command.Imagens,
                command.Estoque
            );
            var resultado = await _mediator.Send(commandComId);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPatch("{id}/estoque")]
    public async Task<ActionResult<int>> AjustarEstoque(Guid id, [FromBody] AjustarEstoqueRequest request)
    {
        try
        {
            var comando = new AjustarEstoqueCommand(id, request.Quantidade);
            var novoEstoque = await _mediator.Send(comando);
            return Ok(new { estoque = novoEstoque });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        try
        {
            var resultado = await _mediator.Send(new DeletarProdutoCommand(id));
            if (!resultado)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}

public class AjustarEstoqueRequest
{
    public int Quantidade { get; set; }
}
