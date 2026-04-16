using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Common;
using DashHubApi.DTOs.Movimentacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashHubApi.Controllers;

[ApiController]
[Authorize]
[Route("movimentacoes")]
public class ControladorMovimentacao : ControllerBase
{
    private readonly IServicoMovimentacao _servicoMovimentacao;

    public ControladorMovimentacao(IServicoMovimentacao servicoMovimentacao)
    {
        _servicoMovimentacao = servicoMovimentacao;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginadoDto<RespostaMovimentacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos([FromQuery] ConsultaMovimentacaoDto consulta)
    {
        var idUsuario = User.ObterIdUsuario();
        var movimentacoes = await _servicoMovimentacao.ObterTodosAsync(idUsuario, consulta);
        return Ok(movimentacoes);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultadoCriacaoMovimentacaoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CriarMovimentacaoDto dto)
    {
        var idUsuario = User.ObterIdUsuario();
        var resultado = await _servicoMovimentacao.CriarAsync(idUsuario, dto);
        return Created($"/movimentacoes?transacaoId={resultado.TransacaoId}", resultado);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RespostaMovimentacaoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] RequisicaoMovimentacaoDto dto)
    {
        var idUsuario = User.ObterIdUsuario();
        var movimentacao = await _servicoMovimentacao.AtualizarAsync(idUsuario, id, dto);
        return Ok(movimentacao);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(RespostaMovimentacaoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusMovimentacaoDto dto)
    {
        var idUsuario = User.ObterIdUsuario();
        var movimentacao = await _servicoMovimentacao.AtualizarStatusAsync(idUsuario, id, dto);
        return Ok(movimentacao);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deletar(int id)
    {
        var idUsuario = User.ObterIdUsuario();
        await _servicoMovimentacao.DeletarAsync(idUsuario, id);
        return NoContent();
    }

    [HttpDelete("transacao/{transacaoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletarMovimentacoesDaTransacao(int transacaoId)
    {
        var idUsuario = User.ObterIdUsuario();
        await _servicoMovimentacao.DeletarPorTransacaoAsync(idUsuario, transacaoId);
        return NoContent();
    }
}
