using Dapper;
using DashHubApi.Core.Entities;
using DashHubApi.Infrastructure.Database;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using DashHubApi.Infrastructure.Sql;

namespace DashHubApi.Infrastructure.Repositories;

public class RepositorioMovimentacao : IRepositorioMovimentacao
{
    private readonly IFabricaConexaoBancoDados _fabricaConexaoBancoDados;

    public RepositorioMovimentacao(IFabricaConexaoBancoDados fabricaConexaoBancoDados)
    {
        _fabricaConexaoBancoDados = fabricaConexaoBancoDados;
    }

    public async Task<(IEnumerable<Movimentacao> itens, int total)> ObterPorIdUsuarioComPaginacaoAsync(
        int idUsuario,
        DateTime? dataInicio,
        DateTime? dataFim,
        int pagina,
        int tamanhoPagina,
        bool ordenarDesc)
    {
        var where = SqlMovimentacao.OndeIdUsuario;
        var parametros = new DynamicParameters();
        parametros.Add("UserId", idUsuario);

        if (dataInicio.HasValue)
        {
            where += " AND m.data_movimentacao >= @DataInicio ";
            parametros.Add("DataInicio", dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            where += " AND m.data_movimentacao <= @DataFim ";
            parametros.Add("DataFim", dataFim.Value);
        }

        var sqlContagem = string.Format(SqlMovimentacao.ContarPorIdUsuarioModelo, where);
        var sqlOrdenacao = ordenarDesc ? "DESC" : "ASC";
        var deslocamento = (pagina - 1) * tamanhoPagina;
        var sqlDados = string.Format(SqlMovimentacao.ObterPorIdUsuarioComPaginacaoModelo, where, sqlOrdenacao);

        parametros.Add("Limit", tamanhoPagina);
        parametros.Add("Offset", deslocamento);

        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var total = await conexao.QuerySingleAsync<int>(sqlContagem, parametros);
        var itens = await conexao.QueryAsync<Movimentacao>(sqlDados, parametros);

        return (itens, total);
    }

    public async Task<Movimentacao?> ObterPorIdEIdUsuarioAsync(int id, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryFirstOrDefaultAsync<Movimentacao>(
            SqlMovimentacao.ObterPorIdEIdUsuario,
            new { Id = id, UserId = idUsuario });
    }

    public async Task<Movimentacao> CriarAsync(Movimentacao movimentacao)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        movimentacao.Id = await conexao.QuerySingleAsync<int>(SqlMovimentacao.Criar, movimentacao);

        var criado = await ObterPorIdEIdUsuarioAsync(movimentacao.Id, movimentacao.UsuarioId);
        return criado ?? movimentacao;
    }

    public async Task<bool> ExisteNoMesAsync(int transacaoId, DateTime data)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var total = await conexao.QuerySingleAsync<int>(SqlMovimentacao.ExisteNoMes, new { TransacaoId = transacaoId, Data = data });
        return total > 0;
    }

    public async Task<bool> AtualizarAsync(Movimentacao movimentacao)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var linhas = await conexao.ExecuteAsync(SqlMovimentacao.Atualizar, movimentacao);
        return linhas > 0;
    }

    public async Task<bool> AtualizarStatusAsync(Movimentacao movimentacao)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var linhas = await conexao.ExecuteAsync(SqlMovimentacao.AtualizarStatus, movimentacao);
        return linhas > 0;
    }

    public async Task<bool> DeletarAsync(int id, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var linhas = await conexao.ExecuteAsync(SqlMovimentacao.Deletar, new { Id = id, UserId = idUsuario });
        return linhas > 0;
    }

    public async Task<int> DeletarPorTransacaoAsync(int transacaoId, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.ExecuteAsync(SqlMovimentacao.DeletarPorTransacao, new { TransacaoId = transacaoId, UserId = idUsuario });
    }

    public async Task<int> DeletarTodasAsync(int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.ExecuteAsync(SqlMovimentacao.DeletarTodas, new { UserId = idUsuario });
    }

    public async Task<(decimal TotalReceitas, decimal TotalDespesas)> ObterResumoAsync(int idUsuario, DateTime? dataInicio, DateTime? dataFim)
    {
        var parametros = new DynamicParameters();
        parametros.Add("UserId", idUsuario);
        parametros.Add("DataInicio", dataInicio);
        parametros.Add("DataFim", dataFim);

        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var resultado = await conexao.QuerySingleAsync<ResultadoResumoConsulta>(SqlMovimentacao.ObterResumo, parametros);
        return (resultado.TotalReceitas, resultado.TotalDespesas);
    }

    public async Task<IEnumerable<DistribuicaoCategoria>> ObterDistribuicaoCategoriasAsync(int idUsuario, DateTime dataInicio, DateTime dataFim)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryAsync<DistribuicaoCategoria>(
            SqlMovimentacao.ObterDistribuicaoCategorias,
            new
            {
                UserId = idUsuario,
                DataInicio = dataInicio,
                DataFim = dataFim
            });
    }

    public async Task<IEnumerable<TopCategoriaGasto>> ObterTopCategoriasAsync(int idUsuario, DateTime dataInicio, DateTime dataFim, int limite)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryAsync<TopCategoriaGasto>(
            SqlMovimentacao.ObterTopCategorias,
            new
            {
                UserId = idUsuario,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Limite = limite
            });
    }

    public async Task<IEnumerable<Movimentacao>> ObterMovimentacoesAbertasParaAlertasAsync(int idUsuario, DateTime dataLimiteExclusiva)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryAsync<Movimentacao>(
            SqlMovimentacao.ObterMovimentacoesAbertasParaAlertas,
            new
            {
                UserId = idUsuario,
                DataLimiteExclusiva = dataLimiteExclusiva
            });
    }

    private sealed class ResultadoResumoConsulta
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
    }
}
