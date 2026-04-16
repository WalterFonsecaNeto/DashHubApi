using System.Text;
using DashHubApi.Application.Interfaces;
using DashHubApi.Application.Services;
using DashHubApi.Infrastructure.Database;
using DashHubApi.Infrastructure.Repositories;
using DashHubApi.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DashHubApi.Extensions;

public static class ServiceCollectionExtensions
{
    public const string PoliticaCorsConfigurada = "PoliticaCorsConfigurada";

    /// <summary>
    /// Configura o Swagger/OpenAPI com suporte a JWT
    /// </summary>
    public static IServiceCollection AdicionarConfiguracaoSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DashHub API",
                Version = "v1"
            });

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato: Bearer {token}",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [jwtSecurityScheme] = Array.Empty<string>()
            });
        });

        return services;
    }

    /// <summary>
    /// Configura autenticação JWT
    /// </summary>
    public static IServiceCollection AdicionarAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Jwt:Key nao configurado. Defina via appsettings ou variavel de ambiente.");
        }

        if (jwtKey.Contains("ALTERE_AQUI", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Jwt:Key nao pode usar valor padrao de placeholder.");
        }

        if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException("Jwt:Key deve ter no minimo 32 bytes para seguranca adequada.");
        }

        var jwtIssuer = configuration["Jwt:Issuer"] ?? "DashHubApi";
        var jwtAudience = configuration["Jwt:Audience"] ?? "DashHubApiUsers";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Registra todos os repositórios
    /// </summary>
    public static IServiceCollection AdicionarRepositorios(this IServiceCollection services)
    {
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();
        services.AddScoped<IRepositorioMovimentacao, RepositorioMovimentacao>();
        services.AddScoped<IRepositorioTransacao, RepositorioTransacao>();

        return services;
    }

    /// <summary>
    /// Registra todos os serviços da aplicação
    /// </summary>
    public static IServiceCollection AdicionarServicosAplicacao(this IServiceCollection services)
    {
        services.AddScoped<IServicoTokenJwt, JwtTokenService>();
        services.AddScoped<IServicoAutenticacao, AuthService>();
        services.AddScoped<IServicoUsuario, UsuarioService>();
        services.AddScoped<IServicoCategoria, CategoriaService>();
        services.AddScoped<IServicoMovimentacao, MovimentacaoService>();
        services.AddScoped<IServicoPainel, DashboardService>();
        services.AddHostedService<JobGeracaoMovimentacoesService>();

        return services;
    }

    /// <summary>
    /// Registra infraestrutura (banco de dados)
    /// </summary>
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services)
    {
        services.AddScoped<IFabricaConexaoBancoDados, FabricaConexaoMySql>();

        return services;
    }

    /// <summary>
    /// Configura CORS por lista de origens permitidas.
    /// Em desenvolvimento, quando não houver configuração, libera qualquer origem para facilitar testes locais.
    /// </summary>
    public static IServiceCollection AdicionarCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var origensConfiguradas = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Select(o => o.Trim())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PoliticaCorsConfigurada, policy =>
            {
                policy.AllowAnyMethod().AllowAnyHeader();

                if (origensConfiguradas.Length > 0)
                {
                    policy.WithOrigins(origensConfiguradas);
                    return;
                }

                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin();
                    return;
                }

                throw new InvalidOperationException("Cors:AllowedOrigins deve ser configurado em ambiente nao-desenvolvimento.");
            });
        });

        return services;
    }
}
