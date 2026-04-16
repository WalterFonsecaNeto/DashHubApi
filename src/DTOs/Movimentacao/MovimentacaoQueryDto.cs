namespace DashHubApi.DTOs.Movimentacao;

public class ConsultaMovimentacaoDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public bool OrdenarDesc { get; set; } = true;
}
