using DashHubApi.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace DashHubApi.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configura o Swagger conforme ambiente ou chave de configuracao.
    /// </summary>
    public static WebApplication UsarConfiguracaoSwagger(this WebApplication app)
    {
        var swaggerHabilitado = app.Configuration.GetValue<bool?>("Swagger:Enabled")
            ?? app.Environment.IsDevelopment();

        if (swaggerHabilitado)
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
