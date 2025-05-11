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
        // Set default values for required fields if they are empty
        catalog.motor_id = string.IsNullOrWhiteSpace(catalog.motor_id) ? "Unknown" : catalog.motor_id;
        catalog.brand = string.IsNullOrWhiteSpace(catalog.brand) ? "Unknown" : catalog.brand;
        catalog.category = string.IsNullOrWhiteSpace(catalog.category) ? "Motor" : catalog.category;
        catalog.current_380v = string.IsNullOrWhiteSpace(catalog.current_380v) ? "0" : catalog.current_380v;
        catalog.current_400v = string.IsNullOrWhiteSpace(catalog.current_400v) ? "0" : catalog.current_400v;
        catalog.current_415v = string.IsNullOrWhiteSpace(catalog.current_415v) ? "0" : catalog.current_415v;
        catalog.current_lrc = string.IsNullOrWhiteSpace(catalog.current_lrc) ? "0" : catalog.current_lrc;
        catalog.efficiency_1_2 = string.IsNullOrWhiteSpace(catalog.efficiency_1_2) ? "0" : catalog.efficiency_1_2;
        catalog.efficiency_3_4 = string.IsNullOrWhiteSpace(catalog.efficiency_3_4) ? "0" : catalog.efficiency_3_4;
        catalog.efficiency_full = string.IsNullOrWhiteSpace(catalog.efficiency_full) ? "0" : catalog.efficiency_full;
        catalog.frame_size = string.IsNullOrWhiteSpace(catalog.frame_size) ? "Unknown" : catalog.frame_size;
        catalog.full_load_rpm = string.IsNullOrWhiteSpace(catalog.full_load_rpm) ? "0" : catalog.full_load_rpm;
        catalog.motor_type = string.IsNullOrWhiteSpace(catalog.motor_type) ? "Unknown" : catalog.motor_type;
        catalog.output_hp = string.IsNullOrWhiteSpace(catalog.output_hp) ? "0" : catalog.output_hp;
        catalog.output_kw = string.IsNullOrWhiteSpace(catalog.output_kw) ? "0" : catalog.output_kw;
        catalog.power_factor_1_2 = string.IsNullOrWhiteSpace(catalog.power_factor_1_2) ? "0" : catalog.power_factor_1_2;
        catalog.power_factor_3_4 = string.IsNullOrWhiteSpace(catalog.power_factor_3_4) ? "0" : catalog.power_factor_3_4;
        catalog.power_factor_full = string.IsNullOrWhiteSpace(catalog.power_factor_full) ? "0" : catalog.power_factor_full;
        catalog.product_name = string.IsNullOrWhiteSpace(catalog.product_name) ? "Unknown" : catalog.product_name;
        catalog.source_page = string.IsNullOrWhiteSpace(catalog.source_page) ? "Unknown" : catalog.source_page;
        catalog.torque_break_down = string.IsNullOrWhiteSpace(catalog.torque_break_down) ? "0" : catalog.torque_break_down;
        catalog.torque_full = string.IsNullOrWhiteSpace(catalog.torque_full) ? "0" : catalog.torque_full;
        catalog.torque_locked_rotor = string.IsNullOrWhiteSpace(catalog.torque_locked_rotor) ? "0" : catalog.torque_locked_rotor;
        catalog.torque_pull_up = string.IsNullOrWhiteSpace(catalog.torque_pull_up) ? "0" : catalog.torque_pull_up;
        catalog.torque_rotor_gd2 = string.IsNullOrWhiteSpace(catalog.torque_rotor_gd2) ? "0" : catalog.torque_rotor_gd2;
        catalog.url = string.IsNullOrWhiteSpace(catalog.url) ? "Unknown" : catalog.url;
        catalog.weight_kg = string.IsNullOrWhiteSpace(catalog.weight_kg) ? "0" : catalog.weight_kg;

        // Handle image upload
        if (imageFiles != null && imageFiles.Count > 0)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFiles[0].FileName, imageFiles[0].OpenReadStream()),
                PublicId = $"motor_catalog/{Guid.NewGuid()}",
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                catalog.image_url = uploadResult.SecureUrl.ToString();
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
        foreach (var item in catalogs)
        {
            Console.WriteLine($"Id: {item.Id}, Motor Type: {item.motor_type}");
        }
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
    public async Task<IActionResult> FilterCatalogs(double requiredMotorEfficiency, double NsbSpeed)
    {
        var catalogs = await _catalogService.GetAllAsync();

        // Khoảng tốc độ quay hợp lệ
        double minSpeed = NsbSpeed * 0.96;
        double maxSpeed = NsbSpeed * 1.04;

        // In thông số ra console
        Console.WriteLine($"Min Speed (RPS): {minSpeed}");
        Console.WriteLine($"Max Speed (RPS): {maxSpeed}");
        // Lọc danh sách động cơ phù hợp
        var filteredCatalogs = catalogs
            .Where(m =>
            {
                double motorPower = double.Parse(m.output_kw);
                double motorSpeed = double.Parse(m.full_load_rpm);
                return motorPower >= requiredMotorEfficiency && motorSpeed >= minSpeed && motorSpeed <= maxSpeed;
            })
            .ToList();

        Console.WriteLine($"Số động cơ phù hợp: {filteredCatalogs.Count}");
        return Ok(filteredCatalogs);
    }
}
