namespace Promos.Authorization.Application.Domain;

public class UserModel
{
    public string UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}