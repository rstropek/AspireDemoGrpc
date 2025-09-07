using AspireDemoGrpc.Shared.Protos;
using Grpc.Core;

namespace Backend.Services;

public class DataServiceImpl : DataService.DataServiceBase
{
    private readonly ILogger<DataServiceImpl> _logger;

    public DataServiceImpl(ILogger<DataServiceImpl> logger)
    {
        _logger = logger;
    }

    public override Task<DataResponse> GetData(DataRequest request, ServerCallContext context)
    {
        _logger.LogInformation("DataService.GetData called");
        
        return Task.FromResult(new DataResponse
        {
            X = 10,
            Y = 20
        });
    }
}
