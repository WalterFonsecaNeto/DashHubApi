using DashHubApi.Core.Entities;
using DashHubApi.Infrastructure.Repositories.Interfaces;

namespace DashHubApi.Application.Services;

public class JobGeracaoMovimentacoesService : BackgroundService
{
    private static readonly TimeSpan IntervaloExecucao = TimeSpan.FromDays(1);
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<JobGeracaoMovimentacoesService> _logger;

    public JobGeracaoMovimentacoesService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<JobGeracaoMovimentacoesService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarTransacoesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar job de geracao de movimentacoes");
            }

            await Task.Delay(IntervaloExecucao, stoppingToken);
        }
    }

    private async Task ProcessarTransacoesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repositorioTransacao = scope.ServiceProvider.GetRequiredService<IRepositorioTransacao>();
        var repositorioMovimentacao = scope.ServiceProvider.GetRequiredService<IRepositorioMovimentacao>();

        var hoje = DateTime.Today;
        var transacoes = await repositorioTransacao.ObterAtivasAsync();

        foreach (var transacao in transacoes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tipo = transacao.TipoTransacao.Trim().ToUpperInvariant();
            if (tipo == "PARCELADA")
            {
                await ProcessarParceladaAsync(repositorioTransacao, repositorioMovimentacao, transacao, hoje);
            }
            else if (tipo == "RECORRENTE")
            {
                await ProcessarRecorrenteAsync(repositorioMovimentacao, transacao, hoje);
            }
        }
    }

    private static async Task ProcessarParceladaAsync(
        IRepositorioTransacao repositorioTransacao,
        IRepositorioMovimentacao repositorioMovimentacao,
        Transacao transacao,
        DateTime hoje)
    {
        if (!transacao.QuantidadeParcelas.HasValue || transacao.QuantidadeParcelas <= 0)
        {
            return;
        }

        var valorParcela = decimal.Round(
            transacao.ValorTotal / transacao.QuantidadeParcelas.Value,
            2,
            MidpointRounding.AwayFromZero);

        var quantidadeParcelas = transacao.QuantidadeParcelas.Value;
        var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);
        var competenciaAtual = new DateTime(hoje.Year, hoje.Month, 1);

        if (competenciaAtual < competenciaInicio)
        {
            return;
        }

        var indiceParcelaAtual = ((competenciaAtual.Year - competenciaInicio.Year) * 12) +
                                 (competenciaAtual.Month - competenciaInicio.Month);

        if (indiceParcelaAtual < quantidadeParcelas)
        {
            var dataParcelaAtual = transacao.DataInicio.Date.AddMonths(indiceParcelaAtual);
            var jaExiste = await repositorioMovimentacao.ExisteNoMesAsync(transacao.Id, dataParcelaAtual);
            if (!jaExiste)
            {
                var movimentacao = new Movimentacao
                {
                    Descricao = transacao.Descricao,
                    Valor = valorParcela,
                    Tipo = transacao.Tipo.ToLowerInvariant(),
                    DataMovimentacao = dataParcelaAtual,
                    CategoriaId = transacao.CategoriaId,
                    UsuarioId = transacao.UsuarioId,
                    TransacaoId = transacao.Id
                };

                await repositorioMovimentacao.CriarAsync(movimentacao);
            }
        }

        var parcelasGeradasAtualizadas = Math.Max(
            transacao.ParcelasGeradas,
            Math.Min(indiceParcelaAtual + 1, quantidadeParcelas));

        var ativa = parcelasGeradasAtualizadas < quantidadeParcelas;
        if (parcelasGeradasAtualizadas != transacao.ParcelasGeradas || !ativa)
        {
            await repositorioTransacao.AtualizarParcelasGeradasEAtivaAsync(
                transacao.Id,
                parcelasGeradasAtualizadas,
                ativa);
        }
    }

    private static async Task ProcessarRecorrenteAsync(
        IRepositorioMovimentacao repositorioMovimentacao,
        Transacao transacao,
        DateTime hoje)
    {
        var competenciaAtual = new DateTime(hoje.Year, hoje.Month, 1);
        var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);

        if (competenciaAtual < competenciaInicio)
        {
            return;
        }

        if (transacao.DataFim.HasValue)
        {
            var competenciaFim = new DateTime(transacao.DataFim.Value.Year, transacao.DataFim.Value.Month, 1);
            if (competenciaAtual > competenciaFim)
            {
                return;
            }
        }

        var dia = transacao.DiaVencimento ?? transacao.DataInicio.Day;
        var diaAjustado = Math.Min(dia, DateTime.DaysInMonth(hoje.Year, hoje.Month));
        var dataDoMes = new DateTime(hoje.Year, hoje.Month, diaAjustado);

        // No mês de início, só gera se a data calculada não ficar antes da data_inicio.
        if (competenciaAtual == competenciaInicio && dataDoMes < transacao.DataInicio.Date)
        {
            dataDoMes = transacao.DataInicio.Date;
        }

        var jaExiste = await repositorioMovimentacao.ExisteNoMesAsync(transacao.Id, dataDoMes);
        if (jaExiste)
        {
            return;
        }

        var movimentacao = new Movimentacao
        {
            Descricao = transacao.Descricao,
            Valor = transacao.ValorTotal,
            Tipo = transacao.Tipo.ToLowerInvariant(),
            DataMovimentacao = dataDoMes,
            CategoriaId = transacao.CategoriaId,
            UsuarioId = transacao.UsuarioId,
            TransacaoId = transacao.Id
        };

        await repositorioMovimentacao.CriarAsync(movimentacao);
    }
}
