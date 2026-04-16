using DashHubApi.Application.Interfaces;
using DashHubApi.Core.Entities;
using DashHubApi.DTOs.Dashboard;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using System.Globalization;

namespace DashHubApi.Application.Services;

public class DashboardService : IServicoPainel
{
    private readonly IRepositorioMovimentacao _repositorioMovimentacao;
    private readonly IRepositorioTransacao _repositorioTransacao;

    public DashboardService(
        IRepositorioMovimentacao repositorioMovimentacao,
        IRepositorioTransacao repositorioTransacao)
    {
        _repositorioMovimentacao = repositorioMovimentacao;
        _repositorioTransacao = repositorioTransacao;
    }

    public async Task<RespostaPainelResumoDto> ObterResumoAsync(int userId, DateTime? dataInicio, DateTime? dataFim)
    {
        if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
        {
            throw new Application.Common.ExcecaoApi("DataInicio nao pode ser maior que DataFim");
        }

        if (!dataInicio.HasValue && !dataFim.HasValue)
        {
            var hoje = DateTime.Today;
            dataInicio = new DateTime(hoje.Year, hoje.Month, 1);
            dataFim = dataInicio.Value.AddMonths(1).AddDays(-1);
        }

        var (totalReceitas, totalDespesas) = await _repositorioMovimentacao.ObterResumoAsync(userId, dataInicio, dataFim);
        return new RespostaPainelResumoDto
        {
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            Saldo = totalReceitas - totalDespesas
        };
    }

    public async Task<ResumoComparativoResponseDto> ObterResumoComparativoAsync(int userId, string? mes)
    {
        var (inicioAtual, fimAtual, mesAtual) = ObterPeriodoMes(mes);
        var inicioAnterior = inicioAtual.AddMonths(-1);
        var fimAnterior = inicioAtual.AddDays(-1);
        var mesAnterior = FormatarMes(inicioAnterior);

        var (receitasAtual, despesasAtual) = await _repositorioMovimentacao.ObterResumoAsync(userId, inicioAtual, fimAtual);
        var (receitasAnterior, despesasAnterior) = await _repositorioMovimentacao.ObterResumoAsync(userId, inicioAnterior, fimAnterior);

        var saldoAtual = receitasAtual - despesasAtual;
        var saldoAnterior = receitasAnterior - despesasAnterior;

        return new ResumoComparativoResponseDto
        {
            MesAtual = new MesResumoDto
            {
                Mes = mesAtual,
                Saldo = saldoAtual,
                TotalReceitas = receitasAtual,
                TotalDespesas = despesasAtual
            },
            MesAnterior = new MesResumoDto
            {
                Mes = mesAnterior,
                Saldo = saldoAnterior,
                TotalReceitas = receitasAnterior,
                TotalDespesas = despesasAnterior
            },
            Variacao = new VariacaoComparativaDto
            {
                SaldoPercentual = CalcularVariacaoPercentual(saldoAtual, saldoAnterior),
                ReceitasPercentual = CalcularVariacaoPercentual(receitasAtual, receitasAnterior),
                DespesasPercentual = CalcularVariacaoPercentual(despesasAtual, despesasAnterior)
            }
        };
    }

    public async Task<IEnumerable<UltimaMovimentacaoDashboardDto>> ObterUltimasMovimentacoesAsync(int userId, int limite)
    {
        var limiteNormalizado = limite <= 0 ? 10 : Math.Min(limite, 100);
        var (itens, _) = await _repositorioMovimentacao.ObterPorIdUsuarioComPaginacaoAsync(userId, null, null, 1, limiteNormalizado, true);

        return itens.Select(m => new UltimaMovimentacaoDashboardDto
        {
            Id = m.Id,
            Descricao = m.Descricao,
            Valor = m.Valor,
            Tipo = m.Tipo.ToUpperInvariant(),
            CategoriaNome = m.CategoriaNome,
            DataMovimentacao = m.DataMovimentacao,
            TransacaoId = m.TransacaoId,
            StatusPagamento = m.StatusPagamento
        });
    }

    public async Task<IEnumerable<DistribuicaoCategoriaDashboardDto>> ObterDistribuicaoCategoriasAsync(int userId, string? mes)
    {
        var (dataInicio, dataFim, _) = ObterPeriodoMes(mes);
        var distribuicao = (await _repositorioMovimentacao.ObterDistribuicaoCategoriasAsync(userId, dataInicio, dataFim)).ToList();
        var total = distribuicao.Sum(x => x.Valor);

        return distribuicao.Select(x => new DistribuicaoCategoriaDashboardDto
        {
            Categoria = x.Categoria,
            Valor = x.Valor,
            Percentual = total == 0 ? 0 : Math.Round((x.Valor / total) * 100, 2, MidpointRounding.AwayFromZero),
            Tipo = x.Tipo.ToUpperInvariant()
        });
    }

