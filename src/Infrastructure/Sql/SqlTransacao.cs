namespace DashHubApi.Infrastructure.Sql;

public static class SqlTransacao
{
    public const string Criar = @"
        INSERT INTO transacoes (
            descricao,
            valor_total,
            tipo,
            tipo_transacao,
            quantidade_parcelas,
            parcelas_geradas,
            dia_vencimento,
            data_inicio,
            data_fim,
            usuario_id,
            categoria_id,
            ativa)
        VALUES (
            @Descricao,
            @ValorTotal,
            @Tipo,
            @TipoTransacao,
            @QuantidadeParcelas,
            @ParcelasGeradas,
            @DiaVencimento,
            @DataInicio,
            @DataFim,
            @UsuarioId,
            @CategoriaId,
            @Ativa);
        SELECT LAST_INSERT_ID();";

    public const string ObterAtivas = @"
        SELECT
            id AS Id,
            descricao AS Descricao,
            valor_total AS ValorTotal,
            tipo AS Tipo,
            tipo_transacao AS TipoTransacao,
            quantidade_parcelas AS QuantidadeParcelas,
            parcelas_geradas AS ParcelasGeradas,
            dia_vencimento AS DiaVencimento,
            data_inicio AS DataInicio,
            data_fim AS DataFim,
            usuario_id AS UsuarioId,
            categoria_id AS CategoriaId,
            ativa AS Ativa,
            data_criacao AS DataCriacao
        FROM transacoes
        WHERE ativa = 1;";

    public const string ObterAtivasPorUsuario = @"
        SELECT
            id AS Id,
            descricao AS Descricao,
            valor_total AS ValorTotal,
            tipo AS Tipo,
            tipo_transacao AS TipoTransacao,
            quantidade_parcelas AS QuantidadeParcelas,
            parcelas_geradas AS ParcelasGeradas,
            dia_vencimento AS DiaVencimento,
            data_inicio AS DataInicio,
            data_fim AS DataFim,
            usuario_id AS UsuarioId,
            categoria_id AS CategoriaId,
            ativa AS Ativa,
            data_criacao AS DataCriacao
        FROM transacoes
        WHERE ativa = 1
          AND usuario_id = @UserId;";

    public const string ObterPorIdEIdUsuario = @"
        SELECT
            id AS Id,
            descricao AS Descricao,
            valor_total AS ValorTotal,
            tipo AS Tipo,
            tipo_transacao AS TipoTransacao,
            quantidade_parcelas AS QuantidadeParcelas,
            parcelas_geradas AS ParcelasGeradas,
            dia_vencimento AS DiaVencimento,
            data_inicio AS DataInicio,
            data_fim AS DataFim,
            usuario_id AS UsuarioId,
            categoria_id AS CategoriaId,
            ativa AS Ativa,
            data_criacao AS DataCriacao
        FROM transacoes
        WHERE id = @TransacaoId
          AND usuario_id = @UserId
        LIMIT 1;";

    public const string AtualizarParcelasGeradasEAtiva = @"
        UPDATE transacoes
        SET parcelas_geradas = @ParcelasGeradas,
            ativa = @Ativa
        WHERE id = @TransacaoId;";

    public const string DesativarTodas = @"
        UPDATE transacoes
        SET ativa = 0
        WHERE usuario_id = @UserId;";
}
