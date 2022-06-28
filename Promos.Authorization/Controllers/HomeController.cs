using Microsoft.AspNetCore.Mvc;

namespace Promos.Authorization.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}