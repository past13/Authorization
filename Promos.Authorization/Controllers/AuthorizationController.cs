using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Promos.Authorization.Application.Domain;
using Promos.Authorization.Data;
using Promos.Authorization.Helpers;
using Promos.Authorization.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Promos.Authorization.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    
    private readonly IUserService _userService;
    
    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserService userService)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
    }
    
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        // var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // if (!result.Succeeded)
        // {
        //     return NotLoggedIn(request);
        // }
        
         if (!User.Identity.IsAuthenticated)
         {
             var returnUrl = Request.PathBase + Request.Path + Request.QueryString;
             return RedirectToAction("Login", "Account", new { returnUrl, request.ClientId });
         }

         // If prompt=login was specified by the client application,
         // immediately return the user agent to the login page.
         if (request.HasPrompt(Prompts.Login))
         {
             return OpenLogin(request);
         }
         
         var userRequest = User.ToUserRequest();
         var user = await LoadUser(userRequest, cancellationToken);
         
         return null;
    }
        
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal claimsPrincipal;

        if (request.IsClientCredentialsGrantType())
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Subject (sub) is a required field, we use the client id as the subject identifier here.
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
            identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

            claimsPrincipal = new ClaimsPrincipal(identity);

            claimsPrincipal.SetScopes(request.GetScopes());
        }

        else if (request.IsAuthorizationCodeGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
            
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }

        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
        
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        return Ok(new
        {
            Name = claimsPrincipal.GetClaim(Claims.Subject),
            Occupation = "Test",
            Age = 43
        });
    }
    
    private IActionResult NotLoggedIn(OpenIddictRequest request)
    {
        // If the client application requested prompt less authenWtication,
        // return an error indicating that the user is not logged in.
        if (request.HasPrompt(Prompts.None))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }));
        }

        var challengeResponse = Challenge(
            authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
            });

        return challengeResponse;
    }
    
    private IActionResult OpenLogin(OpenIddictRequest request)
    {
        // To avoid endless login -> authorization redirects, the prompt=login flag
        // is removed from the authorization request payload before redirecting the user.
        var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

        var parameters = Request.HasFormContentType
            ? Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList()
            : Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

        parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

        return Challenge(
            authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
            });
    }
    
    private async Task<UserModel> LoadUser(UserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.Load(request.UserName, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("The user details cannot be retrieved.");

        return user;
    }
    
    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Permissions.Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Permissions.Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Permissions.Scopes.Roles))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}