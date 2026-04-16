using DashHubApi.Application.Common;
using DashHubApi.Application.Interfaces;
using DashHubApi.Core.Entities;
using DashHubApi.DTOs.Auth;
using DashHubApi.Infrastructure.Repositories.Interfaces;

namespace DashHubApi.Application.Services;

public class AuthService : IServicoAutenticacao
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IServicoTokenJwt _servicoTokenJwt;

    public AuthService(IRepositorioUsuario repositorioUsuario, IServicoTokenJwt servicoTokenJwt)
    {
        _repositorioUsuario = repositorioUsuario;
        _servicoTokenJwt = servicoTokenJwt;
    }

    public async Task<RespostaAutenticacaoDto> RegistrarAsync(RequisicaoRegistroDto dto)
    {
        ValidarRegistro(dto);

        var email = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _repositorioUsuario.ObterPorEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ExcecaoApi("Email ja cadastrado", 409);
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            DataCriacao = DateTime.UtcNow
        };

        var createdUser = await _repositorioUsuario.CriarAsync(usuario);
        var tokenResult = _servicoTokenJwt.GerarToken(createdUser);

        return new RespostaAutenticacaoDto
        {
            Token = tokenResult.Token,
            ExpiraEm = tokenResult.ExpiraEm
        };
    }

    public async Task<RespostaAutenticacaoDto> EntrarAsync(RequisicaoLoginDto dto)
    {
        ValidarEntrada(dto);

        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _repositorioUsuario.ObterPorEmailAsync(email)
            ?? throw new ExcecaoApi("Credenciais invalidas", 401);

        var senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha);
        if (!senhaValida)
        {
            throw new ExcecaoApi("Credenciais invalidas", 401);
        }

        var tokenResult = _servicoTokenJwt.GerarToken(usuario);

        return new RespostaAutenticacaoDto
        {
            Token = tokenResult.Token,
            ExpiraEm = tokenResult.ExpiraEm
        };
    }

    
    private static void ValidarRegistro(RequisicaoRegistroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new ExcecaoApi("Nome e obrigatorio");
        }

        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
        {
            throw new ExcecaoApi("Email invalido");
        }

        if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
        {
            throw new ExcecaoApi("Senha deve ter ao menos 6 caracteres");
        }
    }

    private static void ValidarEntrada(RequisicaoLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ExcecaoApi("Email e obrigatorio");
        }

        if (string.IsNullOrWhiteSpace(dto.Senha))
        {
            throw new ExcecaoApi("Senha e obrigatoria");
        }
    }
}
