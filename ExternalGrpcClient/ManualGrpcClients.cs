using AspireDemoGrpc.Shared.Protos;
using Grpc.Net.Client;
using Grpc.Core;

namespace ExternalGrpcClient;

/// <summary>
/// Manual gRPC client for DataService
/// </summary>
public class DataServiceClient
{
    private readonly GrpcChannel _channel;

    public DataServiceClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<DataResponse> GetDataAsync(DataRequest request)
    {
        try
        {
            // Create a simple HTTP client to call the gRPC service
            var client = new HttpClient();
            client.BaseAddress = new Uri(_channel.Target);
            
            // For now, return mock data - in a real implementation, 
            // you would make the actual gRPC call here
            return new DataResponse { X = 42, Y = 24 };
        }
        catch (Exception ex)
        {
            // Return error response
            throw new RpcException(new Status(StatusCode.Internal, $"Error calling DataService: {ex.Message}"));
        }
    }
}

/// <summary>
/// Manual gRPC client for CalculatorService
/// </summary>
public class CalculatorServiceClient
{
    private readonly GrpcChannel _channel;

    public CalculatorServiceClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<PingResponse> PingAsync(PingRequest request)
    {
        // Mock implementation - in real scenario, make actual gRPC call
        return new PingResponse { Message = "Pong from CalculatorService!" };
    }

    public async Task<AddResponse> AddAsync(AddRequest request)
    {
        // Mock implementation - in real scenario, make actual gRPC call
        return new AddResponse { Sum = 100 };
    }
}
