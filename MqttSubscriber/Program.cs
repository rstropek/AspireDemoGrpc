using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure specific port for MQTT Subscriber
builder.WebHost.UseUrls("http://localhost:5002");


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

// Store received messages in memory
var receivedMessages = new ConcurrentBag<string>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseCors();

// MQTT Subscriber Service
app.MapGet("/messages", () =>
{
    return Results.Ok(new { Messages = receivedMessages.ToArray() });
});

app.MapGet("/clear", () =>
{
    receivedMessages.Clear();
    return Results.Ok(new { Status = "Messages cleared" });
});

app.MapGet("/", () => "MQTT Subscriber Service is running");

// Start MQTT subscriber in background service
var factory = new MqttFactory();
var mqttClient = factory.CreateMqttClient();

mqttClient.ApplicationMessageReceivedAsync += async e =>
{
    try
    {
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        var messageData = JsonSerializer.Deserialize<JsonElement>(payload);
        
        var message = messageData.GetProperty("Message").GetString() ?? "Unknown";
        receivedMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Received MQTT message: {Message}", message);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error processing MQTT message");
    }
};

// Start MQTT connection in background service - don't block startup
var mqttService = app.Services.GetRequiredService<ILogger<Program>>();

// Use local MQTT broker
var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883) // Use local MQTT broker
    .Build();

// Start MQTT connection immediately
_ = Task.Run(async () =>
{
    mqttService.LogInformation("Starting MQTT connection...");
    
    var retryCount = 0;
    while (retryCount < 5) // Limit retries to prevent infinite loop
    {
        try
        {
            mqttService.LogInformation("Attempting to connect to MQTT broker at localhost:1883...");
            await mqttClient.ConnectAsync(options);
            mqttService.LogInformation("Connected to MQTT broker successfully");
            
            await mqttClient.SubscribeAsync("aspire/demo/messages");
            mqttService.LogInformation("MQTT Subscriber subscribed to aspire/demo/messages");
            break; // Success, exit retry loop
        }
        catch (Exception ex)
        {
            retryCount++;
            mqttService.LogWarning(ex, "Failed to connect to MQTT broker (attempt {RetryCount}/5), retrying in 2 seconds...", retryCount);
            await Task.Delay(2000); // Wait 2 seconds before retry
        }
    }

    if (retryCount >= 5)
    {
        mqttService.LogError("Failed to connect to MQTT broker after 5 attempts. MQTT functionality will not be available.");
    }
});


app.Run();
