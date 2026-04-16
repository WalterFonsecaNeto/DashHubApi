using Dapper;
using DashHubApi.Core.Entities;
using DashHubApi.Infrastructure.Database;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using DashHubApi.Infrastructure.Sql;
using MySql.Data.MySqlClient;

namespace DashHubApi.Infrastructure.Repositories;

public class RepositorioTransacao : IRepositorioTransacao
{
    private readonly IFabricaConexaoBancoDados _fabricaConexaoBancoDados;
    private readonly ILogger<RepositorioTransacao> _logger;

    public RepositorioTransacao(
        IFabricaConexaoBancoDados fabricaConexaoBancoDados,
        ILogger<RepositorioTransacao> logger)
    {
        _fabricaConexaoBancoDados = fabricaConexaoBancoDados;
        _logger = logger;
    }

    public async Task<Transacao> CriarAsync(Transacao transacao)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        transacao.Id = await conexao.QuerySingleAsync<int>(SqlTransacao.Criar, transacao);
        return transacao;
    }

    public async Task<Transacao?> ObterPorIdEIdUsuarioAsync(int transacaoId, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryFirstOrDefaultAsync<Transacao>(
            SqlTransacao.ObterPorIdEIdUsuario,
            new
            {
                TransacaoId = transacaoId,
                UserId = idUsuario
            });
    }

    public async Task<IEnumerable<Transacao>> ObterAtivasAsync()
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        try
        {
            return await conexao.QueryAsync<Transacao>(SqlTransacao.ObterAtivas);
        }
        catch (MySqlException ex) when (
            ex.Message.Contains("Unknown column 'parcelas_geradas'", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Unknown column 'ativa'", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Schema de transacoes desatualizado. Execute a migration de transacoes para habilitar o job diario. Erro: {Mensagem}",
                ex.Message);

            // Evita derrubar o serviço em ambientes que ainda não aplicaram a migration.
            return Enumerable.Empty<Transacao>();
        }
    }

    public async Task<IEnumerable<Transacao>> ObterAtivasPorUsuarioAsync(int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        try
        {
            return await conexao.QueryAsync<Transacao>(SqlTransacao.ObterAtivasPorUsuario, new { UserId = idUsuario });
        }
        catch (MySqlException ex) when (
            ex.Message.Contains("Unknown column 'parcelas_geradas'", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Unknown column 'ativa'", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Schema de transacoes desatualizado. Execute a migration de transacoes para habilitar o dashboard de transacoes ativas. Erro: {Mensagem}",
                ex.Message);

            return Enumerable.Empty<Transacao>();
        }
    }

    public async Task AtualizarParcelasGeradasEAtivaAsync(int transacaoId, int parcelasGeradas, bool ativa)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        await conexao.ExecuteAsync(
            SqlTransacao.AtualizarParcelasGeradasEAtiva,
            new
            {
                TransacaoId = transacaoId,
                ParcelasGeradas = parcelasGeradas,
                Ativa = ativa
            });
    }

    public async Task<int> DesativarTodasAsync(int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.ExecuteAsync(
            SqlTransacao.DesativarTodas,
            new { UserId = idUsuario });
    }
}
