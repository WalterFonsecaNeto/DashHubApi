using DashHubApi.DTOs.Categoria;

namespace DashHubApi.Application.Interfaces;

public interface IServicoCategoria
{
    Task<IEnumerable<RespostaCategoriaDto>> ObterTodosAsync(int userId);
    Task<RespostaCategoriaDto> CriarAsync(int userId, RequisicaoCategoriaDto dto);
    Task<RespostaCategoriaDto> AtualizarAsync(int userId, int id, RequisicaoCategoriaDto dto);
    Task DeletarAsync(int userId, int id);
}
