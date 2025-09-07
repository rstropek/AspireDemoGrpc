using AspireDemoGrpc.Shared.Protos;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// Map gRPC services
app.MapGrpcService<DataServiceImpl>();

// Health check endpoint
app.MapGet("/", () => "Backend gRPC service is running");

app.Run();