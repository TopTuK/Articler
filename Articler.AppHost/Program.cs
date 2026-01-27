using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var isDevelopmentMode = builder.Environment.IsDevelopment();
var startupType = Environment.GetEnvironmentVariable("STARTUP_TYPE")?.ToLower();
var pgMode = Environment.GetEnvironmentVariable("POSTGRES_MODE")?.ToLower() ?? "local";

if (startupType == "docker")
{
    var webAppBuild = builder.AddNpmApp("webapp-build", "../webapp", "build");

    var pgUser = builder.AddParameter("postgres-user", "articler");
    var pgPassword = builder.AddParameter("postgres-password", "articler", secret: true);
    var postgres = builder
        .AddPostgres("postgres", port: 5432)
        .WithEnvironment("POSTGRES_USER", "articler") // for scripts
        .WithEnvironment("POSTGRES_PASSWORD", "articler") // for scripts
        .WithEnvironment("POSTGRES_DB", "articler") // for scripts
        .WithUserName(pgUser)
        .WithPassword(pgPassword)
        //.WithDataVolume("articler_postgres") // for debug do not save datavolume
        .WithBindMount("Scripts", "/docker-entrypoint-initdb.d")
        .AddDatabase("articler");

    var siloHost = builder.AddDockerfile("articler-silohost", "..", "Articler.SiloHost/Dockerfile")
        .WithHttpEndpoint(port: 11111, targetPort: 11111, name: "silo")
        .WithHttpEndpoint(port: 30000, targetPort: 30000, name: "gateway")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", isDevelopmentMode ? "Development" : "Docker")
        .WithEnvironment("DOTNET_ENVIRONMENT", isDevelopmentMode ? "Development" : "Docker")
        .WithEnvironment("ORLEANS_SILO_PORT", "11111")
        .WithEnvironment("ORLEANS_GATEWAY_PORT", "30000")
        .WithEnvironment("POSTGRES_CONNECTION_STRING", "Host=postgres;Database=articler;Username=articler;Password=articler")
        .WaitFor(postgres);

    // Start Web API in Docker
    var webApi = builder.AddDockerfile("articler-webapi", "..", "Articler.WebApi/Dockerfile")
        .WaitFor(webAppBuild)
        .WaitFor(siloHost)
        .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", isDevelopmentMode ? "Development" : "Docker")
        .WithEnvironment("DOTNET_ENVIRONMENT", isDevelopmentMode ? "Development" : "Docker")
        .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
        .WithEnvironment("POSTGRES_CONNECTION_STRING", "Host=postgres;Database=articler;Username=articler;Password=articler");
}
else
{
    if (pgMode == "docker")
    {
        var pgUser = builder.AddParameter("postgres-user", "articler");
        var pgPassword = builder.AddParameter("postgres-password", "articler", secret: true);
        var postgres = builder
            .AddPostgres("postgres", port: 5432)
            .WithEnvironment("POSTGRES_USER", "articler") // for scripts
            .WithEnvironment("POSTGRES_PASSWORD", "articler") // for scripts
            .WithEnvironment("POSTGRES_DB", "articler") // for scripts
            .WithUserName(pgUser)
            .WithPassword(pgPassword)
            .WithDataVolume("articler_postgres")
            .WithBindMount("Scripts", "/docker-entrypoint-initdb.d")
            .AddDatabase("articler");

        var front = builder.AddNpmApp("front", "../webapp", "dev");

        var siloHost = builder.AddProject<Projects.Articler_SiloHost>("articler-silohost")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
            .WithReference(postgres)
            .WaitFor(postgres, WaitBehavior.StopOnResourceUnavailable);

        var webApi = builder
            .AddProject<Projects.Articler_WebApi>("api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
            .WithReference(siloHost)
            .WithReference(front)
            .WithHttpsEndpoint(port: 7209)
            .WaitFor(siloHost, WaitBehavior.StopOnResourceUnavailable);
    }
    else
    {
        var front = builder.AddNpmApp("front", "../webapp", "dev");
        var siloHost = builder.AddProject<Projects.Articler_SiloHost>("articler-silohost")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

        var webApi = builder
            .AddProject<Projects.Articler_WebApi>("api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
            .WithReference(siloHost)
            .WithReference(front)
            .WaitFor(siloHost);
    }
}

builder
    .Build()
    .Run();
