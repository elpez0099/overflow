using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SearchService.Data;
using SearchService.Models;
using Typesense;
using Typesense.Setup;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

var typesenseUri = builder.Configuration["services:typesense:typesense:0"];
if (string.IsNullOrEmpty(typesenseUri)) throw new InvalidOperationException("Typesense URI is missing");

var uri = new Uri(typesenseUri);
var typesenseSecret = builder.Configuration["typesense-api-key"];
if (string.IsNullOrEmpty(typesenseSecret)) throw new InvalidOperationException("Typesense API KEY is missing");
builder.Services.AddTypesenseClient(config =>
{
    config.ApiKey = typesenseSecret;
    config.Nodes = new List<Node>
    {
        new (uri.Host, uri.Port.ToString(), uri.Scheme)
    };
});

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

    // Typesense registers ITypesenseClient through an opaque factory.
    // Allow Wolverine to resolve this specific dependency from DI.
    opts.CodeGeneration
        .AlwaysUseServiceLocationFor<ITypesenseClient>();

    opts.UseRabbitMqUsingNamedConnection("messaging")
        .AutoProvision();

    opts.ListenToRabbitQueue("questions.search", queue =>
    {
        queue.BindExchange("questions");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// Default endpoints for monitoring and telemetry
app.MapDefaultEndpoints();

app.MapGet("/search", async (string query, ITypesenseClient client) =>
{
    string? tag = null;
    var tagMatch = Regex.Match(query, @"\[(.*?)\]");
    if (tagMatch.Success)
    {
        tag = tagMatch.Groups[1].Value;
        query = query.Replace(tagMatch.Value, "").Trim();
    }

    var searchParameters = new SearchParameters(query, "title,content");
    if(!string.IsNullOrWhiteSpace(tag))
    {
        searchParameters.FilterBy = $"tags:=[{tag}]";
    }

    try
    {
        var results = await client.Search<SearchQuestion>("questions", searchParameters);
        return Results.Ok(results.Hits.Select(x=> x.Document));
    }
    catch (Exception e)
    {
        return Results.Problem("Search has failed: ", e.Message);
    }
});

app.MapGet("/search/similar-titles", async (string query, ITypesenseClient client) =>
{
    var searchParameters = new SearchParameters(query, "title"); 
    try
    {
        var result = await client.Search<SearchQuestion>("questions", searchParameters);
        return Results.Ok(result.Hits.Select(x=> x.Document));
    }
    catch (Exception e)
    {
        return Results.Problem("Search has failed: ", e.Message);
    }
});



using var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<ITypesenseClient>();
await SearchInitializer.EnsureIndexExistsAsync(client);

app.Run();