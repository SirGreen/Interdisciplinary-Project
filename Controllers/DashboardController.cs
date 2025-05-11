using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.IO.Pipelines;

public class DashboardController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly IExtractionService _extractionService;
    private readonly Cloudinary _cloudinary;

    public DashboardController(ICatalogService catalogService, IExtractionService extractionService, IConfiguration configuration)
    {
        _catalogService = catalogService;
        _extractionService = extractionService;

        // Lấy thông tin Cloudinary từ appsettings.json
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("api/motors/count-by-type")]
    public async Task<IActionResult> GetMotorCountsByType()
    {
        try
        {
            // Fetch all records first
            var catalogs = await _catalogService.GetAllAsync();

            // Group and count by motor_type using LINQ
            var counts = catalogs
                .GroupBy(m => m.motor_type)
                .ToDictionary(g => g.Key, g => g.Count());

            return Ok(counts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("api/motors/filter-catalogs-by-efficiency")]
    public async Task<IActionResult> FilterCatalogsByEfficiency()
    {
        // Step 1: Fetch all motor catalogs
        var catalogs = await _catalogService.GetAllAsync();

        // Step 2: Parse the `efficiency_full` into a numeric value and group by efficiency range
        var efficiencyRanges = catalogs
            .Select(catalog => double.TryParse(catalog.efficiency_full, out var efficiency) ? efficiency : 0)
            .GroupBy(efficiency => (int)(efficiency / 10))  // Group by efficiency ranges (0-10, 10-20, ...)
            .Select(group => new
            {
                Range = $"{group.Key * 10}% - {(group.Key + 1) * 10}%",
                Count = group.Count()
            })
            .OrderBy(range => range.Range)
            .ToList();

        // Return the results
        return Ok(efficiencyRanges);
    }

    [HttpGet]
    [Route("api/motors/filter-catalogs-by-output-hp")]
    public async Task<IActionResult> FilterCatalogsByOutputHp()
    {
        // Fetch all motor catalogs
        var catalogs = await _catalogService.GetAllAsync();

        // Parse the Output HP and calculate min and max
        var outputHpValues = catalogs
            .Select(catalog => double.TryParse(catalog.output_hp, out var outputHp) ? outputHp : 0)
            .ToList();

        double minHp = outputHpValues.Min();
        double maxHp = outputHpValues.Max();

        // Calculate the range size
        double rangeSize = (maxHp - minHp) / 10;

        // Group and count engines in each range
        var outputHpRanges = Enumerable.Range(0, 10)
            .Select(i =>
            {
                double rangeMin = minHp + i * rangeSize;
                double rangeMax = minHp + (i + 1) * rangeSize;

                var countInRange = outputHpValues.Count(hp => hp >= rangeMin && hp < rangeMax);

                // Calculate the average of the range
                var avgHp = (rangeMin + rangeMax) / 2;

                return new
                {
                    Range = $"{rangeMin:F2} - {rangeMax:F2}",
                    Count = countInRange,
                    AverageHp = avgHp
                };
            })
            .ToList();

        return Ok(outputHpRanges);
    }

    // Query to count engines by Full Load RPM with dynamic 10 range
    [HttpGet]
    [Route("api/motors/filter-catalogs-by-full-load-rpm")]
    public async Task<IActionResult> FilterCatalogsByFullLoadRpm()
    {
        // Fetch all motor catalogs
        var catalogs = await _catalogService.GetAllAsync();

        // Parse the Full Load RPM and calculate min and max
        var fullLoadRpmValues = catalogs
            .Select(catalog => double.TryParse(catalog.full_load_rpm, out var fullLoadRpm) ? fullLoadRpm : 0)
            .ToList();

        double minRpm = fullLoadRpmValues.Min();
        double maxRpm = fullLoadRpmValues.Max();

        // Calculate the range size
        double rangeSize = (maxRpm - minRpm) / 10;

        // Group and count engines in each range
        var fullLoadRpmRanges = Enumerable.Range(0, 10)
            .Select(i =>
            {
                double rangeMin = minRpm + i * rangeSize;
                double rangeMax = minRpm + (i + 1) * rangeSize;

                var countInRange = fullLoadRpmValues.Count(rpm => rpm >= rangeMin && rpm < rangeMax);

                // Calculate the average of the range
                var avgRpm = (rangeMin + rangeMax) / 2;

                return new
                {
                    Range = $"{rangeMin:F2} RPM - {rangeMax:F2} RPM",
                    Count = countInRange,
                    AverageRpm = avgRpm
                };
            })
            .ToList();

        return Ok(fullLoadRpmRanges);
    }
}
