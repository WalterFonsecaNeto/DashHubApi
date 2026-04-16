using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Movimentacao;

public class CriarMovimentacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string TipoTransacao { get; set; } = string.Empty;
    public int? QuantidadeParcelas { get; set; }
    public int? DiaVencimento { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public StatusPagamento? StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
}
