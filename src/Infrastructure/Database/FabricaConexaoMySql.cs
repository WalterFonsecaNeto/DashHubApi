using System.Data;
using MySql.Data.MySqlClient;

namespace DashHubApi.Infrastructure.Database;

public class FabricaConexaoMySql : IFabricaConexaoBancoDados
{
    private readonly IConfiguration _configuration;

    public FabricaConexaoMySql(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var stringConexao = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada");

        return new MySqlConnection(stringConexao);
    }
}
