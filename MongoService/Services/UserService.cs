using MongoDB.Driver;
using MongoService.Models;

namespace MongoService.Services;

public class UserService
{
    private readonly IMongoCollection<User>? _users;

    public UserService(IMongoDatabase? database)
    {
        _users = database?.GetCollection<User>("users");
    }

    public async Task<List<User>> GetUsersAsync()
    {
        if (_users == null) return new List<User>();
        return await _users.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetUserAsync(string id)
    {
        if (_users == null) return null;
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (_users == null) throw new InvalidOperationException("MongoDB not available");
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task UpdateUserAsync(string id, User user)
    {
        if (_users == null) throw new InvalidOperationException("MongoDB not available");
        await _users.ReplaceOneAsync(u => u.Id == id, user);
    }

    public async Task DeleteUserAsync(string id)
    {
        if (_users == null) throw new InvalidOperationException("MongoDB not available");
        await _users.DeleteOneAsync(u => u.Id == id);
    }
}
