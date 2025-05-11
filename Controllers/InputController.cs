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
            // Generate PDF
            Console.WriteLine(content);
            var pdfBytes = _pdfExportService.GenerateGearboxPdf(content);

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

            // Tạo bộ truyền theo loại được chọn (ở đây tạm thời dùng 'chain' và các thông số giả định)
            ITransmissionCalculation transmission = TransmissionFactory.CreateTransmission("chain", 7.088, 2.578, 80.561, 2.578, 840232.2606);

            // Tính toán hộp giảm tốc
            var gearboxResult = gearbox.Calculate();
            var kq = (MomenKetQua)gearboxResult["MomenSoVongQuay"];

            var dict = new Dictionary<string, double>
            {
                ["n1"] = kq.N1,
                ["u1"] = kq.U1,
                ["T1"] = kq.T1,
                ["n2"] = kq.N2,
                ["u2"] = kq.U2,
                ["T2"] = kq.T2
            };

            string momencs = kq.MoTa;
            double Lh = request.serviceTime * 300 * request.loadN * 8;

            // Tính bộ truyền bánh răng
            var truyenResult = gearbox.CalcBoTruyen(dict, Lh);

            var vatlieuBoTruyen = truyenResult.GetValueOrDefault("VatLieuBoTruyen");
            var dauVaoUngSuat = truyenResult.GetValueOrDefault("DauVaoUngSuat");
            var ungSuatTiepXuc = truyenResult.GetValueOrDefault("UngSuatTiepXucChoPhep");
            var ungSuatUon = truyenResult.GetValueOrDefault("UngSuatUonChoPhep");
            
            // Xử lý ép kiểu an toàn sang Dictionary<string, object>
            object tinhBanhRangCapNhanh = null;
            object tinhBanhRangCapCham = null;

            if (truyenResult.TryGetValue("TinhBangRangCapNhanh", out var capNhanhRaw) && capNhanhRaw is Dictionary<string, double> capNhanhDict)
            {
                tinhBanhRangCapNhanh = capNhanhDict.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            }

            if (truyenResult.TryGetValue("TinhBangRangCapCham", out var capChamRaw) && capChamRaw is Dictionary<string, double> capChamDict)
            {
                tinhBanhRangCapCham = capChamDict.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            }
            // Tính bộ truyền xích
            var transmissionResult = transmission.CalChain();

            return Ok(new
            {
                // Kết quả gearbox - Bảng 1
                overloadFactor = gearboxResult["overloadFactor"],
                overallEfficiency = gearboxResult["overallEfficiency"],
                requiredMotorEfficiency = gearboxResult["requiredMotorEfficiency"],
                requiredMotorSpeed = gearboxResult["requiredMotorSpeed"],
                NsbSpeed = gearboxResult["NsbSpeed"],
                Un = gearboxResult["Un"],
                MomenSoVongQuay = momencs,

                // Bộ truyền bánh răng
                VatLieuBoTruyen = vatlieuBoTruyen,
                DauVaoUngSuat = dauVaoUngSuat,
                UngSuatTiepXucChoPhep = ungSuatTiepXuc,
                UngSuatUonChoPhep = ungSuatUon,
                TinhBanhRangCapNhanh = tinhBanhRangCapNhanh,
                TinhBanhRangCapCham = tinhBanhRangCapCham,

                // Bộ truyền xích - chuẩn key cho renderChainTable
                soRangDan = transmissionResult.GetValueOrDefault("soRangDan"),
                soRangBiDan = transmissionResult.GetValueOrDefault("soRangBiDan"),
                pitch = transmissionResult.GetValueOrDefault("pitch"),
                shaftDistance = transmissionResult.GetValueOrDefault("shaftDistance"),
                chainSafe = transmissionResult.GetValueOrDefault("chainSafe"),
                contactStrength = transmissionResult.GetValueOrDefault("contactStrength"),
                shaftForce = transmissionResult.GetValueOrDefault("shaftForce"),
                diskDiameterCalc = transmissionResult.GetValueOrDefault("diskDiameterCalc"),
                ongLot = transmissionResult.GetValueOrDefault("ongLot"),
                duongKinhChot = transmissionResult.GetValueOrDefault("duongKinhChot"),
                soMatXich = transmissionResult.GetValueOrDefault("soMatXich"),
                diaDan = transmissionResult.GetValueOrDefault("diaDan"),
                diaBiDan = transmissionResult.GetValueOrDefault("diaBiDan"),
                vatLieuDia1 = transmissionResult.GetValueOrDefault("vatLieuDia1"),
                vatLieuDia2 = transmissionResult.GetValueOrDefault("vatLieuDia2")
            });
        }

    }
}
