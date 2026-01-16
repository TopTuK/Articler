using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using OpenAI.Chat;
using System.ClientModel;

var builder = Host.CreateApplicationBuilder(args);
var host = builder.Build();

// Start the host to ensure all services are initialized
await host.StartAsync();

try
{
    var config = host.Services.GetRequiredService<IConfiguration>();
    Console.WriteLine("Hello World! Host initialized successfully!");
}
finally
{
    // Stop the host gracefully
    await host.StopAsync();
    host.Dispose();
}
