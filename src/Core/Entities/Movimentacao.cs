using DashHubApi.Core.Enums;

namespace DashHubApi.Core.Entities;

public class Movimentacao
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataMovimentacao { get; set; }
    public int CategoriaId { get; set; }
    public int UsuarioId { get; set; }
    public int? TransacaoId { get; set; }
    public StatusPagamento StatusPagamento { get; set; } = StatusPagamento.PENDENTE;
    public DateTime? DataPagamento { get; set; }
    public string? CategoriaNome { get; set; }
}
