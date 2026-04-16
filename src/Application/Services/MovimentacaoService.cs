using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.Core.Entities;
using DashHubApi.Core.Enums;
using DashHubApi.DTOs.Common;
using DashHubApi.DTOs.Movimentacao;
using DashHubApi.Infrastructure.Repositories.Interfaces;

namespace DashHubApi.Application.Services;

public class MovimentacaoService : IServicoMovimentacao
{
    private readonly IRepositorioMovimentacao _repositorioMovimentacao;
    private readonly IRepositorioCategoria _repositorioCategoria;
    private readonly IRepositorioTransacao _repositorioTransacao;

    public MovimentacaoService(
        IRepositorioMovimentacao repositorioMovimentacao,
        IRepositorioCategoria repositorioCategoria,
        IRepositorioTransacao repositorioTransacao)
    {
        _repositorioMovimentacao = repositorioMovimentacao;
        _repositorioCategoria = repositorioCategoria;
        _repositorioTransacao = repositorioTransacao;
    }

    public async Task<ResultadoPaginadoDto<RespostaMovimentacaoDto>> ObterTodosAsync(int userId, ConsultaMovimentacaoDto query)
    {
        var pagina = query.Pagina < 1 ? 1 : query.Pagina;
        var tamanhoPagina = query.TamanhoPagina switch
        {
            <= 0 => 10,
            > 100 => 100,
            _ => query.TamanhoPagina
        };

        if (query.DataInicio.HasValue && query.DataFim.HasValue && query.DataInicio > query.DataFim)
        {
            throw new ExcecaoApi("DataInicio nao pode ser maior que DataFim");
        }

        var dataInicio = query.DataInicio;
        var dataFim = query.DataFim;

        if (!dataInicio.HasValue && !dataFim.HasValue)
        {
            var hoje = DateTime.Today;
            dataInicio = new DateTime(hoje.Year, hoje.Month, 1);
            dataFim = dataInicio.Value.AddMonths(1).AddDays(-1);
        }

        var (itens, total) = await _repositorioMovimentacao.ObterPorIdUsuarioComPaginacaoAsync(
            userId,
            dataInicio,
            dataFim,
            pagina,
            tamanhoPagina,
            true);

        var responseItens = itens.Select(ParaResposta).ToList();
        var totalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina);

