using Dapper;
using DashHubApi.Core.Entities;
using DashHubApi.Infrastructure.Database;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using DashHubApi.Infrastructure.Sql;

namespace DashHubApi.Infrastructure.Repositories;

public class RepositorioCategoria : IRepositorioCategoria
{
    private readonly IFabricaConexaoBancoDados _fabricaConexaoBancoDados;

    public RepositorioCategoria(IFabricaConexaoBancoDados fabricaConexaoBancoDados)
    {
        _fabricaConexaoBancoDados = fabricaConexaoBancoDados;
    }

    public async Task<IEnumerable<Categoria>> ObterPorIdUsuarioAsync(int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryAsync<Categoria>(SqlCategoria.ObterPorIdUsuario, new { UserId = idUsuario });
    }

    public async Task<Categoria?> ObterPorIdEIdUsuarioAsync(int id, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryFirstOrDefaultAsync<Categoria>(
            SqlCategoria.ObterPorIdEIdUsuario,
            new { Id = id, UserId = idUsuario });
    }

    public async Task<Categoria> CriarAsync(Categoria categoria)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        categoria.Id = await conexao.QuerySingleAsync<int>(SqlCategoria.Criar, categoria);
        return categoria;
    }

    public async Task<bool> AtualizarAsync(Categoria categoria)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var linhas = await conexao.ExecuteAsync(SqlCategoria.Atualizar, categoria);
        return linhas > 0;
    }

    public async Task<bool> DeletarAsync(int id, int idUsuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var linhas = await conexao.ExecuteAsync(SqlCategoria.Deletar, new { Id = id, UserId = idUsuario });
        return linhas > 0;
    }
}
