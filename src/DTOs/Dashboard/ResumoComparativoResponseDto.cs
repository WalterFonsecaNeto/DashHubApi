namespace DashHubApi.DTOs.Dashboard;

public class ResumoComparativoResponseDto
{
    public MesResumoDto MesAtual { get; set; } = new();
    public MesResumoDto MesAnterior { get; set; } = new();
    public VariacaoComparativaDto Variacao { get; set; } = new();
}

public class MesResumoDto
{
    public string Mes { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
}

public class VariacaoComparativaDto
{
    public decimal SaldoPercentual { get; set; }
    public decimal ReceitasPercentual { get; set; }
    public decimal DespesasPercentual { get; set; }
}
