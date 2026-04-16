namespace DashHubApi.Core.Entities;

public class Transacao
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string TipoTransacao { get; set; } = string.Empty;
    public int? QuantidadeParcelas { get; set; }
    public int ParcelasGeradas { get; set; }
    public int? DiaVencimento { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
}
