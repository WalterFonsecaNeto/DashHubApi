using DashHubApi.Core.Entities;

namespace DashHubApi.Infrastructure.Repositories.Interfaces;

public interface IRepositorioMovimentacao
{
    Task<(IEnumerable<Movimentacao> itens, int total)> ObterPorIdUsuarioComPaginacaoAsync(
        int idUsuario,
        DateTime? dataInicio,
        DateTime? dataFim,
        int pagina,
        int tamanhoPagina,
        bool ordenarDesc);

    Task<Movimentacao?> ObterPorIdEIdUsuarioAsync(int id, int idUsuario);
    Task<Movimentacao> CriarAsync(Movimentacao movimentacao);
    Task<bool> ExisteNoMesAsync(int transacaoId, DateTime data);
    Task<bool> AtualizarAsync(Movimentacao movimentacao);
    Task<bool> AtualizarStatusAsync(Movimentacao movimentacao);
    Task<bool> DeletarAsync(int id, int idUsuario);
    Task<int> DeletarPorTransacaoAsync(int transacaoId, int idUsuario);
    Task<int> DeletarTodasAsync(int idUsuario);
    Task<(decimal TotalReceitas, decimal TotalDespesas)> ObterResumoAsync(int idUsuario, DateTime? dataInicio, DateTime? dataFim);
    Task<IEnumerable<DistribuicaoCategoria>> ObterDistribuicaoCategoriasAsync(int idUsuario, DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<TopCategoriaGasto>> ObterTopCategoriasAsync(int idUsuario, DateTime dataInicio, DateTime dataFim, int limite);
    Task<IEnumerable<Movimentacao>> ObterMovimentacoesAbertasParaAlertasAsync(int idUsuario, DateTime dataLimiteExclusiva);
}
