using Articler.AppDomain.Constants;
using Articler.AppDomain.Services.VectorStorage;
using Articler.AppDomain.Settings;
using Articler.SiloHost.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using OpenAI;
using Orleans.Hosting;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Serilog;
using System.ClientModel;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
{
    // Configure QDrant settings
    services.Configure<QdrantSettings>(configuration.GetSection("Qdrant"));

    // Configure OpenAIClientSettings
    // services.Configure<OpenAIClientSettings>(configuration.GetSection("DeepSeek"));

    // configure openai clients
    services.Configure<OpenAIClientSettings>(
        OpenAIClientSettings.OpenAIOptions, 
        configuration.GetSection("OpenAI"));
    services.Configure<OpenAIClientSettings>(
        OpenAIClientSettings.DeepSeekOptions,
        configuration.GetSection("DeepSeek"));
    services.Configure<OpenAIClientSettings>(
        OpenAIClientSettings.OpenRouterOptions,
        configuration.GetSection("OpenRouter"));
}

static void ConfigureServices(IServiceCollection services)
{
    // QdrantClient singleton
    services.AddSingleton<QdrantClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
        return new QdrantClient(
            new Uri(settings.Host),
            apiKey: settings.ApiKey);
    });

    // https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-may-2025?pivots=programming-language-csharp
    // IVectorStore -> VectorStore
    services.AddSingleton<VectorStore>(sp =>
    {
        return new QdrantVectorStore(
            sp.GetRequiredService<QdrantClient>(),
            ownsClient: false,
            options: new()
            {
                HasNamedVectors = true,
            }
        );
    });

    services.AddKeyedSingleton<OpenAIClient>("OpenAI", (sp, opt) =>
    {
        var settings = sp.GetOptionsByName<OpenAIClientSettings>(OpenAIClientSettings.OpenAIOptions);

        var credential = new ApiKeyCredential(settings.ApiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl)
        };

        var client = new OpenAIClient(credential, options);
        return client;
    });

    services.AddKeyedSingleton<OpenAIClient>("DeepSeek", (sp, opt) =>
    {
        var settings = sp.GetOptionsByName<OpenAIClientSettings>(OpenAIClientSettings.DeepSeekOptions);

        var credential = new ApiKeyCredential(settings.ApiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl)
        };

        var client = new OpenAIClient(credential, options);
        return client;
    });

    services.AddKeyedSingleton<OpenAIClient>("OpenRouter", (sp, opt) =>
    {
        var settings = sp.GetOptionsByName<OpenAIClientSettings>(OpenAIClientSettings.OpenRouterOptions);

        var credential = new ApiKeyCredential(settings.ApiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl)
        };

        var client = new OpenAIClient(credential, options);
        return client;
    });

    services.AddTransient<IVectorStorageService, VectorStorageService>();
}

static void ConfigureHostedServices(IServiceCollection services)
{
    services.AddHostedService<StartupHostedService>();
}

try
{
    // Create a host that can cohost aspnetcore AND orleans together in a single process.
    var builder = WebApplication.CreateBuilder(args);
    var env = builder.Environment;

    ConfigureOptions(builder.Services, builder.Configuration);
    ConfigureServices(builder.Services);
    ConfigureHostedServices(builder.Services);

    builder
        .UseOrleans(siloBuilder =>
        {
            if (env.IsDevelopment())
            {
                siloBuilder.UseLocalhostClustering();
            }

            siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(options =>
            {
                options.ClusterId = OrleansConstants.ClusterId;
                options.ServiceId = OrleansConstants.ServiceId;
            })
            .AddAdoNetGrainStorage(
                name: OrleansConstants.AdoStorageProviderName,
                configureOptions: options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = "Host=localhost;Port=5432;Database=articler;Username=articler;Password=articler";
                }
            )
            .ConfigureLogging(logging => logging.AddConsole());
        });

    var app = builder.Build();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SiloHost: Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}