    public async Task<IEnumerable<TopCategoriaDashboardDto>> ObterTopCategoriasAsync(int userId, string? mes, int limite)
    {
        var (dataInicio, dataFim, _) = ObterPeriodoMes(mes);
        var limiteNormalizado = limite <= 0 ? 5 : Math.Min(limite, 50);
        var top = (await _repositorioMovimentacao.ObterTopCategoriasAsync(userId, dataInicio, dataFim, limiteNormalizado)).ToList();
        var total = top.Sum(x => x.TotalGasto);

        return top.Select(x => new TopCategoriaDashboardDto
        {
            Categoria = x.Categoria,
            TotalGasto = x.TotalGasto,
            Percentual = total == 0 ? 0 : Math.Round((x.TotalGasto / total) * 100, 2, MidpointRounding.AwayFromZero),
            NumeroParcelas = x.NumeroParcelas
        });
    }

    public async Task<IEnumerable<TransacaoAtivaDashboardDto>> ObterTransacoesAtivasAsync(int userId)
    {
        var hoje = DateTime.Today;
        var transacoes = await _repositorioTransacao.ObterAtivasPorUsuarioAsync(userId);

        var itens = new List<TransacaoAtivaDashboardDto>();
        foreach (var transacao in transacoes)
        {
            var tipoTransacao = transacao.TipoTransacao.Trim().ToUpperInvariant();
            if (tipoTransacao is not ("RECORRENTE" or "PARCELADA"))
            {
                continue;
            }

            var proximoVencimento = CalcularProximoVencimento(transacao, hoje);
            if (!proximoVencimento.HasValue)
            {
                continue;
            }

            var dto = new TransacaoAtivaDashboardDto
            {
                Id = transacao.Id,
                Descricao = transacao.Descricao,
                Tipo = transacao.Tipo.ToUpperInvariant(),
                Valor = ObterValorExibicaoTransacao(transacao),
                TipoTransacao = tipoTransacao,
                ProximoVencimento = proximoVencimento.Value,
                DiaVencimento = transacao.DiaVencimento
            };

            if (tipoTransacao == "PARCELADA" && transacao.QuantidadeParcelas.HasValue)
            {
                dto.ParcelaAtual = Math.Min(transacao.ParcelasGeradas + 1, transacao.QuantidadeParcelas.Value);
                dto.TotalParcelas = transacao.QuantidadeParcelas.Value;
            }

            itens.Add(dto);
        }

        return itens.OrderBy(x => x.ProximoVencimento).ThenBy(x => x.Id);
    }

    public async Task<IEnumerable<EvolucaoSaldoMesDto>> ObterEvolucao12MesesAsync(int userId)
    {
        var competenciaAtual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var inicioJanela = competenciaAtual.AddMonths(-11);
        var resposta = new List<EvolucaoSaldoMesDto>(12);

        for (var i = 0; i < 12; i++)
        {
            var competencia = inicioJanela.AddMonths(i);
            var dataInicio = competencia;
            var dataFim = competencia.AddMonths(1).AddDays(-1);
            var (receitas, despesas) = await _repositorioMovimentacao.ObterResumoAsync(userId, dataInicio, dataFim);

            resposta.Add(new EvolucaoSaldoMesDto
            {
                Mes = FormatarMes(competencia),
                Saldo = receitas - despesas
            });
        }

        return resposta;
    }

    public async Task<AlertasDashboardResponseDto> ObterAlertasAsync(int userId)
    {
        var hoje = DateTime.Today;
        var limiteProximosDias = hoje.AddDays(7);
        var movimentacoes = await _repositorioMovimentacao.ObterMovimentacoesAbertasParaAlertasAsync(userId, limiteProximosDias.AddDays(1));

        var vencenHoje = new List<AlertaVencimentoItemDto>();
        var vencemProximos7dias = new List<AlertaVencimentoItemDto>();
        var emAtraso = new List<AlertaVencimentoItemDto>();

        foreach (var movimentacao in movimentacoes)
        {
            var dataVencimento = movimentacao.DataMovimentacao.Date;

            var item = new AlertaVencimentoItemDto
            {
                Descricao = movimentacao.Descricao,
                DataVencimento = dataVencimento,
                StatusPagamento = movimentacao.StatusPagamento
            };

            if (dataVencimento == hoje)
            {
                vencenHoje.Add(item);
                continue;
            }

            if (dataVencimento < hoje)
            {
                emAtraso.Add(item);
                continue;
            }

            if (dataVencimento <= limiteProximosDias)
            {
                vencemProximos7dias.Add(item);
            }
        }

        return new AlertasDashboardResponseDto
        {
            VencenHoje = vencenHoje.OrderBy(x => x.DataVencimento).ToList(),
            VencemProximos7dias = vencemProximos7dias.OrderBy(x => x.DataVencimento).ToList(),
            EmAtraso = emAtraso.OrderBy(x => x.DataVencimento).ToList()
        };
    }

