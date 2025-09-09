using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WebApi;

// Extension methods and models related to User management (MongoDB backed)
public static class UserManagementExtensions
{
    public static IEndpointRouteBuilder UseUserManagement(this IEndpointRouteBuilder app)
    {
        // Group all user endpoints under /users for clarity
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", async (IMongoClient client) =>
        {
            var collection = GetCollection(client);
            var users = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
            return Results.Ok(users);
        });

        group.MapGet("/{id}", async (string id, IMongoClient client) =>
        {
            var collection = GetCollection(client);
            var user = await collection.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user is not null ? Results.Ok(user) : Results.NotFound();
        });

        group.MapPost("/", async (User user, IMongoClient client) =>
        {
            var collection = GetCollection(client);
            await collection.InsertOneAsync(user);
            return Results.Created($"/users/{user.Id}", user);
        });

        group.MapPut("/{id}", async (string id, User user, IMongoClient client) =>
        {
            var collection = GetCollection(client);
            await collection.ReplaceOneAsync(u => u.Id == id, user);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (string id, IMongoClient client) =>
        {
            var collection = GetCollection(client);
            await collection.DeleteOneAsync(u => u.Id == id);
            return Results.NoContent();
        });

        return app;
    }

    private static IMongoCollection<User> GetCollection(IMongoClient client)
        => client.GetDatabase("mongodb").GetCollection<User>("users");
}

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
