namespace DashHubApi.Infrastructure.Sql;

/// <summary>
/// Consultas SQL para operações com Categorias
/// </summary>
public static class SqlCategoria
{
    public const string ObterPorIdUsuario = @"
        SELECT
            id AS Id,
            nome AS Nome,
            tipo AS Tipo,
            usuario_id AS UsuarioId
        FROM categorias
        WHERE usuario_id = @UserId
        ORDER BY nome ASC;";

    public const string ObterPorIdEIdUsuario = @"
        SELECT
            id AS Id,
            nome AS Nome,
            tipo AS Tipo,
            usuario_id AS UsuarioId
        FROM categorias
        WHERE id = @Id AND usuario_id = @UserId
        LIMIT 1;";

    public const string Criar = @"
        INSERT INTO categorias (nome, tipo, usuario_id)
        VALUES (@Nome, @Tipo, @UsuarioId);
        SELECT LAST_INSERT_ID();";

    public const string Atualizar = @"
        UPDATE categorias
        SET nome = @Nome, tipo = @Tipo
        WHERE id = @Id AND usuario_id = @UsuarioId;";

    public const string Deletar = "DELETE FROM categorias WHERE id = @Id AND usuario_id = @UserId;";
}
