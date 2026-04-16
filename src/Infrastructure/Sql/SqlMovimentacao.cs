namespace DashHubApi.Infrastructure.Sql;

/// <summary>
/// Consultas SQL para operações com Movimentações
/// </summary>
public static class SqlMovimentacao
{
    public const string ObterPorIdEIdUsuario = @"
        SELECT
            m.id AS Id,
            m.descricao AS Descricao,
            m.valor AS Valor,
            m.tipo AS Tipo,
            m.data_movimentacao AS DataMovimentacao,
            m.categoria_id AS CategoriaId,
            m.usuario_id AS UsuarioId,
            m.transacao_id AS TransacaoId,
            m.status_pagamento AS StatusPagamento,
            m.data_pagamento AS DataPagamento,
            c.nome AS CategoriaNome
        FROM movimentacoes m
        INNER JOIN categorias c ON c.id = m.categoria_id
        WHERE m.id = @Id AND m.usuario_id = @UserId
        LIMIT 1;";

    public const string Criar = @"
        INSERT INTO movimentacoes (descricao, valor, tipo, data_movimentacao, categoria_id, usuario_id, transacao_id, status_pagamento, data_pagamento)
        VALUES (@Descricao, @Valor, @Tipo, @DataMovimentacao, @CategoriaId, @UsuarioId, @TransacaoId, @StatusPagamento, @DataPagamento);
        SELECT LAST_INSERT_ID();";

    public const string ExisteNoMes = @"
        SELECT COUNT(1)
        FROM movimentacoes
        WHERE transacao_id = @TransacaoId
          AND MONTH(data_movimentacao) = MONTH(@Data)
          AND YEAR(data_movimentacao) = YEAR(@Data);";

    public const string Atualizar = @"
        UPDATE movimentacoes
        SET
            descricao = @Descricao,
            valor = @Valor,
            tipo = @Tipo,
            data_movimentacao = @DataMovimentacao,
            categoria_id = @CategoriaId,
            status_pagamento = @StatusPagamento,
            data_pagamento = @DataPagamento
        WHERE id = @Id AND usuario_id = @UsuarioId;";

    public const string AtualizarStatus = @"
        UPDATE movimentacoes
        SET
            status_pagamento = @StatusPagamento,
            data_pagamento = @DataPagamento
        WHERE id = @Id AND usuario_id = @UsuarioId;";

    public const string Deletar = "DELETE FROM movimentacoes WHERE id = @Id AND usuario_id = @UserId;";

    public const string DeletarPorTransacao = @"
        DELETE FROM movimentacoes
        WHERE transacao_id = @TransacaoId
          AND usuario_id = @UserId;";

    public const string DeletarTodas = @"
        DELETE FROM movimentacoes
        WHERE usuario_id = @UserId;";

    public const string ObterResumo = @"
        SELECT
            COALESCE(SUM(CASE WHEN tipo = 'receita' THEN valor ELSE 0 END), 0) AS TotalReceitas,
            COALESCE(SUM(CASE WHEN tipo = 'despesa' THEN valor ELSE 0 END), 0) AS TotalDespesas
        FROM movimentacoes
        WHERE usuario_id = @UserId
          AND (@DataInicio IS NULL OR data_movimentacao >= @DataInicio)
          AND (@DataFim IS NULL OR data_movimentacao <= @DataFim);";

        public const string ObterDistribuicaoCategorias = @"
                SELECT
                        COALESCE(c.nome, 'Sem categoria') AS Categoria,
                        m.tipo AS Tipo,
                        COALESCE(SUM(m.valor), 0) AS Valor
                FROM movimentacoes m
                LEFT JOIN categorias c ON c.id = m.categoria_id
                WHERE m.usuario_id = @UserId
                    AND m.data_movimentacao >= @DataInicio
                    AND m.data_movimentacao <= @DataFim
                GROUP BY c.nome, m.tipo
                ORDER BY Valor DESC;";

        public const string ObterTopCategorias = @"
                SELECT
                        COALESCE(c.nome, 'Sem categoria') AS Categoria,
                        COALESCE(SUM(m.valor), 0) AS TotalGasto,
                        COUNT(1) AS NumeroParcelas
                FROM movimentacoes m
                LEFT JOIN categorias c ON c.id = m.categoria_id
                WHERE m.usuario_id = @UserId
                    AND m.tipo = 'despesa'
                    AND m.data_movimentacao >= @DataInicio
                    AND m.data_movimentacao <= @DataFim
                GROUP BY c.nome
                ORDER BY TotalGasto DESC
                LIMIT @Limite;";

    // Consultas para paginação com filtros dinâmicos
    public const string ContarPorIdUsuarioModelo = @"
        SELECT COUNT(1)
        FROM movimentacoes m
        {0};";

    public const string ObterPorIdUsuarioComPaginacaoModelo = @"
        SELECT
            m.id AS Id,
            m.descricao AS Descricao,
            m.valor AS Valor,
            m.tipo AS Tipo,
            m.data_movimentacao AS DataMovimentacao,
            m.categoria_id AS CategoriaId,
            m.usuario_id AS UsuarioId,
            m.transacao_id AS TransacaoId,
            m.status_pagamento AS StatusPagamento,
            m.data_pagamento AS DataPagamento,
            c.nome AS CategoriaNome
        FROM movimentacoes m
        LEFT JOIN categorias c ON c.id = m.categoria_id
        {0}
        ORDER BY m.data_movimentacao {1}, m.id DESC
        LIMIT @Limit OFFSET @Offset;";

    public const string OndeIdUsuario = " WHERE m.usuario_id = @UserId ";

    public const string ObterMovimentacoesAbertasParaAlertas = @"
        SELECT
            m.id AS Id,
            m.descricao AS Descricao,
            m.valor AS Valor,
            m.tipo AS Tipo,
            m.data_movimentacao AS DataMovimentacao,
            m.categoria_id AS CategoriaId,
            m.usuario_id AS UsuarioId,
            m.transacao_id AS TransacaoId,
            m.status_pagamento AS StatusPagamento,
            m.data_pagamento AS DataPagamento,
            c.nome AS CategoriaNome
        FROM movimentacoes m
        LEFT JOIN categorias c ON c.id = m.categoria_id
        WHERE m.usuario_id = @UserId
          AND m.status_pagamento IN ('PENDENTE', 'ATRASADO')
          AND m.data_movimentacao < @DataLimiteExclusiva
        ORDER BY m.data_movimentacao ASC, m.id ASC;";
}
