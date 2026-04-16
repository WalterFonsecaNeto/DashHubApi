namespace DashHubApi.DTOs.Dashboard;

public class TopCategoriaDashboardDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal TotalGasto { get; set; }
    public decimal Percentual { get; set; }
    public int NumeroParcelas { get; set; }
}
