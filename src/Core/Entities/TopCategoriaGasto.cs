namespace DashHubApi.Core.Entities;

public class TopCategoriaGasto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal TotalGasto { get; set; }
    public int NumeroParcelas { get; set; }
}
