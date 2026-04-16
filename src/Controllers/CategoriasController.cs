using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashHubApi.Controllers;

[ApiController]
[Authorize]
[Route("categorias")]
public class ControladorCategoria : ControllerBase
{
    private readonly IServicoCategoria _servicoCategoria;

    public ControladorCategoria(IServicoCategoria servicoCategoria)
    {
        _servicoCategoria = servicoCategoria;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RespostaCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var idUsuario = User.ObterIdUsuario();
        var categorias = await _servicoCategoria.ObterTodosAsync(idUsuario);
        return Ok(categorias);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RespostaCategoriaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] RequisicaoCategoriaDto dto)
    {
        var idUsuario = User.ObterIdUsuario();
        var categoria = await _servicoCategoria.CriarAsync(idUsuario, dto);
        return Created($"/categorias/{categoria.Id}", categoria);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RespostaCategoriaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] RequisicaoCategoriaDto dto)
    {
        var idUsuario = User.ObterIdUsuario();
        var categoria = await _servicoCategoria.AtualizarAsync(idUsuario, id, dto);
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deletar(int id)
    {
        var idUsuario = User.ObterIdUsuario();
        await _servicoCategoria.DeletarAsync(idUsuario, id);
        return NoContent();
    }
}
