using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Dashboard;

public class UltimaMovimentacaoDashboardDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? CategoriaNome { get; set; }
    public DateTime DataMovimentacao { get; set; }
    public int? TransacaoId { get; set; }
    public StatusPagamento StatusPagamento { get; set; }
}
