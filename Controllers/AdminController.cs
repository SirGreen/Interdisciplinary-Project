using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

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
        return View("AddMotorManual", extractedData);
    }


    [HttpPost]
    public async Task<IActionResult> AddMotorManual(MotorCatalog catalog, List<IFormFile>? imageFiles)
    {
        // Gán giá trị mặc định "Unknown" nếu trường bị bỏ trống
        catalog.Power = string.IsNullOrWhiteSpace(catalog.Power) ? "Unknown" : catalog.Power;
        catalog.Voltage = string.IsNullOrWhiteSpace(catalog.Voltage) ? "Unknown" : catalog.Voltage;
        catalog.Poles = string.IsNullOrWhiteSpace(catalog.Poles) ? "Unknown" : catalog.Poles;
        catalog.FrameSize = string.IsNullOrWhiteSpace(catalog.FrameSize) ? "Unknown" : catalog.FrameSize;
        catalog.Protection = string.IsNullOrWhiteSpace(catalog.Protection) ? "Unknown" : catalog.Protection;
        catalog.Standard = string.IsNullOrWhiteSpace(catalog.Standard) ? "Unknown" : catalog.Standard;
        catalog.Material = string.IsNullOrWhiteSpace(catalog.Material) ? "Unknown" : catalog.Material;
        catalog.MountingType = string.IsNullOrWhiteSpace(catalog.MountingType) ? "Unknown" : catalog.MountingType;
        catalog.ShaftDiameter = string.IsNullOrWhiteSpace(catalog.ShaftDiameter) ? "Unknown" : catalog.ShaftDiameter;
        catalog.Footprint = string.IsNullOrWhiteSpace(catalog.Footprint) ? "Unknown" : catalog.Footprint;
        catalog.Technology = string.IsNullOrWhiteSpace(catalog.Technology) ? "Unknown" : catalog.Technology;
        catalog.URL = string.IsNullOrWhiteSpace(catalog.URL) ? "Unknown" : catalog.URL;

        // Xử lý upload ảnh
        if (imageFiles != null && imageFiles.Count > 0)
        {
            catalog.ImageUrls = new List<string>(); // Đảm bảo danh sách không null

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

    [HttpPost]
    public async Task<IActionResult> DeleteMotorCatalog(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Error"] = "ID không hợp lệ.";
            return RedirectToAction("CatalogList");
        }

        try
        {
            await _catalogService.DeleteCatalogAsync(id);
            TempData["Success"] = "Catalog đã được xóa thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi xóa catalog: " + ex.Message;
        }

        return RedirectToAction("CatalogList");
    }

    [HttpGet]
    [Route("api/filter-catalogs")]
    public async Task<IActionResult> FilterCatalogs(double requiredMotorEfficiency, double requiredMotorSpeed)
    {
        var catalogs = await _catalogService.GetAllAsync();

        // Lọc danh sách động cơ phù hợp
        var filteredCatalogs = catalogs
            .Where(m =>
            {
                double motorPower = ExtractKW(m.Power);
                int motorPoles = ExtractPoles(m.Poles);
                double baseSpeed = GetSpeedFromPoles(motorPoles);

                // Khoảng tốc độ quay hợp lệ
                double minSpeed = requiredMotorSpeed * 0.96;
                double maxSpeed = requiredMotorSpeed * 1.04;

                // In thông số ra console
                Console.WriteLine($"Motor Power (KW): {motorPower}");
                Console.WriteLine($"Motor Poles: {motorPoles}");
                Console.WriteLine($"Base Speed (RPS): {baseSpeed}");
                Console.WriteLine($"Min Speed (RPS): {minSpeed}");
                Console.WriteLine($"Max Speed (RPS): {maxSpeed}");

                return motorPower >= requiredMotorEfficiency && baseSpeed >= minSpeed && baseSpeed <= maxSpeed;
            })
            .ToList();

        Console.WriteLine($"Số động cơ phù hợp: {filteredCatalogs.Count}");
        return Ok(filteredCatalogs);
    }

    // Hàm trích xuất số KW từ chuỗi dạng "0.75kw/1HP" hoặc "22KW/30HP"
    private double ExtractKW(string powerString)
    {
        if (string.IsNullOrEmpty(powerString)) return 0;

        var match = Regex.Match(powerString, @"(\d+(\.\d+)?)\s*(KW|kw|kW)");
        return match.Success ? double.Parse(match.Groups[1].Value) : 0;
    }

    private int ExtractPoles(string polesString)
    {
        if (string.IsNullOrEmpty(polesString)) return 0;

        var match = Regex.Match(polesString, @"(\d+)P", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }


    private double GetSpeedFromPoles(int poles)
    {
        return poles switch
        {
            2 => 2850 / 60.0,  // 47.5 vòng/giây
            4 => 1425 / 60.0,  // 23.75 vòng/giây
            6 => 950 / 60.0,   // 15.83 vòng/giây
            8 => 720 / 60.0,   // 12 vòng/giây
            _ => 0             // Nếu không xác định số cực, trả về 0
        };
    }


}
