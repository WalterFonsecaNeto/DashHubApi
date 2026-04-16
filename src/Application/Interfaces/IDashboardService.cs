using DashHubApi.DTOs.Dashboard;

namespace DashHubApi.Application.Interfaces;

public interface IServicoPainel
{
    Task<RespostaPainelResumoDto> ObterResumoAsync(int userId, DateTime? dataInicio, DateTime? dataFim);
    Task<ResumoComparativoResponseDto> ObterResumoComparativoAsync(int userId, string? mes);
    Task<IEnumerable<UltimaMovimentacaoDashboardDto>> ObterUltimasMovimentacoesAsync(int userId, int limite);
    Task<IEnumerable<DistribuicaoCategoriaDashboardDto>> ObterDistribuicaoCategoriasAsync(int userId, string? mes);
    Task<IEnumerable<TopCategoriaDashboardDto>> ObterTopCategoriasAsync(int userId, string? mes, int limite);
    Task<IEnumerable<TransacaoAtivaDashboardDto>> ObterTransacoesAtivasAsync(int userId);
    Task<IEnumerable<EvolucaoSaldoMesDto>> ObterEvolucao12MesesAsync(int userId);
    Task<AlertasDashboardResponseDto> ObterAlertasAsync(int userId);
}
