using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DADN.Models;
using System.Text.Json;

namespace DADN.Controllers
{
    public class InputController : Controller
    {
        private readonly ILogger<InputController> _logger;

        public InputController(ILogger<InputController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit([FromBody] InputModel input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
            }

            _logger.LogInformation($"Dữ liệu nhận: {JsonSerializer.Serialize(input)}");

            // Trả về dữ liệu để frontend lưu vào Local Storage
            return Ok(input);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
