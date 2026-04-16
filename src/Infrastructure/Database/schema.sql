CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(120) NOT NULL,
    email VARCHAR(180) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL,
    data_criacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS categorias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(120) NOT NULL,
    tipo ENUM('receita', 'despesa') NOT NULL,
    usuario_id INT NOT NULL,
    CONSTRAINT fk_categorias_usuarios
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS transacoes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descricao VARCHAR(255) NOT NULL,
    valor_total DECIMAL(10,2) NOT NULL,
    tipo ENUM('RECEITA','DESPESA') NOT NULL,
    tipo_transacao ENUM('UNICA','PARCELADA','RECORRENTE') NOT NULL,
    quantidade_parcelas INT NULL,
    parcelas_geradas INT NOT NULL DEFAULT 0,
    dia_vencimento INT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NULL,
    usuario_id INT NOT NULL,
    categoria_id INT NOT NULL,
    ativa BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_transacoes_usuarios
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_transacoes_categorias
        FOREIGN KEY (categoria_id) REFERENCES categorias(id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS movimentacoes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descricao VARCHAR(255) NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    tipo ENUM('receita', 'despesa') NOT NULL,
    data_movimentacao DATETIME NOT NULL,
    categoria_id INT NOT NULL,
    usuario_id INT NOT NULL,
    transacao_id INT NULL,
    status_pagamento ENUM('PENDENTE','PAGO','ATRASADO','CANCELADO') NOT NULL DEFAULT 'PENDENTE',
    data_pagamento DATETIME NULL,
    CONSTRAINT fk_movimentacoes_categorias
        FOREIGN KEY (categoria_id) REFERENCES categorias(id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_movimentacoes_usuarios
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_movimentacoes_transacoes
        FOREIGN KEY (transacao_id) REFERENCES transacoes(id)
        ON DELETE SET NULL
);

CREATE INDEX idx_categorias_usuario_id ON categorias(usuario_id);
CREATE INDEX idx_movimentacoes_usuario_data ON movimentacoes(usuario_id, data_movimentacao);
CREATE INDEX idx_movimentacoes_status_usuario_data ON movimentacoes(usuario_id, status_pagamento, data_movimentacao);
CREATE INDEX idx_movimentacoes_usuario_status ON movimentacoes(usuario_id, status_pagamento);
CREATE INDEX idx_movimentacoes_transacao_id ON movimentacoes(transacao_id);
CREATE INDEX idx_transacoes_ativa_tipo ON transacoes(ativa, tipo_transacao);