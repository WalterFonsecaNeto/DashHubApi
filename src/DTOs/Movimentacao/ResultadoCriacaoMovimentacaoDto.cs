namespace DashHubApi.DTOs.Movimentacao;

public class ResultadoCriacaoMovimentacaoDto
{
    public int TransacaoId { get; set; }
    public string TipoTransacao { get; set; } = string.Empty;
    public RespostaMovimentacaoDto? MovimentacaoInicial { get; set; }
}
