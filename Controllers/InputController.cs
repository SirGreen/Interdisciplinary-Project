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

        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Result()
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

        [HttpPost("SuggestNext")]
        public IActionResult SuggestNext([FromBody] PartialInputModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.field))
            {
                return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ!" });
            }

            var field = input.field.ToLower();
            var data = input.existingData ?? new Dictionary<string, double>();

            string suggestion = field switch
            {
                "force" => "Thường trong khoảng 100 - 500 N",
                "speed" => "0.5 - 2.5 m/s nếu F > 200",
                "diameter" => data.ContainsKey("force") && data["force"] > 300
                                ? "Chọn D lớn hơn 200 mm để chịu tải"
                                : "Thông thường D: 100 - 300 mm",
                "serviceTime" => "Từ 3 đến 10 năm là hợp lý",
                "workdays" => "Thường là 250 ngày/năm",
                "workhours" => "Thông dụng: 8 giờ/ngày",
                "startupmoment" => data.ContainsKey("loadmoment")
                                    ? $"Nên lớn hơn mômen tải ({data["loadmoment"]} Nm)"
                                    : "Chọn lớn hơn mômen tải",
                "loadmoment" => "Tùy tải, khoảng 100 - 500 Nm",
                "torch" => data.ContainsKey("force") ? $"Tải có thể ~ {data["force"] / 2} N" : "Tải khoảng 50 - 200 N",
                "t" => data.ContainsKey("torch") ? $"Thời gian tương ứng ~ {Math.Round(data["torch"] / 10.0, 1)} giây" : "Thời gian khoảng 10 - 30 giây",
                _ => "Gợi ý: nhập giá trị hợp lý theo hệ thống"
            };

            return Ok(new
            {
                field = input.field,
                suggestion
            });
        }


        [HttpPost("CalGear")]
        public IActionResult Calculate([FromBody] CalGearRequestModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.transType))
            {
                return BadRequest("Thiếu dữ liệu đầu vào!");
            }

            // Khởi tạo gearbox
            GearboxDesign gearbox = new GearboxDesign(
                request.force,
                request.speed,
                request.diameter,
                request.serviceTime,
                request.loadN,
                request.Torchlist,
                request.tlist
            );

            // Tạo bộ truyền theo loại được chọn
            ITransmissionCalculation transmission = TransmissionFactory.CreateTransmission("chain", 7.088, 2.578, 80.561, 2.578, 840232.2606);

            // Tính toán
            var gearboxResult = gearbox.Calculate();
            var transmissionResult = transmission.CalChain();
            // Tính toán
            var truyenResult = gearbox.CalcTruyen();

            // Trích xuất 4 giá trị
            var vatlieuBoTruyen = truyenResult.GetValueOrDefault("VatLieuBoTruyen");
            var dauVaoUngSuat = truyenResult.GetValueOrDefault("DauVaoUngSuat");
            var ungSuatTiepXuc = truyenResult.GetValueOrDefault("UngSuatTiepXucChoPhep");
            

            return Ok(new
            {
                // Gearbox results
                overloadFactor = gearboxResult["overloadFactor"],
                overallEfficiency = gearboxResult["overallEfficiency"],
                requiredMotorEfficiency = gearboxResult["requiredMotorEfficiency"],
                requiredMotorSpeed = gearboxResult["requiredMotorSpeed"],
                NsbSpeed = gearboxResult["NsbSpeed"],
                Un = gearboxResult["Un"],
                MomenSoVongQuay = gearboxResult["MomenSoVongQuay"],

                // Bo truyen B16–B19
                VatLieuBoTruyen = vatlieuBoTruyen,
                DauVaoUngSuat = dauVaoUngSuat,
                UngSuatTiepXucChoPhep = ungSuatTiepXuc,

                // Gán giá trị mới cho form Thiết kế đĩa xích
                pitch = transmissionResult.ContainsKey("BuocXich_p") ? transmissionResult["BuocXich_p"] : null,
                shaftDistance = transmissionResult.ContainsKey("KhoangCachTruc_aStan") ? transmissionResult["KhoangCachTruc_aStan"] : null,
                chainSafe = transmissionResult.ContainsKey("XichAnToan") ? transmissionResult["XichAnToan"] : null,
                contactStrength = transmissionResult.ContainsKey("DoBenTiepXuc_Oh") ? transmissionResult["DoBenTiepXuc_Oh"] : null,
                shaftForce = transmissionResult.ContainsKey("LucTacDungTrenTruc_Frk") ? transmissionResult["LucTacDungTrenTruc_Frk"] : null,
                diskDiameterCalc = transmissionResult.ContainsKey("DuongKinhDiaXich_TinhToan") ? transmissionResult["DuongKinhDiaXich_TinhToan"] : null

            });
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
        public MotorCatalog Motor { get; set; }
    }
}
