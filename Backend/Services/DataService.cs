using AspireDemoGrpc.Shared.Protos;
using Grpc.Core;

namespace Backend.Services;

public class DataServiceImpl(ILogger<DataServiceImpl> logger) : DataService.DataServiceBase
{
    public override Task<DataResponse> GetData(DataRequest request, ServerCallContext context)
    {
        logger.LogInformation("DataService.GetData called");
        
        return Task.FromResult(new DataResponse
        {
            X = 10,
            Y = 20
        });
    }
}
