namespace DashHubApi.DTOs.Dashboard;

public class DistribuicaoCategoriaDashboardDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
