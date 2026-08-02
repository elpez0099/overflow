using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QuestionService.Data;
using QuestionService.Services;
using Wolverine;
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

builder.Services.AddAuthentication().AddKeycloakJwtBearer(serviceName: "keycloak", realm: "overflow", options =>
{
    options.RequireHttpsMetadata = false;
    options.Audience = "overflow";
});

// Adds database initialization and migration support
builder.AddNpgsqlDbContext<QuestionDbContext>("questionDb");

// Adds Open Telemetry for RabbitMQ
builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
{
    traceProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
        .AddSource("Wolverine");
});

// RabbitMQ Service
builder.Host.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();
    opts.PublishAllMessages().ToRabbitExchange("questions");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Intended for role based access control RBAC
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