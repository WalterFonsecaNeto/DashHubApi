CREATE TABLE transacoes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descricao VARCHAR(255),
    valor_total DECIMAL(10,2),
    tipo ENUM('RECEITA','DESPESA'),
    tipo_transacao ENUM('UNICA','PARCELADA','RECORRENTE'),
    quantidade_parcelas INT NULL,
    parcelas_geradas INT DEFAULT 0,
    dia_vencimento INT NULL,
    data_inicio DATE,
    data_fim DATE NULL,
    usuario_id INT,
    categoria_id INT,
    ativa BOOLEAN DEFAULT TRUE,
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    FOREIGN KEY (categoria_id) REFERENCES categorias(id)
);

ALTER TABLE movimentacoes
ADD COLUMN transacao_id INT NULL,
ADD FOREIGN KEY (transacao_id) REFERENCES transacoes(id);

ALTER TABLE movimentacoes
    ADD COLUMN status_pagamento ENUM('PENDENTE','PAGO','ATRASADO','CANCELADO') NOT NULL DEFAULT 'PENDENTE'
    AFTER transacao_id;

ALTER TABLE movimentacoes
    ADD COLUMN data_pagamento DATETIME NULL
    AFTER status_pagamento;

CREATE INDEX idx_movimentacoes_status_usuario_data
    ON movimentacoes (usuario_id, status_pagamento, data_movimentacao);

CREATE INDEX idx_movimentacoes_usuario_status
    ON movimentacoes (usuario_id, status_pagamento);
