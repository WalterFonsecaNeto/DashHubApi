using DashHubApi.Core.Entities;

namespace DashHubApi.Infrastructure.Repositories.Interfaces;

public interface IRepositorioUsuario
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario> CriarAsync(Usuario usuario);
}
