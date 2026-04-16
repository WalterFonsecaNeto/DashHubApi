using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.DTOs.Usuario;
using DashHubApi.Infrastructure.Repositories.Interfaces;

namespace DashHubApi.Application.Services;

public class UsuarioService : IServicoUsuario
{
    private readonly IRepositorioUsuario _repositorioUsuario;

    public UsuarioService(IRepositorioUsuario repositorioUsuario)
    {
        _repositorioUsuario = repositorioUsuario;
    }

    public async Task<RespostaUsuarioMeuDto> ObterMeuAsync(int userId)
    {
        var usuario = await _repositorioUsuario.ObterPorIdAsync(userId)
            ?? throw new ExcecaoApi("Usuario nao encontrado", 404);

        return new RespostaUsuarioMeuDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            DataCriacao = usuario.DataCriacao
        };
    }
}
