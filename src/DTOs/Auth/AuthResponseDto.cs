namespace DashHubApi.DTOs.Auth;

public class RespostaAutenticacaoDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
