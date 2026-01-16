using System;
using Articler.AppDomain.Models.VectorStorage;
using Articler.AppDomain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Articler.SiloHost.Services
{
    public class StartupHostedService(
        ILogger<StartupHostedService> logger,
        IServiceScopeFactory scopeFactory) : IHostedService
    {
        private readonly ILogger<StartupHostedService> _logger = logger;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartupHostedService::StartAsync: Initializing application services.");

            using var scope = _scopeFactory.CreateScope();

            var vectorStore = scope.ServiceProvider.GetRequiredService<VectorStore>();
            var qdrantSettings = scope.ServiceProvider
                .GetRequiredService<IOptions<QdrantSettings>>()
                .Value;

            try
            {
                _logger.LogInformation("StartupHostedService::StartAsync: ensuring qdrant collections exist with proper schema.");

                var openAICollection = vectorStore.GetCollection<Guid, DocumentStorageRecord>(qdrantSettings.CollectionName);
                await openAICollection.EnsureCollectionExistsAsync(cancellationToken);
                _logger.LogInformation("StartupHostedService::StartAsync: qdrant collection ensured with proper schema. " +
                    "CollectionName={collectionName}", qdrantSettings.CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "StartupHostedService::StartAsync: exception raised. Message: {exMessage}",
                    ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
