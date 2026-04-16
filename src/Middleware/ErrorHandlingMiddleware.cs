using System.Net;
using System.Text.Json;
using DashHubApi.Application.Common;

namespace DashHubApi.Middleware;

public class MiddlewareGerenciamentoErro
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareGerenciamentoErro> _logger;

    public MiddlewareGerenciamentoErro(RequestDelegate next, ILogger<MiddlewareGerenciamentoErro> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _next(contexto);
        }
        catch (ExcecaoApi ex)
        {
            await EscreverErroAsync(contexto, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no processamento da requisição");
            await EscreverErroAsync(contexto, (int)HttpStatusCode.InternalServerError, "Erro interno do servidor");
        }
    }

    private static async Task EscreverErroAsync(HttpContext contexto, int statusCode, string message)
    {
        contexto.Response.StatusCode = statusCode;
        contexto.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode
        });

        await contexto.Response.WriteAsync(payload);
    }
}
