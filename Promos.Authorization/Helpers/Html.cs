using Microsoft.AspNetCore.Mvc.ModelBinding;
using Promos.Authentication.ViewModels;

namespace Promos.Authorization.Helpers;

public static class Html
{
    public static int Errors(this ModelStateDictionary state, IErrorModel error)
    {
        state.AddModelError(error.Key, error.Message);
        return state.ErrorCount;
    }
}
