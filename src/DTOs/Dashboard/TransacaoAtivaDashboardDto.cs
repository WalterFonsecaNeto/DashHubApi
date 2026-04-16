namespace DashHubApi.DTOs.Dashboard;

public class TransacaoAtivaDashboardDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string TipoTransacao { get; set; } = string.Empty;
    public DateTime ProximoVencimento { get; set; }
    public int? DiaVencimento { get; set; }
    public int? ParcelaAtual { get; set; }
    public int? TotalParcelas { get; set; }
}
