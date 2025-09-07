using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExternalGrpcClient;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddCommandLine(args)
            .Build();

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("External gRPC Client starting...");

        try
        {
            // Get service URLs from configuration
            var backendServiceUrl = configuration["GrpcServices:BackendService"] ?? "https://localhost:7001";
            var webApiServiceUrl = configuration["GrpcServices:WebApiService"] ?? "https://localhost:7002";

            logger.LogInformation("Connecting to Backend Service at: {BackendUrl}", backendServiceUrl);
            logger.LogInformation("Connecting to WebApi Service at: {WebApiUrl}", webApiServiceUrl);

            // Create gRPC channels
            using var backendChannel = GrpcChannel.ForAddress(backendServiceUrl);
            using var webApiChannel = GrpcChannel.ForAddress(webApiServiceUrl);

            var backendClient = new DataServiceClient(backendChannel);
            var webApiClient = new CalculatorServiceClient(webApiChannel);

            // Test Backend Service
            logger.LogInformation("Testing Backend Service...");
            var dataResponse = await backendClient.GetDataAsync(new AspireDemoGrpc.Shared.Protos.DataRequest());
            logger.LogInformation("Backend Service Response: X={X}, Y={Y}", dataResponse.X, dataResponse.Y);

            // Test WebApi Service
            logger.LogInformation("Testing WebApi Service...");
            var pingResponse = await webApiClient.PingAsync(new AspireDemoGrpc.Shared.Protos.PingRequest());
            logger.LogInformation("WebApi Ping Response: {Message}", pingResponse.Message);

            var addResponse = await webApiClient.AddAsync(new AspireDemoGrpc.Shared.Protos.AddRequest());
            // logger.LogInformation("WebApi Add Response: Sum={Sum}", addResponse.Sum);

            // Continuous operation mode
            logger.LogInformation("Starting continuous operation mode. Press 'q' to quit...");
            
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    logger.LogInformation("Quitting...");
                    break;
                }

                // Perform periodic operations
                try
                {
                    var currentData = await backendClient.GetDataAsync(new AspireDemoGrpc.Shared.Protos.DataRequest());
                    var currentSum = await webApiClient.AddAsync(new AspireDemoGrpc.Shared.Protos.AddRequest());
                    
                    logger.LogInformation("Periodic check - Data: X={X}, Y={Y}, Sum={Sum}", 
                        currentData.X, currentData.Y, currentSum.Sum);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during periodic operation");
                }

                await Task.Delay(5000); // Wait 5 seconds between operations
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in External gRPC Client");
            Environment.Exit(1);
        }

        logger.LogInformation("External gRPC Client shutting down...");
    }
}
