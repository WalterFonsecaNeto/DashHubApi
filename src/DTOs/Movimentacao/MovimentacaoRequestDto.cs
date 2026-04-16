using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Movimentacao;

public class RequisicaoMovimentacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime? DataMovimentacao { get; set; }
    public DateTime? DataInicio { get; set; }
    public int CategoriaId { get; set; }
    public StatusPagamento? StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
}
