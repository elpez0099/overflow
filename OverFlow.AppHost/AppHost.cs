var builder = DistributedApplication.CreateBuilder(args);
#pragma warning disable ASPIRECERTIFICATES001

// Keycloak Authentication Service
var keycloak = builder
    .AddKeycloak("keycloak", 6001)
    .WithoutHttpsCertificate()
    .WithDataVolume("keycloak-data");

// Postgres Service
var postgres = builder
    .AddPostgres(name:"postgres", port:5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();
// Question DB
var questionDb = postgres.AddDatabase("questionDb");

// RabbitMQ Service
var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15672);

// Typesense service
var typesenseSecret = builder.AddParameter("typesense-api-key", secret: true);
var typesense = builder
    .AddContainer("typesense", "typesense/typesense", "29.0")
    .WithArgs("--data-dir", "/data", "--api-key", typesenseSecret, "--enable-cors")
    .WithVolume("typesense-data", "/data")
    .WithHttpEndpoint(8108, 8108, name: "typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

// Question Service depends on Keycloak and Postgres
var questionService = builder
    .AddProject<Projects.QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDb)
    .WithReference(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(questionDb)
    .WaitFor(rabbitMq);

// Search Service Registration
var searchService = builder.
    AddProject<Projects.SearchService>("search-svc")
    .WithEnvironment("typesense-api-key", typesenseSecret)
    .WithReference(typesenseContainer)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WaitFor(typesense);

builder.Build().Run();