        return new ResultadoPaginadoDto<RespostaMovimentacaoDto>
        {
            Itens = responseItens,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = total,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<ResultadoCriacaoMovimentacaoDto> CriarAsync(int userId, CriarMovimentacaoDto dto)
    {
        await ValidarCriacaoMovimentacao(dto, userId);

        var tipo = NormalizarTipo(dto.Tipo);
        var tipoTransacao = NormalizarTipoTransacao(dto.TipoTransacao);
        var transacao = new Transacao
        {
            Descricao = dto.Descricao.Trim(),
            ValorTotal = dto.Valor,
            Tipo = tipoTransacao == "RECORRENTE" ? tipo.ToUpperInvariant() : tipo.ToUpperInvariant(),
            TipoTransacao = tipoTransacao,
            QuantidadeParcelas = dto.QuantidadeParcelas,
            ParcelasGeradas = 0,
            DiaVencimento = dto.DiaVencimento,
            DataInicio = dto.DataInicio.Date,
            DataFim = dto.DataFim?.Date,
            UsuarioId = userId,
            CategoriaId = dto.CategoriaId,
            Ativa = tipoTransacao is "PARCELADA" or "RECORRENTE"
        };

        transacao = await _repositorioTransacao.CriarAsync(transacao);

        if (tipoTransacao == "RECORRENTE")
        {
            RespostaMovimentacaoDto? movimentacaoInicial = null;

            var hoje = DateTime.Today;
            var competenciaAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var competenciaInicio = new DateTime(transacao.DataInicio.Year, transacao.DataInicio.Month, 1);
            var competenciaFim = transacao.DataFim.HasValue
                ? new DateTime(transacao.DataFim.Value.Year, transacao.DataFim.Value.Month, 1)
                : DateTime.MaxValue;

            if (competenciaAtual < competenciaInicio)
            {
                return new ResultadoCriacaoMovimentacaoDto
                {
                    TransacaoId = transacao.Id,
                    TipoTransacao = tipoTransacao,
                    MovimentacaoInicial = null
                };
            }

            for (var competencia = competenciaInicio;
                 competencia <= competenciaAtual && competencia <= competenciaFim;
                 competencia = competencia.AddMonths(1))
            {
                var dia = transacao.DiaVencimento ?? transacao.DataInicio.Day;
                var diaAjustado = Math.Min(dia, DateTime.DaysInMonth(competencia.Year, competencia.Month));
                var dataDoMes = new DateTime(competencia.Year, competencia.Month, diaAjustado);

                if (competencia == competenciaInicio && dataDoMes < transacao.DataInicio.Date)
                {
                    dataDoMes = transacao.DataInicio.Date;
                }

                var jaExiste = await _repositorioMovimentacao.ExisteNoMesAsync(transacao.Id, dataDoMes);
                if (jaExiste)
                {
                    continue;
                }

                var execucao = new Movimentacao
                {
                    Descricao = transacao.Descricao,
                    Valor = transacao.ValorTotal,
                    Tipo = tipo,
                    DataMovimentacao = dataDoMes,
                    CategoriaId = transacao.CategoriaId,
                    UsuarioId = transacao.UsuarioId,
                    TransacaoId = transacao.Id
                };

                AplicarStatusPagamento(execucao, dto.StatusPagamento, dto.DataPagamento, false);

                var criada = await _repositorioMovimentacao.CriarAsync(execucao);
                if (competencia == competenciaAtual)
                {
                    movimentacaoInicial = ParaResposta(criada);
                }
            }

            return new ResultadoCriacaoMovimentacaoDto
            {
                TransacaoId = transacao.Id,
                TipoTransacao = tipoTransacao,
                MovimentacaoInicial = movimentacaoInicial
            };
        }

        if (tipoTransacao == "PARCELADA")
        {
            var valorParcela = decimal.Round(
                dto.Valor / dto.QuantidadeParcelas!.Value,
                2,
                MidpointRounding.AwayFromZero);

            var quantidadeParcelas = dto.QuantidadeParcelas.Value;
            var parcelasGeradas = 0;
            var competenciaAtual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            RespostaMovimentacaoDto? movimentacaoInicial = null;

            while (parcelasGeradas < quantidadeParcelas)
            {
                var dataParcela = transacao.DataInicio.Date.AddMonths(parcelasGeradas);
                var competenciaParcela = new DateTime(dataParcela.Year, dataParcela.Month, 1);
                if (competenciaParcela > competenciaAtual)
                {
                    break;
                }

                var jaExiste = await _repositorioMovimentacao.ExisteNoMesAsync(transacao.Id, dataParcela);
                if (!jaExiste)
                {
                    var movimentacaoParcela = new Movimentacao
                    {
                        Descricao = transacao.Descricao,
                        Valor = valorParcela,
                        Tipo = tipo,
                        DataMovimentacao = dataParcela,
                        CategoriaId = transacao.CategoriaId,
                        UsuarioId = transacao.UsuarioId,
                        TransacaoId = transacao.Id
                    };

                    AplicarStatusPagamento(movimentacaoParcela, dto.StatusPagamento, dto.DataPagamento, false);

                    var criada = await _repositorioMovimentacao.CriarAsync(movimentacaoParcela);
                    if (competenciaParcela == competenciaAtual || movimentacaoInicial is null)
                    {
                        movimentacaoInicial = ParaResposta(criada);
                    }
                }

                parcelasGeradas++;
            }

            var ativa = parcelasGeradas < quantidadeParcelas;
            if (parcelasGeradas != transacao.ParcelasGeradas || !ativa)
            {
                await _repositorioTransacao.AtualizarParcelasGeradasEAtivaAsync(transacao.Id, parcelasGeradas, ativa);
            }

            return new ResultadoCriacaoMovimentacaoDto
            {
                TransacaoId = transacao.Id,
                TipoTransacao = tipoTransacao,
                MovimentacaoInicial = movimentacaoInicial
            };
        }

        var valorMovimentacao = dto.Valor;

        var movimentacao = new Movimentacao
        {
            Descricao = dto.Descricao.Trim(),
            Valor = valorMovimentacao,
            Tipo = tipo,
            DataMovimentacao = dto.DataInicio.Date,
            CategoriaId = dto.CategoriaId,
            UsuarioId = userId,
            TransacaoId = transacao.Id
        };

        AplicarStatusPagamento(movimentacao, dto.StatusPagamento, dto.DataPagamento, false);

        var created = await _repositorioMovimentacao.CriarAsync(movimentacao);

        if (tipoTransacao == "UNICA")
        {
            await _repositorioTransacao.AtualizarParcelasGeradasEAtivaAsync(transacao.Id, 1, false);
        }

        return new ResultadoCriacaoMovimentacaoDto
        {
            TransacaoId = transacao.Id,
            TipoTransacao = tipoTransacao,
            MovimentacaoInicial = ParaResposta(created)
        };
    }

    public async Task<RespostaMovimentacaoDto> AtualizarAsync(int userId, int id, RequisicaoMovimentacaoDto dto)
    {
        await ValidarMovimentacao(dto, userId);

        var existing = await _repositorioMovimentacao.ObterPorIdEIdUsuarioAsync(id, userId)
            ?? throw new ExcecaoApi("Movimentacao nao encontrada", 404);

        existing.Descricao = dto.Descricao.Trim();
        existing.Valor = dto.Valor;
        existing.Tipo = NormalizarTipo(dto.Tipo);
        existing.DataMovimentacao = ObterDataMovimentacaoAtualizacao(dto);
        existing.CategoriaId = dto.CategoriaId;
        AplicarStatusPagamento(existing, dto.StatusPagamento, dto.DataPagamento, true);

        var updated = await _repositorioMovimentacao.AtualizarAsync(existing);
        if (!updated)
        {
            throw new ExcecaoApi("Nao foi possivel atualizar a movimentacao", 500);
        }

        var reloaded = await _repositorioMovimentacao.ObterPorIdEIdUsuarioAsync(id, userId)
            ?? throw new ExcecaoApi("Movimentacao nao encontrada", 404);

        return ParaResposta(reloaded);
    }

    public async Task<RespostaMovimentacaoDto> AtualizarStatusAsync(int userId, int id, AtualizarStatusMovimentacaoDto dto)
    {
        var existing = await _repositorioMovimentacao.ObterPorIdEIdUsuarioAsync(id, userId)
            ?? throw new ExcecaoApi("Movimentacao nao encontrada", 404);

        AplicarStatusPagamento(existing, dto.StatusPagamento, dto.DataPagamento, false);

        var updated = await _repositorioMovimentacao.AtualizarStatusAsync(existing);
        if (!updated)
        {
            throw new ExcecaoApi("Nao foi possivel atualizar a movimentacao", 500);
        }

        var reloaded = await _repositorioMovimentacao.ObterPorIdEIdUsuarioAsync(id, userId)
            ?? throw new ExcecaoApi("Movimentacao nao encontrada", 404);

        return ParaResposta(reloaded);
    }

    public async Task DeletarAsync(int userId, int id)
    {
        var movimentacao = await _repositorioMovimentacao.ObterPorIdEIdUsuarioAsync(id, userId);
        if (movimentacao is null)
        {
            throw new ExcecaoApi("Movimentacao nao encontrada", 404);
        }

        var deleted = await _repositorioMovimentacao.DeletarAsync(id, userId);
        if (!deleted)
        {
            throw new ExcecaoApi("Movimentacao nao encontrada", 404);
        }

        if (!movimentacao.TransacaoId.HasValue)
        {
            return;
        }

        var transacao = await _repositorioTransacao.ObterPorIdEIdUsuarioAsync(movimentacao.TransacaoId.Value, userId);
        if (transacao is null)
        {
            return;
        }

        var tipoTransacao = transacao.TipoTransacao.Trim().ToUpperInvariant();
        if ((tipoTransacao == "RECORRENTE" || tipoTransacao == "PARCELADA") && transacao.Ativa)
        {
            await _repositorioTransacao.AtualizarParcelasGeradasEAtivaAsync(
                transacao.Id,
                transacao.ParcelasGeradas,
                false);
        }
    }

    public async Task DeletarPorTransacaoAsync(int userId, int transacaoId)
    {
        var transacao = await _repositorioTransacao.ObterPorIdEIdUsuarioAsync(transacaoId, userId);
        if (transacao is null)
        {
            throw new ExcecaoApi("Transacao nao encontrada", 404);
        }

        await _repositorioTransacao.AtualizarParcelasGeradasEAtivaAsync(
            transacao.Id,
            transacao.ParcelasGeradas,
            false);

        await _repositorioMovimentacao.DeletarPorTransacaoAsync(transacao.Id, userId);
    }

    public async Task DeletarTodasAsync(int userId)
    {
        await _repositorioTransacao.DesativarTodasAsync(userId);
        await _repositorioMovimentacao.DeletarTodasAsync(userId);
    }

    private async Task ValidarMovimentacao(RequisicaoMovimentacaoDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new ExcecaoApi("Descricao e obrigatoria");
        }

        if (dto.Valor < 0)
        {
            throw new ExcecaoApi("Valor nao pode ser negativo");
        }

        _ = ObterDataMovimentacaoAtualizacao(dto);

        _ = NormalizarTipo(dto.Tipo);

        var categoria = await _repositorioCategoria.ObterPorIdEIdUsuarioAsync(dto.CategoriaId, userId);
        if (categoria is null)
        {
            throw new ExcecaoApi("Categoria nao encontrada para este usuario", 404);
        }
    }

