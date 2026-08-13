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

// DI
builder.Services.AddMemoryCache();
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

using var scope =  app.Services.CreateScope();
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