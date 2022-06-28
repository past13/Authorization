namespace Promos.Authorization.Data;

public interface IUserRequest
{
    string UserName { get; }
}

public class UserRequest: IUserRequest
{
    public UserRequest(string userName)
    {
        UserName = userName;
    }

    public string UserName { get; }
}