    private static (DateTime dataInicio, DateTime dataFim, string mesFormatado) ObterPeriodoMes(string? mes)
    {
        if (string.IsNullOrWhiteSpace(mes))
        {
            var hoje = DateTime.Today;
            var inicio = new DateTime(hoje.Year, hoje.Month, 1);
            var fim = inicio.AddMonths(1).AddDays(-1);
            return (inicio, fim, FormatarMes(inicio));
        }

        if (!DateTime.TryParseExact(mes, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataMes))
        {
            throw new Application.Common.ExcecaoApi("Parametro mes invalido. Use o formato yyyy-MM");
        }

        var dataInicio = new DateTime(dataMes.Year, dataMes.Month, 1);
        var dataFim = dataInicio.AddMonths(1).AddDays(-1);
        return (dataInicio, dataFim, FormatarMes(dataInicio));
    }

    private static string FormatarMes(DateTime data)
    {
        return $"{data.Year:D4}-{data.Month:D2}";
    }

    private static decimal CalcularVariacaoPercentual(decimal atual, decimal anterior)
    {
        if (anterior == 0)
        {
            return atual == 0 ? 0 : 100;
        }

        return Math.Round(((atual - anterior) / anterior) * 100, 2, MidpointRounding.AwayFromZero);
    }

    private static DateTime? CalcularProximoVencimento(Transacao transacao, DateTime hoje)
    {
        var tipoTransacao = transacao.TipoTransacao.Trim().ToUpperInvariant();
        if (tipoTransacao == "PARCELADA")
        {
            if (!transacao.QuantidadeParcelas.HasValue || transacao.ParcelasGeradas >= transacao.QuantidadeParcelas.Value)
            {
                return null;
            }

            return transacao.DataInicio.Date.AddMonths(transacao.ParcelasGeradas);
        }

        if (tipoTransacao != "RECORRENTE")
        {
            return null;
        }

        var competencia = new DateTime(hoje.Year, hoje.Month, 1);
        var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);
        if (competencia < competenciaInicio)
        {
            competencia = competenciaInicio;
        }

        var proximo = CalcularDataNoMes(transacao, competencia);
        if (proximo < hoje)
        {
            competencia = competencia.AddMonths(1);
            proximo = CalcularDataNoMes(transacao, competencia);
        }

        if (transacao.DataFim.HasValue)
        {
            var competenciaFim = new DateTime(transacao.DataFim.Value.Year, transacao.DataFim.Value.Month, 1);
            if (competencia > competenciaFim)
            {
                return null;
            }
        }

        return proximo;
    }

    private static DateTime? CalcularDataReferenciaAlerta(Transacao transacao, DateTime hoje)
    {
        var tipoTransacao = transacao.TipoTransacao.Trim().ToUpperInvariant();
        if (tipoTransacao == "PARCELADA")
        {
            if (!transacao.QuantidadeParcelas.HasValue || transacao.ParcelasGeradas >= transacao.QuantidadeParcelas.Value)
            {
                return null;
            }

            return transacao.DataInicio.Date.AddMonths(transacao.ParcelasGeradas);
        }

        if (tipoTransacao != "RECORRENTE")
        {
            return null;
        }

        var competencia = new DateTime(hoje.Year, hoje.Month, 1);
        var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);
        if (competencia < competenciaInicio)
        {
            competencia = competenciaInicio;
        }

        if (transacao.DataFim.HasValue)
        {
            var competenciaFim = new DateTime(transacao.DataFim.Value.Year, transacao.DataFim.Value.Month, 1);
            if (competencia > competenciaFim)
            {
                return null;
            }
        }

        return CalcularDataNoMes(transacao, competencia);
    }

    private static DateTime CalcularDataNoMes(Transacao transacao, DateTime competencia)
    {
        var dia = transacao.DiaVencimento ?? transacao.DataInicio.Day;
        var diaAjustado = Math.Min(dia, DateTime.DaysInMonth(competencia.Year, competencia.Month));
        var data = new DateTime(competencia.Year, competencia.Month, diaAjustado);

        var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);
        if (competencia == competenciaInicio && data < transacao.DataInicio.Date)
        {
            return transacao.DataInicio.Date;
        }

        return data;
    }

    private static decimal ObterValorExibicaoTransacao(Transacao transacao)
    {
        var tipoTransacao = transacao.TipoTransacao.Trim().ToUpperInvariant();
        if (tipoTransacao == "PARCELADA" && transacao.QuantidadeParcelas.HasValue && transacao.QuantidadeParcelas > 0)
        {
            return decimal.Round(
                transacao.ValorTotal / transacao.QuantidadeParcelas.Value,
                2,
                MidpointRounding.AwayFromZero);
        }

        return transacao.ValorTotal;
    }
}
