using System.Data;

namespace DashHubApi.Infrastructure.Database;

public interface IFabricaConexaoBancoDados
{
    IDbConnection CreateConnection();
}
