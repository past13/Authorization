using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Promos.Authorization.Application.Domain;
using Promos.Authorization.Data;

namespace Promos.Authorization.Services;

public interface IUserService
{
    Task<UserModel?> Load(string userName, CancellationToken cancellationToken);
    Task<UserModel?> LoadByPassword(string userName, string password, CancellationToken cancellationToken);
    bool IsBlockedUser(DateTimeOffset? lockoutEnd);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task<UserModel?> Load(string userName, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Where(u => u.NormalizedUserName.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        // Todo: created method for Locked
        if (user == null && IsBlockedUser(user?.LockoutEnd))
            return null;
        
        return ToUserModel(user);
    }

    public async Task<UserModel?> LoadByPassword(string userName, string password, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Where(u => u.NormalizedUserName.ToLower() == userName.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return null;
        
        //Todo: move to separate class
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? ToUserModel(user) : null;
    }

    public bool IsBlockedUser(DateTimeOffset? lockoutEnd)
    {
        return lockoutEnd.HasValue && lockoutEnd > DateTime.UtcNow;
    } 

    private static UserModel? ToUserModel(ApplicationUser? user)
    {
        if (user == null)
            return null;

        return new UserModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            LockoutEnd = user.LockoutEnd
        };
    }
}