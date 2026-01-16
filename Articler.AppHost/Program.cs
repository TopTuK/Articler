using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var startupType = Environment.GetEnvironmentVariable("STARTUP_TYPE")?.ToLower();
var IsNotDevelopment = !builder.Environment.IsDevelopment();

if (startupType == "docker")
{

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

builder
    .Build()
    .Run();
