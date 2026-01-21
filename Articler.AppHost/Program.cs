using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var isDevelopmentMode = builder.Environment.IsDevelopment();
var startupType = Environment.GetEnvironmentVariable("STARTUP_TYPE")?.ToLower();
var pgMode = Environment.GetEnvironmentVariable("POSTGRES_MODE")?.ToLower() ?? "local";

if (startupType == "docker")
{

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
            .WaitFor(postgres);

        var webApi = builder
            .AddProject<Projects.Articler_WebApi>("api")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
            .WithReference(siloHost)
            .WithReference(front)
            .WithHttpsEndpoint(port: 7209)
            .WaitFor(siloHost);
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
