using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure specific port for MQTT Publisher
builder.WebHost.UseUrls("http://localhost:5001");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseCors();

// MQTT Publisher Service
app.MapGet("/publish/{message}", async (string message, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Attempting to publish MQTT message: {Message}", message);
        
        var factory = new MqttFactory();
       using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // Use local MQTT broker
            .WithTimeout(TimeSpan.FromSeconds(10)) // Add timeout
            .Build();

        logger.LogInformation("Connecting to MQTT broker at localhost:1883...");
        await mqttClient.ConnectAsync(options);
        logger.LogInformation("Connected to MQTT broker successfully");

        var payload = JsonSerializer.Serialize(new
        {
            Message = message,
            Timestamp = DateTime.UtcNow,
            Source = "MqttPublisher"
        });

        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic("aspire/demo/messages")
            .WithPayload(payload)
            .WithRetainFlag(false)
            .Build();

        logger.LogInformation("Publishing message to topic 'aspire/demo/messages'...");
        await mqttClient.PublishAsync(mqttMessage);
        await mqttClient.DisconnectAsync();

        logger.LogInformation("Successfully published message: {Message}", message);
        return Results.Ok(new { Status = "Message published", Message = message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to publish MQTT message: {Error}", ex.Message);
        return Results.Problem($"Failed to publish message: {ex.Message}");
    }
});

app.MapGet("/", () => "MQTT Publisher Service is running");


app.Run();
