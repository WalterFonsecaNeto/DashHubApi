using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.Core.Entities;
using DashHubApi.DTOs.Categoria;
using DashHubApi.Infrastructure.Repositories.Interfaces;

namespace DashHubApi.Application.Services;

public class CategoriaService : IServicoCategoria
{
    private readonly IRepositorioCategoria _repositorioCategoria;

    public CategoriaService(IRepositorioCategoria repositorioCategoria)
    {
        _repositorioCategoria = repositorioCategoria;
    }

    public async Task<IEnumerable<RespostaCategoriaDto>> ObterTodosAsync(int userId)
    {
        var categorias = await _repositorioCategoria.ObterPorIdUsuarioAsync(userId);
        return categorias.Select(c => new RespostaCategoriaDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Tipo = c.Tipo
        });
    }

    public async Task<RespostaCategoriaDto> CriarAsync(int userId, RequisicaoCategoriaDto dto)
    {
        ValidarCategoria(dto);

        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Tipo = NormalizarTipo(dto.Tipo),
            UsuarioId = userId
        };

        var created = await _repositorioCategoria.CriarAsync(categoria);

        return new RespostaCategoriaDto
        {
            Id = created.Id,
            Nome = created.Nome,
            Tipo = created.Tipo
        };
    }

    public async Task<RespostaCategoriaDto> AtualizarAsync(int userId, int id, RequisicaoCategoriaDto dto)
    {
        ValidarCategoria(dto);

        var existing = await _repositorioCategoria.ObterPorIdEIdUsuarioAsync(id, userId)
            ?? throw new ExcecaoApi("Categoria nao encontrada", 404);

        existing.Nome = dto.Nome.Trim();
        existing.Tipo = NormalizarTipo(dto.Tipo);

        var updated = await _repositorioCategoria.AtualizarAsync(existing);
        if (!updated)
        {
            throw new ExcecaoApi("Nao foi possivel atualizar a categoria", 500);
        }

        return new RespostaCategoriaDto
        {
            Id = existing.Id,
            Nome = existing.Nome,
            Tipo = existing.Tipo
        };
    }

    public async Task DeletarAsync(int userId, int id)
    {
        var deleted = await _repositorioCategoria.DeletarAsync(id, userId);
        if (!deleted)
        {
            throw new ExcecaoApi("Categoria nao encontrada", 404);
        }
    }

    private static void ValidarCategoria(RequisicaoCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new ExcecaoApi("Nome da categoria e obrigatorio");
        }

        _ = NormalizarTipo(dto.Tipo);
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
}
