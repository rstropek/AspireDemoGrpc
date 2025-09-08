using MongoDB.Driver;
using MongoService.Models;
using MongoService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure specific port for MongoDB Service
builder.WebHost.UseUrls("http://localhost:5003");

// Configure MongoDB with deferred connection and retry logic
builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    var mongoConnectionString = configuration.GetConnectionString("mongodb") ?? "mongodb://localhost:27017";
    logger.LogInformation("Attempting to connect to MongoDB at: {ConnectionString}", mongoConnectionString);
    
    try
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase("aspiredemo");
        
        // Test the connection
        mongoClient.ListDatabaseNames();
        logger.LogInformation("MongoDB connection successful");
        return database;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to MongoDB: {Error}", ex.Message);
        return null!;
    }
});

builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// MongoDB User endpoints
app.MapGet("/users", async (UserService userService) =>
{
    var users = await userService.GetUsersAsync();
    return Results.Ok(users);
});

app.MapGet("/users/{id}", async (string id, UserService userService) =>
{
    var user = await userService.GetUserAsync(id);
    return user != null ? Results.Ok(user) : Results.NotFound();
});

app.MapPost("/users", async (User user, UserService userService) =>
{
    var createdUser = await userService.CreateUserAsync(user);
    return Results.Created($"/users/{createdUser.Id}", createdUser);
});

app.MapPut("/users/{id}", async (string id, User user, UserService userService) =>
{
    await userService.UpdateUserAsync(id, user);
    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (string id, UserService userService) =>
{
    await userService.DeleteUserAsync(id);
    return Results.NoContent();
});

// Health check endpoint
app.MapGet("/", () => "MongoDB Service is running");

app.Run();
