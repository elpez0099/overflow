var builder = DistributedApplication.CreateBuilder(args);
#pragma warning disable ASPIRECERTIFICATES001

// Docker Compose Generation
var dockerCompose = builder.AddDockerComposeEnvironment("production")
    .WithDashboard(cfg => cfg.WithHostPort(8080));

// Keycloak Authentication Service
var keycloak = builder
    .AddKeycloak("keycloak", 6001)
    .WithDataVolume("keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEndpoint(6001,8080, "keycloak", isExternal:true)
    .WithRealmImport("../infra/realms");

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
    .WithEnvironment("TYPESENSE_DATA_DIR", "/data")
    .WithEnvironment("TYPESENSE_ENABLE_CORS", "true")
    .WithEnvironment("TYPESENSE_API_KEY", typesenseSecret)
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

// Reverse Proxy for services
var yarp = builder.AddYarp("gateway")
    .WithConfiguration(cfg =>
    {
        cfg.AddRoute("/questions/{**catch-all}", questionService);
        cfg.AddRoute("/tags/{**catch-all}", questionService);
        cfg.AddRoute("/search/{**catch-all}", searchService);
    })
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8001")
    .WithEndpoint(8001, 8001, scheme: "http", name: "gateway", isExternal: true);

builder.Build().Run();