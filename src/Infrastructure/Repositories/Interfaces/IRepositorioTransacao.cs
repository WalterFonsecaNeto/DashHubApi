using DashHubApi.Core.Entities;

namespace DashHubApi.Infrastructure.Repositories.Interfaces;

public interface IRepositorioTransacao
{
    Task<Transacao> CriarAsync(Transacao transacao);
    Task<Transacao?> ObterPorIdEIdUsuarioAsync(int transacaoId, int idUsuario);
    Task<IEnumerable<Transacao>> ObterAtivasAsync();
    Task<IEnumerable<Transacao>> ObterAtivasPorUsuarioAsync(int idUsuario);
    Task AtualizarParcelasGeradasEAtivaAsync(int transacaoId, int parcelasGeradas, bool ativa);
    Task<int> DesativarTodasAsync(int idUsuario);
}
