using Dapper;
using DashHubApi.Core.Entities;
using DashHubApi.Infrastructure.Database;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using DashHubApi.Infrastructure.Sql;

namespace DashHubApi.Infrastructure.Repositories;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly IFabricaConexaoBancoDados _fabricaConexaoBancoDados;

    public RepositorioUsuario(IFabricaConexaoBancoDados fabricaConexaoBancoDados)
    {
        _fabricaConexaoBancoDados = fabricaConexaoBancoDados;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryFirstOrDefaultAsync<Usuario>(SqlUsuario.ObterPorEmail, new { Email = email });
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        return await conexao.QueryFirstOrDefaultAsync<Usuario>(SqlUsuario.ObterPorId, new { Id = id });
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        using var conexao = _fabricaConexaoBancoDados.CreateConnection();
        var id = await conexao.QuerySingleAsync<int>(SqlUsuario.Criar, usuario);
        usuario.Id = id;
        return usuario;
    }
}