    private static void AplicarStatusPagamento(Movimentacao movimentacao, StatusPagamento? statusPagamento, DateTime? dataPagamento, bool manterStatusAtual)
    {
        var statusFinal = statusPagamento ?? (manterStatusAtual ? movimentacao.StatusPagamento : StatusPagamento.PENDENTE);
        movimentacao.StatusPagamento = statusFinal;

        if (statusFinal == StatusPagamento.PAGO)
        {
            movimentacao.DataPagamento = dataPagamento ?? movimentacao.DataPagamento ?? DateTime.Now;
            return;
        }

        if (statusPagamento.HasValue || !manterStatusAtual)
        {
            movimentacao.DataPagamento = null;
        }
    }

    private static DateTime ObterDataMovimentacaoAtualizacao(RequisicaoMovimentacaoDto dto)
    {
        var data = dto.DataMovimentacao ?? dto.DataInicio;
        if (!data.HasValue || data.Value == default)
        {
            throw new ExcecaoApi("DataMovimentacao ou DataInicio e obrigatoria");
        }

        return data.Value;
    }

    private async Task ValidarCriacaoMovimentacao(CriarMovimentacaoDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new ExcecaoApi("Descricao e obrigatoria");
        }

        if (dto.Valor <= 0)
        {
            throw new ExcecaoApi("Valor deve ser maior que zero");
        }

