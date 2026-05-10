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
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoCommand command)
    {
        ProdutoDto resultado = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProdutosPaginadoDto), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
    {
        var resultado = await _mediator.Send(new ObterProdutoPorIdQuery(id));
        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, [FromBody] AtualizarProdutoCommand command)
    {
        AtualizarProdutoCommand commandComId = new AtualizarProdutoCommand(
            id,
            command.Nome,
            command.SKU,
            command.Preço,
            command.IdCategoria,
            command.Descrição,
            command.Imagens,
            command.Estoque
        );
        ProdutoDto resultado = await _mediator.Send(commandComId);
        return Ok(resultado);
    }

    [HttpPatch("{id}/estoque")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> AjustarEstoque(Guid id, [FromBody] AjustarEstoqueRequest request)
    {
        AjustarEstoqueCommand comando = new AjustarEstoqueCommand(id, request.Quantidade);
        int novoEstoque = await _mediator.Send(comando);
        return Ok(new { estoque = novoEstoque });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        bool resultado = await _mediator.Send(new DeletarProdutoCommand(id));
        if (!resultado)
            return NotFound();

        return NoContent();
    }
}

public class AjustarEstoqueRequest
{
    public int Quantidade { get; set; }
}
