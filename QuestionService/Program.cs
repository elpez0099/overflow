using Helpers;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.Services;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Adds telemetry and metrics
builder.AddServiceDefaults();

// We tell .net to use cache based on RAM memory
// The limitation is that the memory lives in a single node, it is not distributed
builder.Services.AddMemoryCache();
// En caso de querer usar redis para cache distribuido, aspire ya lo integra
// var redis = builder.AddRedis("cache");

// DI
builder.Services.AddScoped<TagService>();

// Set keycloak authentication
builder.Services.AddKeycloakAuthentication();

// Adds database initialization and migration support
builder.AddNpgsqlDbContext<QuestionDbContext>("questionDb");

// RabbitMQ Service
await builder.UseWolverineWithRabbitMqAsync(opts =>
{
    opts.PublishAllMessages().ToRabbitExchange("questions");
    opts.ApplicationAssembly = typeof(Program).Assembly;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Intended for role based access control RBAC
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Register endpoints for observability and health check of the app
app.MapDefaultEndpoints();

// app.Services representa a un contenedor de dependency injection
// al usar CreateScope, se crea un nuevo scope dentro del contenedor
// Esto permite tener control sobre el ciclo de vida del servicio
using var scope =  app.Services.CreateScope();
// Cada scope esta preparado para inyectar una instancia de un servicio
// Para ello provee un service locator que permite pedir una instancia especifica de una clase o interfaz
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<QuestionDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception e)
{
    var logger =  services.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "An error occurred seeding the DB.");
}

app.Run();