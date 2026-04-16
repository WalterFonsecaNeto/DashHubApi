# Deploy no EasyPanel - DashHubApi

## 1) Tipo de serviço
- Crie um App usando Dockerfile do repositório.
- Build context: raiz do projeto (onde esta o Dockerfile).
- Dockerfile path: Dockerfile
- Internal port: 8080

## 2) Variaveis de ambiente obrigatorias (PRD)
Configure no EasyPanel estas variaveis:

- ASPNETCORE_ENVIRONMENT=Production
- ConnectionStrings__DefaultConnection=server=server=painel.walterfonsecaneto.com.br;port=3307;database=dashhub_db;user=root;password=1ae2b337e3864b84b08b;Allow User Variables=True;
- Jwt__Key=COLOQUE_UMA_CHAVE_FORTE_COM_32_BYTES_OU_MAIS
- Jwt__Issuer=DashHubApi
- Jwt__Audience=DashHubApiUsers
- Jwt__ExpirationMinutes=120
- Cors__AllowedOrigins__0=https://seu-frontend.com
- Cors__AllowedOrigins__1=https://www.seu-frontend.com

## 3) Dominio e roteamento
- Vincule o dominio no EasyPanel para o servico da API.
- Mantenha HTTPS habilitado no proxy reverso da plataforma.

## 4) Health check recomendado
Use um endpoint publico da API que responda 200 para validar subida.
Se nao existir, vale criar um endpoint /health simples.

## 5) Observacoes de seguranca
- Nao use valores de exemplo para Jwt__Key em producao.
- Nao mantenha senha de banco em appsettings versionado.
- CORS em producao deve listar somente dominios do frontend.

## 6) Teste rapido pos-deploy
1. Chame o endpoint de autenticacao para obter token.
2. Chame um endpoint protegido com Bearer token.
3. Valide no browser se o frontend consegue consumir sem erro de CORS.
