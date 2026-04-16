using DashHubApi.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace DashHubApi.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configura o pipeline de desenvolvimento (Swagger e ferramentas de debug)
    /// </summary>
    public static WebApplication UsarConfiguracaoDesenvolvimento(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    /// <summary>
    /// Configura o middleware e segurança da aplicação
    /// </summary>
    public static WebApplication UsarMiddlewareAplicacao(this WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseHttpsRedirection();
        app.UseMiddleware<MiddlewareGerenciamentoErro>();
        app.UseCors(ServiceCollectionExtensions.PoliticaCorsConfigurada);
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
