using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

public class AdminController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly IExtractionService _extractionService;
    private readonly Cloudinary _cloudinary;

    public AdminController(ICatalogService catalogService, IExtractionService extractionService, IConfiguration configuration)
    {
        _catalogService = catalogService;
        _extractionService = extractionService;

        // Lấy thông tin Cloudinary từ appsettings.json
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
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

        // 🛠 Render view để người dùng chỉnh sửa thông tin
        return View("ConfirmCatalog", extractedData);
    }


    [HttpPost]
    public async Task<IActionResult> ConfirmCatalog(MotorCatalog catalog, List<IFormFile>? imageFiles)
    {
        if (imageFiles != null && imageFiles.Count > 0)
        {
            foreach (var imageFile in imageFiles)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                    PublicId = $"motor_catalog/{Guid.NewGuid()}",
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    catalog.ImageUrls.Add(uploadResult.SecureUrl.ToString());
                }
            }
        }

        await _catalogService.AddCatalogAsync(catalog);

        TempData["Success"] = "Thông tin đã được cập nhật!";
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
