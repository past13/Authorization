using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Promos.Authorization.Application.Domain;
using Promos.Authorization.Data;
using Promos.Authorization.Helpers;
using Promos.Authorization.Services;
using Promos.Authorization.ViewModels;

namespace Promos.Authorization.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserService userService
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
    }
    
    [HttpGet("close")]
    [AllowAnonymous]
    public IActionResult Logout(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("close")]
    [HttpDelete("close")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmLogout(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", new { returnUrl });
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }
    
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userService.LoadByPassword(model.Email, model.Password, cancellationToken);

            if (user != null)
            {
                if (_userService.IsBlockedUser(user.LockoutEnd))
                {
                    ModelState.Errors(Errors.LoginIsBlocked);
                    return View(model);
                }
            }
    
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View(model);
    }
}