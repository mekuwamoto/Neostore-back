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

    private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];
    private const long TamanhoMaximoBytes = 5 * 1024 * 1024;

    public ProdutoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProdutoDto>> Criar(
        [FromForm] CriarProdutoRequest request,
        CancellationToken cancellationToken)
    {
        foreach (IFormFile arquivo in request.Imagens)
        {
            if (!TiposPermitidos.Contains(arquivo.ContentType))
                return BadRequest(new { erro = $"Tipo de arquivo não permitido: {arquivo.ContentType}" });

            if (arquivo.Length > TamanhoMaximoBytes)
                return BadRequest(new { erro = "Arquivo excede o tamanho máximo de 5 MB." });
        }

        CriarProdutoCommand command = new(
            request.Nome,
            request.SKU,
            request.Preço,
            request.IdCategoria,
            request.Descrição,
            request.Estoque,
            request.Imagens);

        ProdutoDto resultado = await _mediator.Send(command, cancellationToken);
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
        ObterProdutosPaginadoQuery query = new(pagina, tamanho, idCategoria, nome, sku);
        ProdutosPaginadoDto resultado = await _mediator.Send(query);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
    {
        ProdutoDto? resultado = await _mediator.Send(new ObterProdutoPorIdQuery(id));
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
        AtualizarProdutoCommand commandComId = new(
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
        AjustarEstoqueCommand comando = new(id, request.Quantidade);
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

public record CriarProdutoRequest(
    string Nome,
    string SKU,
    decimal Preço,
    Guid IdCategoria,
    string Descrição,
    int Estoque,
    List<IFormFile> Imagens);

public class AjustarEstoqueRequest
{
    public int Quantidade { get; set; }
}
