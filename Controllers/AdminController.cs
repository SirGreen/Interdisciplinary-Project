using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AdminController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly IExtractionService _extractionService;

    public AdminController(ICatalogService catalogService, IExtractionService extractionService)
    {
        _catalogService = catalogService;
        _extractionService = extractionService;
    }

    // Hiển thị form upload
    [HttpGet]
    public IActionResult UploadCatalog()
    {
        return View();
    }

    // Xử lý upload URL catalog
    [HttpPost]
    public async Task<IActionResult> UploadCatalog(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Error"] = "URL không hợp lệ.";
            return RedirectToAction("UploadCatalog");
        }

        var extractedData = await _extractionService.ExtractDataFromWebAsync(url);

        if (extractedData == null)
        {
            TempData["Error"] = "Không thể trích xuất dữ liệu.";
            return RedirectToAction("UploadCatalog");
        }

        await _catalogService.AddCatalogAsync(extractedData);
        TempData["Success"] = "Dữ liệu đã được lưu thành công!";
        return RedirectToAction("CatalogList");
    }

    // Hiển thị danh sách catalog
    [HttpGet]
    public async Task<IActionResult> CatalogList()
    {
        var catalogs = await _catalogService.GetAllAsync();
        return View(catalogs);
    }

    // Hiển thị form nhập tay
    [HttpGet]
    public IActionResult AddMotorManual()
    {
        return View();
    }

    // Xử lý thêm catalog bằng nhập tay
    [HttpPost]
    public async Task<IActionResult> AddMotorManual(MotorCatalog motor)
    {
        if (motor == null)
        {
            TempData["Error"] = "Dữ liệu không hợp lệ.";
            return RedirectToAction("AddMotorManual");
        }

        await _catalogService.AddCatalogAsync(motor);
        TempData["Success"] = "Dữ liệu động cơ đã được thêm thành công!";
        return RedirectToAction("CatalogList");
    }
}
