using System.Runtime.CompilerServices;
using Promos.Authentication.ViewModels;

namespace Promos.Authorization.Application.Domain;

public class Errors
{
    public static readonly IErrorModel LoginIsBlocked = Create("Login is not allowed. Reason: blocked.");

    private static IErrorModel Create(string message, [CallerMemberName] string key = "")
    {
        return new ErrorModel(key, message);
    }

    private class ErrorModel : IErrorModel
    {
        public ErrorModel(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; }
        public string Message { get; }
    }
}