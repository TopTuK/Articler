using Articler.AppDomain.Constants;
using Articler.AppDomain.Factories.Token;
using Articler.AppDomain.Services.Document;
using Articler.AppDomain.Services.TokenService;
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
using Qdrant.Client;
using Serilog;
using System.ClientModel;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
{
    // Configure QDrant settings
    services.Configure<QdrantSettings>(configuration.GetSection("Qdrant"));

    services.Configure<ChatAgentSettings>(
        ChatAgentSettings.OpenAI,
        configuration.GetRequiredSection("ChatAgent:OpenAI"));
    services.Configure<ChatAgentSettings>(
        ChatAgentSettings.DeepSeek,
        configuration.GetRequiredSection("ChatAgent:DeepSeek"));

    services.Configure<EmbeddingAgentSettings>(
        EmbeddingAgentSettings.OpenRouter,
        configuration.GetRequiredSection("EmbeddingAgent:OpenRouter"));
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
            options: new() // Decided to use different collections
            {
                HasNamedVectors = true,
            }
        );
    });

    // Singleton -> Transient
    services.AddKeyedTransient<OpenAIClient>("DeepSeekChatClient", (sp, opt) =>
    {
        var settings = sp.GetOptionsByName<ChatAgentSettings>(ChatAgentSettings.DeepSeek);
        var credential = new ApiKeyCredential(settings.ApiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl)
        };

        return new OpenAIClient(credential, options);
    });

    // Singleton -> Transient
    services.AddKeyedTransient<OpenAIClient>("OpenRouterEmbeddingClient", (sp, opt) =>
    {
        var settings = sp.GetOptionsByName<EmbeddingAgentSettings>(EmbeddingAgentSettings.OpenRouter);
        var credential = new ApiKeyCredential(settings.ApiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl)
        };

        return new OpenAIClient(credential, options);
    });

    // Tokenizers
    services.AddTransient<ICalculateTokenService>(sp =>
    {
        //var settings = sp.GetOptionsByName<EmbeddingAgentSettings>(EmbeddingAgentSettings.OpenRouter);
        return TokenServiceFactory.CreateCalculateTokenService("text-embedding-3-small");
    });

    // Transient services
    services.AddTransient<IVectorStorageService, VectorStorageService>();
    services.AddTransient<IPdfDocumentService, PdfDocumentService>();
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

    builder.Services.AddHttpClient();
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