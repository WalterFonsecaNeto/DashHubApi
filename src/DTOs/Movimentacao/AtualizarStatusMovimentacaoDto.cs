using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Movimentacao;

public sealed class AtualizarStatusMovimentacaoDto
{
    public StatusPagamento StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
}
