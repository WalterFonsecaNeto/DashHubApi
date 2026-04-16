namespace DashHubApi.Infrastructure.Sql;

/// <summary>
/// Consultas SQL para operações com Usuários
/// </summary>
public static class SqlUsuario
{
    public const string ObterPorEmail = @"
        SELECT
            id AS Id,
            nome AS Nome,
            email AS Email,
            senha AS Senha,
            data_criacao AS DataCriacao
        FROM usuarios
        WHERE email = @Email
        LIMIT 1;";

    public const string ObterPorId = @"
        SELECT
            id AS Id,
            nome AS Nome,
            email AS Email,
            senha AS Senha,
            data_criacao AS DataCriacao
        FROM usuarios
        WHERE id = @Id
        LIMIT 1;";

    public const string Criar = @"
        INSERT INTO usuarios (nome, email, senha, data_criacao)
        VALUES (@Nome, @Email, @Senha, @DataCriacao);
        SELECT LAST_INSERT_ID();";
}
