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

    [ApiController]
    [Route("Input")]
    
    public class GearBoxController : Controller
    {
        private readonly PdfExportService _pdfExportService;

        public GearBoxController()
        {
            _pdfExportService = new PdfExportService();
        }
        [HttpPost("ExportPdf")]
        public IActionResult ExportToPdf([FromBody] TechnicalData content)
        {
            // if (request == null)
            // {
            //     return BadRequest("Invalid input data!");
            // }

            // // Perform calculation
            // var gearbox = new GearboxDesign(
            //     request.force,
            //     request.speed,
            //     request.diameter,
            //     request.serviceTime,
            //     request.loadN,
            //     request.Torchlist,
            //     request.tlist
            // );

            // var results = gearbox.Calculate();

            // Generate PDF
            Console.WriteLine(content);
            var pdfBytes = _pdfExportService.GenerateGearboxPdf(content);

            // // Return as file download
            // return Ok(pdfBytes);
            return File(pdfBytes, "application/pdf", $"GearboxDesign_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }


        [HttpPost("CalGear")]
        public IActionResult Calculate([FromBody] CalGearRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.transType))
            {
                return BadRequest("Thiếu dữ liệu đầu vào!");
            }

            // Truyền các tham số từ request
            GearboxDesign gearbox = new GearboxDesign(
                request.force,
                request.speed,
                request.diameter,
                request.serviceTime,
                request.loadN,
                request.Torchlist,
                request.tlist
            );

            // Tạo bộ truyền dựa trên lựa chọn của người dùng
            ITransmissionCalculation transmission;
            switch (request.transType.ToLower())
            {
                case "belt":
                    transmission = TransmissionFactory.CreateTransmission("belt", request.force, request.speed, request.diameter, request.serviceTime, request.loadN);
                    break;
                case "chain":
                    transmission = TransmissionFactory.CreateTransmission("chain", request.force, request.speed, request.diameter, request.serviceTime, request.loadN);
                    break;
                case "gear":
                    transmission = TransmissionFactory.CreateTransmission("gear", request.force, request.speed, request.diameter, request.serviceTime, request.loadN);
                    break;
                default:
                    return BadRequest("Loại bộ truyền không hợp lệ");
            }

            // Thực hiện tính toán
            //gearbox.Calculate();

            // Lấy kết quả
            var results = gearbox.Calculate();

            return Ok(results);
        }
    }

    // Model nhận dữ liệu từ request
    public class CalGearRequestModel
    {
        public string transType { get; set; }
        public double force { get; set; }
        public double speed { get; set; }
        public double diameter { get; set; }
        public double serviceTime { get; set; }
        public double loadN { get; set; }
        public double[] Torchlist { get; set; }
        public double[] tlist { get; set; }
    }

    public class TechnicalData
    {
        public double OverloadFactor { get; set; }
        public double OverallEfficiency { get; set; }
        public double RequiredMotorEfficiency { get; set; }
        public double RequiredMotorSpeed { get; set; }
        public double NsbSpeed { get; set; }
        public double Un { get; set; }
        public string MomenSoVongQuay { get; set; }
    }
}
