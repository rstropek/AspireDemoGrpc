using AspireDemoGrpc.Shared.Protos;
using Grpc.Core;

namespace WebApi.Services;

public class CalculatorServiceImpl : CalculatorServiceBase
{
    private readonly DataServiceClient _dataClient;
    private readonly ILogger<CalculatorServiceImpl> _logger;

    public CalculatorServiceImpl(DataServiceClient dataClient, ILogger<CalculatorServiceImpl> logger)
    {
        _dataClient = dataClient;
        _logger = logger;
    }

    public override async Task<AddResponse> Add(AddRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CalculatorService.Add called");

        // Get data from the backend service via gRPC
        var dataResponse = await _dataClient.GetDataAsync(new DataRequest());

        var sum = dataResponse.X + dataResponse.Y;

        return new AddResponse
        {
            Sum = sum
        };
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CalculatorService.Ping called");
        
        return Task.FromResult(new PingResponse
        {
            Message = "pong"
        });
    }
}
