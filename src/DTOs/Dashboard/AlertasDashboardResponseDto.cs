using DashHubApi.Core.Enums;

namespace DashHubApi.DTOs.Dashboard;

public class AlertasDashboardResponseDto
{
    public List<AlertaVencimentoItemDto> VencenHoje { get; set; } = [];
    public List<AlertaVencimentoItemDto> VencemProximos7dias { get; set; } = [];
    public List<AlertaVencimentoItemDto> EmAtraso { get; set; } = [];
}

public class AlertaVencimentoItemDto
{
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public StatusPagamento StatusPagamento { get; set; }
}
