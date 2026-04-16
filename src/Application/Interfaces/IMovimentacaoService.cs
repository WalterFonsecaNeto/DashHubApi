using DashHubApi.DTOs.Common;
using DashHubApi.DTOs.Movimentacao;

namespace DashHubApi.Application.Interfaces;

public interface IServicoMovimentacao
{
    Task<ResultadoPaginadoDto<RespostaMovimentacaoDto>> ObterTodosAsync(int userId, ConsultaMovimentacaoDto query);
    Task<ResultadoCriacaoMovimentacaoDto> CriarAsync(int userId, CriarMovimentacaoDto dto);
    Task<RespostaMovimentacaoDto> AtualizarAsync(int userId, int id, RequisicaoMovimentacaoDto dto);
    Task<RespostaMovimentacaoDto> AtualizarStatusAsync(int userId, int id, AtualizarStatusMovimentacaoDto dto);
    Task DeletarAsync(int userId, int id);
    Task DeletarPorTransacaoAsync(int userId, int transacaoId);
    Task DeletarTodasAsync(int userId);
}
