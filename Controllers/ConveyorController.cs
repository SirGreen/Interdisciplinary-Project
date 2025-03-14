using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DADN.Models;
using System.Diagnostics;

namespace DADN.Controllers;

public class ConveyorController : Controller
{
    private readonly ILogger<ConveyorController> _logger;
    private const string StorageKey = "conveyorData"; // Key lưu trong Local Storage

    public ConveyorController(ILogger<ConveyorController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SaveData([FromBody] ConveyorBeltModel data)
    {
        if (data == null)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }

        _logger.LogInformation($"Saving Data: {JsonSerializer.Serialize(data)}");

        // Đọc dữ liệu hiện tại từ Local Storage (JS sẽ gọi hàm này)
        var storedData = HttpContext.Session.GetString(StorageKey);
        List<ConveyorBeltModel> list = string.IsNullOrEmpty(storedData)
            ? new List<ConveyorBeltModel>()
            : JsonSerializer.Deserialize<List<ConveyorBeltModel>>(storedData) ?? new List<ConveyorBeltModel>();

        // Thêm dữ liệu mới
        list.Add(data);

        // Lưu lại vào Session
        HttpContext.Session.SetString(StorageKey, JsonSerializer.Serialize(list));

        return Json(new { success = true });
    }

    public IActionResult GetData()
    {
        var storedData = HttpContext.Session.GetString(StorageKey);
        var data = string.IsNullOrEmpty(storedData) ? new List<ConveyorBeltModel>() : JsonSerializer.Deserialize<List<ConveyorBeltModel>>(storedData);
        return Json(new { success = true, data });
    }

    [HttpPost]
    public IActionResult DeleteData(int index)
    {
        var storedData = HttpContext.Session.GetString(StorageKey);
        if (string.IsNullOrEmpty(storedData))
        {
            return Json(new { success = false, message = "Không có dữ liệu để xóa!" });
        }

        var list = JsonSerializer.Deserialize<List<ConveyorBeltModel>>(storedData) ?? new List<ConveyorBeltModel>();

        if (index < 0 || index >= list.Count)
        {
            return Json(new { success = false, message = "Index không hợp lệ!" });
        }

        list.RemoveAt(index);
        HttpContext.Session.SetString(StorageKey, JsonSerializer.Serialize(list));

        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult ClearAllData()
    {
        HttpContext.Session.Remove(StorageKey);
        return Json(new { success = true });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
