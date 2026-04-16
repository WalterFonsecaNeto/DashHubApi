using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashHubApi.Controllers;

[ApiController]
[Authorize]
[Route("usuario")]
public class ControladorUsuario : ControllerBase
{
    private readonly IServicoUsuario _servicoUsuario;

    public ControladorUsuario(IServicoUsuario servicoUsuario)
    {
        _servicoUsuario = servicoUsuario;
    }

    [HttpGet("meu")]
    [ProducesResponseType(typeof(RespostaUsuarioMeuDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterMeu()
    {
        var idUsuario = User.ObterIdUsuario();
        var usuario = await _servicoUsuario.ObterMeuAsync(idUsuario);
        return Ok(usuario);
    }
}