        _ = NormalizarTipo(dto.Tipo);
        var tipoTransacao = NormalizarTipoTransacao(dto.TipoTransacao);

        if (tipoTransacao == "PARCELADA" && (!dto.QuantidadeParcelas.HasValue || dto.QuantidadeParcelas.Value <= 0))
        {
            throw new ExcecaoApi("QuantidadeParcelas deve ser maior que zero para transacao parcelada");
        }

        if (dto.DataFim.HasValue && dto.DataFim.Value.Date < dto.DataInicio.Date)
        {
            throw new ExcecaoApi("DataFim nao pode ser menor que DataInicio");
        }

        if (dto.DiaVencimento.HasValue && (dto.DiaVencimento.Value < 1 || dto.DiaVencimento.Value > 31))
        {
            throw new ExcecaoApi("DiaVencimento deve estar entre 1 e 31");
        }

        var categoria = await _repositorioCategoria.ObterPorIdEIdUsuarioAsync(dto.CategoriaId, userId);
        if (categoria is null)
        {
            throw new ExcecaoApi("Categoria nao encontrada para este usuario", 404);
        }
    }

    private static string NormalizarTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ExcecaoApi("Tipo e obrigatorio");
        }

        var normalized = tipo.Trim().ToLowerInvariant();
        if (normalized is not ("receita" or "despesa"))
        {
            throw new ExcecaoApi("Tipo deve ser receita ou despesa");
        }

        return normalized;
    }

    private static string NormalizarTipoTransacao(string tipoTransacao)
    {
        if (string.IsNullOrWhiteSpace(tipoTransacao))
        {
            throw new ExcecaoApi("TipoTransacao e obrigatorio");
        }

        var normalizado = tipoTransacao.Trim().ToUpperInvariant();
        if (normalizado is not ("UNICA" or "PARCELADA" or "RECORRENTE"))
        {
            throw new ExcecaoApi("TipoTransacao deve ser UNICA, PARCELADA ou RECORRENTE");
        }

        return normalizado;
    }

    private static RespostaMovimentacaoDto ParaResposta(Movimentacao m)
    {
        return new RespostaMovimentacaoDto
        {
            Id = m.Id,
            Descricao = m.Descricao,
            Valor = m.Valor,
            Tipo = m.Tipo,
            DataMovimentacao = m.DataMovimentacao,
            DataInicio = m.DataMovimentacao,
            CategoriaId = m.CategoriaId,
            TransacaoId = m.TransacaoId,
            StatusPagamento = m.StatusPagamento,
            DataPagamento = m.DataPagamento,
            CategoriaNome = m.CategoriaNome
        };
    }
}
