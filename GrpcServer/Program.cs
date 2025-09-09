using GrpcServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // Note: Service defaults come from shared project

builder.Services.AddGrpc();

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.UseCors();

app.MapGrpcService<GreeterService>().RequireCors("AllowAll");

app.Run();
