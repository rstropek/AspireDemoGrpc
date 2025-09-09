using System.Text.Json;
using MQTTnet;

namespace WebApi;

public static class MqttMessagingExtensions
{
    public static IEndpointRouteBuilder UseMqttMessaging(this IEndpointRouteBuilder app)
    {
        // Retain original flat routes for backward compatibility
        app.MapPost("/sendMessage", async (HttpContext context, MqttClientFactory factory, IConfiguration config) =>
        {
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);
            var message = data?.GetValueOrDefault("message", "Default message") ?? "Default message";

            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    config["MQTT:HOST"] ?? throw new InvalidOperationException("MQTT:HOST not set"),
                    int.Parse(config["MQTT:PORT"] ?? throw new InvalidOperationException("MQTT:PORT not set")))
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .Build();

            await mqttClient.ConnectAsync(options);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic("demo/messages")
                .WithPayload(message)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);
            await mqttClient.DisconnectAsync();

            return Results.Ok(new { Status = "Message sent", Message = message });
        }).WithTags("MQTT").WithName("SendMessage");

        app.MapGet("/messages", async (HttpContext context, MqttClientFactory factory, IConfiguration config) =>
        {
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");

            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    config["MQTT:HOST"] ?? throw new InvalidOperationException("MQTT:HOST not set"),
                    int.Parse(config["MQTT:PORT"] ?? throw new InvalidOperationException("MQTT:PORT not set")))
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .Build();

            await mqttClient.ConnectAsync(options);

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("demo/messages"))
                .Build();

            await mqttClient.SubscribeAsync(subscribeOptions);

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var payload = e.ApplicationMessage.ConvertPayloadToString();
                    var sseMessage = $"data: {JsonSerializer.Serialize(new { message = payload, timestamp = DateTime.UtcNow })}\n\n";
                    await context.Response.WriteAsync(sseMessage);
                    await context.Response.Body.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending SSE message: {ex.Message}");
                }
            };

            context.RequestAborted.Register(async () =>
            {
                try
                {
                    if (mqttClient.IsConnected)
                    {
                        await mqttClient.DisconnectAsync();
                    }
                }
                catch { /* ignore */ }
                finally
                {
                    mqttClient.Dispose();
                }
            });

            try
            {
                await context.Response.WriteAsync("data: {\"message\":\"Connected to message stream\",\"timestamp\":\"" + DateTime.UtcNow + "\"}\n\n");
                await context.Response.Body.FlushAsync();

                while (!context.RequestAborted.IsCancellationRequested)
                {
                    await Task.Delay(1000, context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                // client disconnected
            }
            finally
            {
                if (mqttClient.IsConnected)
                {
                    await mqttClient.DisconnectAsync();
                }
                mqttClient.Dispose();
            }
        }).WithTags("MQTT").WithName("MessageStream");

        return app;
    }
}
