using System;
using System.Net;
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
    private const string ApiToken = "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";

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

        var result = await TryExtractData(url, pageContent);
        Console.WriteLine(result != null ? "Trích xuất dữ liệu thành công!" : "Trích xuất dữ liệu thất bại!");
        if (result == null) result = new MotorCatalog
        {
            Id = ObjectId.GenerateNewId().ToString(),
            URL = url,
            Power = "N/A",
            Model = "N/A",
            Voltage = "N/A",
            Speed = "N/A",
            Standard = "N/A",
            Technology = "N/A",
            Material = "N/A",
            Protection = "N/A",
            FrameSize = "N/A",
            MountingType = "N/A",
            ShaftDiameter = "N/A",
            Footprint = "N/A"
        };
        return result;
    }

    private async Task<MotorCatalog?> TryExtractData(string url, string pageContent, int retries = 3)
    {
        int attempt = 0;
        MotorCatalog? result = null;
        while (attempt < retries && result == null)
        {
            result = await ExtractDataFromTextAsync(url, pageContent);
            if (result == null)
            {
                attempt++;
                Console.WriteLine($"Thử lại lần {attempt}...");
                await Task.Delay(2000); // Đợi 2 giây trước khi thử lại
            }
        }
        return result;
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

    private async Task<MotorCatalog?> ExtractDataFromTextAsync(string url, string inputText)
    {
        try
        {
            var prompt = $"Hãy trích xuất thông tin động cơ từ văn bản sau và trả về JSON hợp lệ:\n\n{inputText}\n\n" +
                         "Định dạng JSON: { \"Power\": \"\", \"Model\": \"\", \"Voltage\": \"\", \"Speed\": \"\", \"Standard\": \"\", \"Material\": \"\", \"Protection\": \"\" }";

            var requestBody = new
            {
                inputs = prompt,
                parameters = new { max_length = 512 }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            // Gọi API HuggingFace và nhận kết quả
            var responseString = await CallHuggingFaceApi(jsonContent);
            if (responseString == null)
            {
                Console.WriteLine("Không nhận được dữ liệu từ HuggingFace API.");
                return null;
            }

            Console.WriteLine($"Output từ model: {responseString}");

            var extractedData = ParseOutput(responseString);

            if (!extractedData.ContainsKey("Power") || !extractedData.ContainsKey("Model") || !extractedData.ContainsKey("Voltage"))
            {
                Console.WriteLine("API không trả đủ thông tin Power, Model, Voltage.");
                return null;
            }

            return new MotorCatalog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                URL = url,
                Power = extractedData.GetValueOrDefault("Power", "N/A"),
                Model = extractedData.GetValueOrDefault("Model", "N/A"),
                Voltage = extractedData.GetValueOrDefault("Voltage", "N/A"),
                Speed = extractedData.GetValueOrDefault("Speed", "N/A"),
                Standard = extractedData.GetValueOrDefault("Standard", "N/A"),
                Material = extractedData.GetValueOrDefault("Material", "N/A"),
                Protection = extractedData.GetValueOrDefault("Protection", "N/A"),
                Technology = extractedData.GetValueOrDefault("Technology", "N/A"),
                FrameSize = extractedData.GetValueOrDefault("FrameSize", "N/A"),
                MountingType = extractedData.GetValueOrDefault("MountingType", "N/A"),
                ShaftDiameter = extractedData.GetValueOrDefault("ShaftDiameter", "N/A"),
                Footprint = extractedData.GetValueOrDefault("Footprint", "N/A")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi ExtractDataFromTextAsync: {ex.Message}");
            return null;
        }
    }
    private Dictionary<string, string> ParseOutput(string text)
    {
        var result = new Dictionary<string, string>();

        try
        {
            var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(text);
            if (jsonData == null || jsonData.Count == 0 || !jsonData[0].ContainsKey("generated_text"))
            {
                Console.WriteLine("API không trả về dữ liệu hợp lệ.");
                return result;
            }

            string rawGeneratedText = jsonData[0]["generated_text"];

            string ExtractValue(string key)
            {
                var match = Regex.Match(rawGeneratedText, $@"\""{key}\"":\s*\""(.*?)\""");
                return match.Success ? match.Groups[1].Value.Trim() : "N/A";
            }

            var keys = new[]
            {
            "Power", "Model", "Voltage", "Speed", "Standard", "Material",
            "Protection", "Shaft Diameter", "FrameSize", "MountingType",
            "Footprint", "Technology"
        };

            foreach (var key in keys)
            {
                result[key.Replace(" ", "")] = ExtractValue(key);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi parse output từ model: {ex.Message}");
        }

        return result;
    }


    private async Task<string?> CallHuggingFaceApi(StringContent jsonContent)
    {
        try
        {
            using var response = await _httpClient.PostAsync(HuggingFaceApiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Console.WriteLine($"HuggingFace API trả về lỗi {response.StatusCode} - Service Unavailable.");
                }
                else
                {
                    Console.WriteLine($"Lỗi gọi API HuggingFace: {response.StatusCode} - {response.ReasonPhrase}");
                }
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
            {
                Console.WriteLine("API trả về dữ liệu rỗng.");
                return null;
            }

            return responseString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi gọi HuggingFace API: {ex.Message}");
            return null;
        }
    }
}
