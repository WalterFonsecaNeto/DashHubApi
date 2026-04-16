using DashHubApi.Core.Entities;

namespace DashHubApi.Infrastructure.Repositories.Interfaces;

public interface IRepositorioCategoria
{
    Task<IEnumerable<Categoria>> ObterPorIdUsuarioAsync(int idUsuario);
    Task<Categoria?> ObterPorIdEIdUsuarioAsync(int id, int idUsuario);
    Task<Categoria> CriarAsync(Categoria categoria);
    Task<bool> AtualizarAsync(Categoria categoria);
    Task<bool> DeletarAsync(int id, int idUsuario);
}
