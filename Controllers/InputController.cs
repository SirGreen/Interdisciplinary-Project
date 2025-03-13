using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DADN.Models;

namespace DADN.Controllers;

public class InputController : Controller
{
    private readonly ILogger<InputController> _logger;

    public InputController(ILogger<InputController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // _logger.LogInformation("Home page visited");
        return View();
    }

    public IActionResult Test()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
