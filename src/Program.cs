using DashHubApi.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Serviços
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AdicionarConfiguracaoSwagger();
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarCors(builder.Configuration, builder.Environment);
builder.Services.AdicionarInfraestrutura();
builder.Services.AdicionarRepositorios();
builder.Services.AdicionarServicosAplicacao();

var app = builder.Build();

// Configuração do Pipeline
app.UsarConfiguracaoDesenvolvimento();
app.UsarMiddlewareAplicacao();
app.MapControllers();

app.Run();
