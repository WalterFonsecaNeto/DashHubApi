using DashHubApi.DTOs.Auth;

namespace DashHubApi.Application.Interfaces;

public interface IServicoAutenticacao
{
    Task<RespostaAutenticacaoDto> RegistrarAsync(RequisicaoRegistroDto dto);
    Task<RespostaAutenticacaoDto> EntrarAsync(RequisicaoLoginDto dto);
}
