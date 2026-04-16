using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Movimentacao;

public class RespostaMovimentacaoDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataMovimentacao { get; set; }
    public DateTime DataInicio { get; set; }
    public int CategoriaId { get; set; }
    public int? TransacaoId { get; set; }
    public StatusPagamento StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? CategoriaNome { get; set; }
}
