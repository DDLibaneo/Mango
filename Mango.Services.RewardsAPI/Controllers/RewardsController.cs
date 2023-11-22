using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.RewardsAPI.Controllers;

public class RewardsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
