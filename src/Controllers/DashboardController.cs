using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashHubApi.Controllers;

[ApiController]
[Authorize]
[Route("painel")]
[Route("dashboard")]
public class ControladorPainel : ControllerBase
{
    private readonly IServicoPainel _servicoPainel;

    public ControladorPainel(IServicoPainel servicoPainel)
    {
        _servicoPainel = servicoPainel;
    }

    [HttpGet("resumo")]
    [ProducesResponseType(typeof(RespostaPainelResumoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterResumo([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        var idUsuario = User.ObterIdUsuario();
        var resumo = await _servicoPainel.ObterResumoAsync(idUsuario, dataInicio, dataFim);
        return Ok(resumo);
    }

    [HttpGet("resumo-comparativo")]
    [ProducesResponseType(typeof(ResumoComparativoResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterResumoComparativo([FromQuery] string? mes)
    {
        var idUsuario = User.ObterIdUsuario();
        var resumo = await _servicoPainel.ObterResumoComparativoAsync(idUsuario, mes);
        return Ok(resumo);
    }

    [HttpGet("ultimas-movimentacoes")]
    [ProducesResponseType(typeof(IEnumerable<UltimaMovimentacaoDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterUltimasMovimentacoes([FromQuery] int limite = 10)
    {
        var idUsuario = User.ObterIdUsuario();
        var movimentacoes = await _servicoPainel.ObterUltimasMovimentacoesAsync(idUsuario, limite);
        return Ok(movimentacoes);
    }

    [HttpGet("distribuicao-categorias")]
    [ProducesResponseType(typeof(IEnumerable<DistribuicaoCategoriaDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterDistribuicaoCategorias([FromQuery] string? mes)
    {
        var idUsuario = User.ObterIdUsuario();
        var distribuicao = await _servicoPainel.ObterDistribuicaoCategoriasAsync(idUsuario, mes);
        return Ok(distribuicao);
    }

    [HttpGet("top-categorias")]
    [ProducesResponseType(typeof(IEnumerable<TopCategoriaDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTopCategorias([FromQuery] string? mes, [FromQuery] int limite = 5)
    {
        var idUsuario = User.ObterIdUsuario();
        var topCategorias = await _servicoPainel.ObterTopCategoriasAsync(idUsuario, mes, limite);
        return Ok(topCategorias);
    }

    [HttpGet("transacoes-ativas")]
    [ProducesResponseType(typeof(IEnumerable<TransacaoAtivaDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTransacoesAtivas()
    {
        var idUsuario = User.ObterIdUsuario();
        var transacoes = await _servicoPainel.ObterTransacoesAtivasAsync(idUsuario);
        return Ok(transacoes);
    }

    [HttpGet("evolucao-12-meses")]
    [ProducesResponseType(typeof(IEnumerable<EvolucaoSaldoMesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterEvolucao12Meses()
    {
        var idUsuario = User.ObterIdUsuario();
        var evolucao = await _servicoPainel.ObterEvolucao12MesesAsync(idUsuario);
        return Ok(evolucao);
    }

    [HttpGet("alertas")]
    [ProducesResponseType(typeof(AlertasDashboardResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAlertas()
    {
        var idUsuario = User.ObterIdUsuario();
        var alertas = await _servicoPainel.ObterAlertasAsync(idUsuario);
        return Ok(alertas);
    }
}
