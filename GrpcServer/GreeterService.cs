using Grpc.Core;
using GrpcDemo;

namespace GrpcServer;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        logger.LogInformation($"Saying hello to {request.Name}");

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}"
        });
    }
}