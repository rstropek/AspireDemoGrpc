using System.Diagnostics;
using System.Net;
using GrpcDemo;
using MongoDB.Driver;
using MQTTnet;
using WebApi;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpcClient<Greeter.GreeterClient>(o =>
{
    o.Address = new Uri("http://grpcserver");
});
builder.Services.AddSingleton<MqttClientFactory>();

builder.AddServiceDefaults(); // Note: Service defaults come from shared project
builder.Services.AddCors();

builder.AddMongoDBClient(connectionName: "mongodbdatabase");

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// .NET Activity Source is called "Tracer" in OpenTelemetry.
// Consider using https://opentelemetry.io/docs/languages/net/shim/
// to harmonize the naming of the components.
var source = new ActivitySource("WebApi");

app.MapGet("/ping", () => new { Message = "pong" });

app.MapGet("/ip", async (IConfiguration config) =>
{
    // See also https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview?tabs=docker#service-endpoint-environment-variable-format
    // We could also access the environment variable services__mosquitto__mqtt__0 directly.
    var serviceReference = config["Services:mosquitto:mqtt:0"];
    serviceReference = serviceReference?.Replace("tcp://", string.Empty);
    var port = serviceReference?[(serviceReference.IndexOf(':') + 1)..] ?? string.Empty;
    serviceReference = serviceReference![..^(port.Length + 1)];

    var ipAddress = string.Empty;
    try
    {
        if (!string.IsNullOrEmpty(serviceReference))
        {
            var hostEntry = await Dns.GetHostEntryAsync(serviceReference);
            ipAddress = hostEntry.AddressList.FirstOrDefault()?.ToString() ?? "Not found";
        }
        else
        {
            ipAddress = "Service reference not available";
        }
    }
    catch (Exception ex)
    {
        ipAddress = $"Error resolving IP: {ex.Message}";
    }

    return Results.Ok(new
    {
        ServiceReference = serviceReference,
        Port = port,
        IpAddress = ipAddress
    });
});

app.MapGet("/callGreeter", async (Greeter.GreeterClient client) =>
{
    HelloReply reply;
    using (var activity = source.StartActivity("Calling gRPC Greeter"))
    {
        reply = await client.SayHelloAsync(new HelloRequest { Name = "WebApi" });
    }

    return Results.Ok(new { reply.Message });
});

// MQTT messaging endpoints
app.UseMqttMessaging();

// User management endpoints
app.UseUserManagement();

// Rust service endpoints
app.UseRustyEndpoints();

app.Run();