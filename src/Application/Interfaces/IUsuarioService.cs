using DashHubApi.DTOs.Usuario;

namespace DashHubApi.Application.Interfaces;

public interface IServicoUsuario
{
    Task<RespostaUsuarioMeuDto> ObterMeuAsync(int userId);
}
