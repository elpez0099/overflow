using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
#pragma warning disable ASPIRECERTIFICATES001

// Docker Compose Generation for production environment
// This generates a deploy based on a docker-compose.yaml with name "production"
// It also indicates that the aspire dashboard will be accessible through http://localhost:8080
var dockerCompose = builder.AddDockerComposeEnvironment("production")
    .WithDashboard(cfg => cfg.WithHostPort(8080));

// Keycloak Authentication Service
var keycloak = builder
    .AddKeycloak("keycloak", 6001) // Keycloak will be available on http://localhost:6001
    .WithDataVolume("keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    // Dentro del proyecto hemos definido un archivo json con la config del realm
    // Asi que solo lo importamos para no perder todo cada vez se reinicia
    .WithRealmImport("../infra/realms") 
    // Keycloak por defecto usa el puerto 8080 cuando se crea con aspire
    // Asi que creamos una variable de ambiente que podrá leer nginx
    // Cuando haya una peticion con target host id.overflow.local
    // Nginx ruteará el trafico al puerto 8080, es decir http://keycloak:8080
    .WithEnvironment("VIRTUAL_HOST", "ID.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8080");

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
// Este servicio no tiene una integracion con aspire, asi que se levanta como contenedor normal
var typesenseSecret = builder.AddParameter("typesense-api-key", secret: true);
var typesense = builder
    .AddContainer("typesense", "typesense/typesense", "29.0")
    .WithEnvironment("TYPESENSE_DATA_DIR", "/data")
    .WithEnvironment("TYPESENSE_ENABLE_CORS", "true")
    .WithEnvironment("TYPESENSE_API_KEY", typesenseSecret)
    .WithVolume("typesense-data", "/data")
    .WithHttpEndpoint(8108, 8108, name: "typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

// Question Service se agrega como proyecto, ya que debe compilarse para el deployment
// Tambien se definen referencias y dependencias del servicio con otros servicios como keycloak o rabbitMq
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

// YARP es un reverse proxy, pero en este caso se utiliza como gateway
// Es decir recibe una peticion y dependiendo del endpoint YARP decide a que servicio enviar la peticion
// Esto quiere decir que cualquier peticion que tenga como url /questions/ se rutea a questionService
// YARP funciona tanto para development como para produccion, ya que no le interesa quien mande la peticion
// En development recibe la peticion del browser o postman y aplica las reglas de redireccionamiento
// En producción quien recibe primero las peticiones es Nginx, asi que nginx envia el trafico a YARP
var yarp = builder.AddYarp("gateway")
    .WithConfiguration(cfg =>
    {
        cfg.AddRoute("/questions/{**catch-all}", questionService);
        cfg.AddRoute("/tags/{**catch-all}", questionService);
        cfg.AddRoute("/search/{**catch-all}", searchService);
    })
    // Esta linea es mandatoria, ya que YARP recibirá peticiones desde distintas interfaces de red:
    // Nginx -> http://gateway:8001
    // Browser -> http://localhost:8001
    // Asi que con esto se configura la aplicación para que pueda recibir trafico de ambas fuentes
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8001")
    // En este caso tanto el puerto externo como interno son 8001 a traves del schema http
    // El nombre del servicio es gateway y "isExternal" indica que el servicio puede alcanzarse desde fuera del contenedor
    .WithEndpoint(8001, 8001, scheme: "http", name: "gateway", isExternal: true)
    .WithEnvironment("VIRTUAL_HOST", "api.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8001");

// Para agregar el servicio webApp de UI es necesario instalar Aspire.hosting.javascript en AppHost.cs
// Adicionalmente es necesario agregar la carpeta webapp a la solucion "Add / existing folder / webpapp"
var webapp = builder.AddJavaScriptApp("webapp", "../webapp")
    .WithReference(keycloak)
    .WithHttpEndpoint(env: "PORT", port: 3000);

if (!builder.Environment.IsDevelopment())
{
    // Solo en el caso de que se despliegue en modo produccion se creará un contenedor para nginx
    // Este servicio estará disponible en el puerto 80
    // Gracias a que hemos incluido WithEnvironment("VIRTUAL_HOST", "api.overflow.local")
    // asi como .WithEnvironment("VIRTUAL_PORT", "8001");
    // No es necesario crear un archivo .conf donde definamos locations para nginx
    builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy","1.8")
        .WithEndpoint(80,80, "nginx", isExternal: true)
        // Esta linea permite a nginx inspeccionar los contenedores para generar automaticamente
        // el archivo conf.d/default.conf basandose en VIRTUAL_HOST y VIRTUAL_PORT
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", true);
}

builder.Build().Run();