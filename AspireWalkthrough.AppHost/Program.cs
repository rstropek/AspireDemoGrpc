using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var mosquitto = builder.AddContainer("mosquitto", image: "eclipse-mosquitto:latest")
    .WithBindMount(Path.Join("..", "Mosquitto", "mosquitto.conf"), "/mosquitto/config/mosquitto.conf")
    .WithEndpoint("mqtt", e =>
    {
        e.Port = 1883;
        e.TargetPort = 1833;
        e.Protocol = ProtocolType.Tcp;
        e.UriScheme = "tcp";
    });

var backend = builder.AddProject<Projects.Backend>("backend")
    .WithReplicas(2);

var postgres = builder
    .AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(cfg => cfg.WithDataVolume("postgresdata", isReadOnly: false));
var aspiredb = postgres.AddDatabase("aspiredb");
var postgresdb = postgres.AddDatabase(name: "postgresdb", databaseName: "postgres");

var cache = builder
    .AddAzureRedis("cache")
    .RunAsContainer();

// Add MQTT services without Docker broker - use local MQTT broker
var mqttPublisher = builder.AddProject<Projects.MqttPublisher>("mqtt-publisher")
    .WithExternalHttpEndpoints();

var mqttSubscriber = builder.AddProject<Projects.MqttSubscriber>("mqtt-subscriber")
    .WithExternalHttpEndpoints()
    .WithReference(mosquitto.GetEndpoint("mqtt"))
    .WithEnvironment(ctx =>
    {
        var ep = mosquitto.GetEndpoint("mqtt");
        ctx.EnvironmentVariables["MQTT__HOST"] = ep.Host;
        ctx.EnvironmentVariables["MQTT__PORT"] = ep.Port.ToString();
        ctx.EnvironmentVariables["MQTT__URI"]  = ep.ToString()!; // e.g. tcp://localhost:1883
    });

// Add MongoDB
var mongoDb = builder.AddMongoDB("mongodb")
    .WithDataVolume("mongodata", isReadOnly: false);

var mongoService = builder.AddProject<Projects.MongoService>("mongo-service")
    .WithReference(mongoDb)
    .WaitFor(mongoDb);

var webapi = builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(backend)
    .WithReplicas(2)
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithReference(aspiredb)
    .WaitFor(aspiredb)
    .WithReference(cache)
    .WaitFor(cache)
    .WithExternalHttpEndpoints();

var frontend = builder.AddNpmApp("frontend", "../AngularFrontend")
    .WithReference(webapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
