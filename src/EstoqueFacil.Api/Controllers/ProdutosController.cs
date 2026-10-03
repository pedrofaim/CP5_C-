using EstoqueFacil.Api.Data;
using EstoqueFacil.Api.Dtos;
using EstoqueFacil.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Api.Controllers;

[ApiController]
[Route("api/v1/produtos")]
[Produces("application/json")]
public class ProdutosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> Listar(CancellationToken ct)
    {
        var produtos = await db.Produtos.AsNoTracking().OrderBy(p => p.Id)
            .Select(p => new ProdutoResponse(p.Id, p.Nome, p.Categoria, p.Preco, p.Quantidade))
            .ToListAsync(ct);
        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> Buscar(int id, CancellationToken ct)
    {
        var produto = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produto is null) return NaoEncontrado(id);
        return Ok(Resposta(produto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProdutoResponse>> Criar(ProdutoRequest request, CancellationToken ct)
    {
        var produto = new Produto();
        Preencher(produto, request);
        db.Produtos.Add(produto);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Buscar), new { id = produto.Id }, Resposta(produto));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, ProdutoRequest request, CancellationToken ct)
    {
        var produto = await db.Produtos.FindAsync([id], ct);
        if (produto is null) return NaoEncontrado(id);
        Preencher(produto, request);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var produto = await db.Produtos.FindAsync([id], ct);
        if (produto is null) return NaoEncontrado(id);
        db.Produtos.Remove(produto);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private ObjectResult NaoEncontrado(int id) => Problem(statusCode: 404,
        title: "Produto não encontrado", detail: $"Não existe produto com o ID {id}.");

    private static ProdutoResponse Resposta(Produto p) => new(p.Id, p.Nome, p.Categoria, p.Preco, p.Quantidade);

    private static void Preencher(Produto produto, ProdutoRequest request)
    {
        produto.Nome = request.Nome;
        produto.Categoria = request.Categoria;
        produto.Preco = decimal.Round(request.Preco!.Value, 2, MidpointRounding.AwayFromZero);
        produto.Quantidade = request.Quantidade!.Value;
    }
}
