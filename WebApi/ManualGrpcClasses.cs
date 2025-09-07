using AspireDemoGrpc.Shared.Protos;
using Grpc.Core;

namespace WebApi.Services;

/// <summary>
/// Manual base class for CalculatorService
/// </summary>
public abstract class CalculatorServiceBase
{
    public virtual Task<AddResponse> Add(AddRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, ""));
    }

    public virtual Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        throw new RpcException(new Status(StatusCode.Unimplemented, ""));
    }
}

/// <summary>
/// Manual client for DataService
/// </summary>
public class DataServiceClient
{
    private readonly HttpClient _httpClient;

    public DataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DataResponse> GetDataAsync(DataRequest request)
    {
        // Mock implementation - in real scenario, make actual gRPC call
        return new DataResponse { X = 10, Y = 20 };
    }
}
