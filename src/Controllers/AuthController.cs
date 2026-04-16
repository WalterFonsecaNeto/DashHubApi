using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DashHubApi.Controllers;

[ApiController]
[Route("autenticacao")]
public class ControladorAutenticacao : ControllerBase
{
    private readonly IServicoAutenticacao _servicoAutenticacao;

    public ControladorAutenticacao(IServicoAutenticacao servicoAutenticacao)
    {
        _servicoAutenticacao = servicoAutenticacao;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(typeof(RespostaAutenticacaoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Registrar([FromBody] RequisicaoRegistroDto dto)
    {
        var resposta = await _servicoAutenticacao.RegistrarAsync(dto);
        return Ok(resposta);
    }

    [HttpPost("entrar")]
    [ProducesResponseType(typeof(RespostaAutenticacaoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Entrar([FromBody] RequisicaoLoginDto dto)
    {
        var resposta = await _servicoAutenticacao.EntrarAsync(dto);
        return Ok(resposta);
    }
}
