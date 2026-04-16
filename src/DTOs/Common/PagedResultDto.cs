namespace DashHubApi.DTOs.Common;

public class ResultadoPaginadoDto<T>
{
    public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
}
