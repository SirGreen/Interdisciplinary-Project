using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ExtractionService : IExtractionService
{
    private readonly HttpClient _httpClient;
    private const string HuggingFaceApiUrl = "https://api-inference.huggingface.co/models/letran1110/vit5_trash_classifier";
    private const string ApiToken = "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"; // Thay bằng token của bạn

    public ExtractionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiToken}");
    }

    public async Task<MotorCatalog> ExtractDataFromWebAsync(string url)
    {
        string pageContent = await CrawlWebContent(url);
        if (string.IsNullOrWhiteSpace(pageContent))
        {
            throw new Exception("Không lấy được nội dung trang web.");
        }

        return await ExtractDataFromTextAsync(url, pageContent);
    }

    private async Task<string> CrawlWebContent(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Lỗi khi tải trang web: {response.StatusCode}");
            }

            string html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.InnerText.Trim();
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi crawl web: {ex.Message}");
        }
    }

    private async Task<MotorCatalog> ExtractDataFromTextAsync(string url, string inputText)
    {
        var prompt = $"Hãy trích xuất thông tin động cơ từ văn bản sau và trả về JSON hợp lệ:\n\n{inputText}\n\n" +
                     "Định dạng JSON: { \"Power\": \"\", \"Model\": \"\", \"Voltage\": \"\", \"Poles\": \"\", \"Standard\": \"\", \"Material\": \"\" }";

        var requestBody = new
        {
            inputs = prompt,
            parameters = new { max_length = 512 }
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(HuggingFaceApiUrl, jsonContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Lỗi khi gọi API Hugging Face: {response.StatusCode}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new Exception("API trả về dữ liệu rỗng.");
        }

        try
        {
            Console.WriteLine($"Output từ model: {responseString}");

            var extractedData = ParseOutput(responseString);

            // Kiểm tra 3 thông tin quan trọng, nếu thiếu thì báo lỗi
            if (!extractedData.ContainsKey("Power") || !extractedData.ContainsKey("Model") || !extractedData.ContainsKey("Voltage"))
            {
                throw new Exception("API không trả về đủ thông tin cần thiết (Power, Model, Voltage).");
            }

            return new MotorCatalog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                URL = url,
                Power = extractedData["Power"],
                Model = extractedData["Model"],
                Voltage = extractedData["Voltage"],
                Speed = extractedData["Speed"],
                Standard = extractedData.GetValueOrDefault("Standard", "N/A"),
                Material = extractedData.GetValueOrDefault("Material", "N/A"),
                Protection = extractedData.GetValueOrDefault("Protection", "N/A"),
                ShaftDiameter = extractedData.GetValueOrDefault("ShaftDiameter", "N/A"),
                FrameSize = extractedData.GetValueOrDefault("FrameSize", "N/A"),
                MountingType = extractedData.GetValueOrDefault("MountingType", "N/A"),
                Footprint = extractedData.GetValueOrDefault("Footprint", "N/A"),
                Technology = extractedData.GetValueOrDefault("Technology", "N/A")
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi parse JSON từ model: {ex.Message}");
        }
    }

    private Dictionary<string, string> ParseOutput(string text)
    {
        try
        {
            // 🛠 Bước 1: Lấy chuỗi JSON từ mảng trả về của model
            var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(text);
            if (jsonData == null || jsonData.Count == 0 || !jsonData[0].ContainsKey("generated_text"))
            {
                throw new Exception("❌ API không trả về dữ liệu hợp lệ.");
            }

            // 🔍 Bước 2: Lấy nội dung trong `generated_text`
            string rawGeneratedText = jsonData[0]["generated_text"];

            // 🔍 Bước 3: Dùng regex để trích xuất từng thông tin
            string ExtractValue(string key)
            {
                var match = Regex.Match(rawGeneratedText, $@"\""{key}\"":\s*\""(.*?)\""");
                return match.Success ? match.Groups[1].Value.Trim() : "N/A";
            }

            // 📌 Trích xuất các trường dữ liệu
            return new Dictionary<string, string>
        {
            { "Power", ExtractValue("Power") },
            { "Model", ExtractValue("Model") },
            { "Voltage", ExtractValue("Voltage") },
            { "Poles", ExtractValue("Poles") },
            { "Standard", ExtractValue("Standard") },
            { "Material", ExtractValue("Material") },
            { "Protection", ExtractValue("Protection") },
            { "ShaftDiameter", ExtractValue("Shaft Diameter") }, // Chú ý key có khoảng trắng
            { "FrameSize", ExtractValue("Frame Size") },
            { "MountingType", ExtractValue("Mounting Type") },
            { "Footprint", ExtractValue("Footprint") },
            { "Technology", ExtractValue("Technology") }
        };
        }
        catch (Exception ex)
        {
            throw new Exception($"❌ Lỗi khi parse output từ model: {ex.Message}");
        }
    }
}
