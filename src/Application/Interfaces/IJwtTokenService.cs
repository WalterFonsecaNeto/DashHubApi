using DashHubApi.Core.Entities;

namespace DashHubApi.Application.Interfaces;

public interface IServicoTokenJwt
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
