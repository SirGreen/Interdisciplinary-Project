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
using DADN.Services;

public class ExtractionService : IExtractionService
{
    private readonly HttpClient _httpClient;
    private const string HuggingFaceApiUrl = "https://api-inference.huggingface.co/models/letran1110/vit5_motor_extractor";
    private const string ApiToken = "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";

    public ExtractionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (!ApiConfig.UseCustomApi)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiToken}");
        }
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
            motor_id = url,
            brand = "Unknown",
            category = "Motor",
            current_380v = "0",
            current_400v = "0",
            current_415v = "0",
            current_lrc = "0",
            efficiency_1_2 = "0",
            efficiency_3_4 = "0",
            efficiency_full = "0",
            frame_size = "Unknown",
            full_load_rpm = "0",
            image_url = "",
            motor_type = "Unknown",
            output_hp = "0",
            output_kw = "0",
            power_factor_1_2 = "0",
            power_factor_3_4 = "0",
            power_factor_full = "0",
            product_name = "Unknown",
            source_page = url,
            torque_break_down = "0",
            torque_full = "0",
            torque_locked_rotor = "0",
            torque_pull_up = "0",
            torque_rotor_gd2 = "0",
            url = url,
            weight_kg = "0"
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
            if (ApiConfig.UseCustomApi)
            {
                var requestBody = new
                {
                    text = inputText
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var responseString = await CallCustomApi(jsonContent);
                if (responseString == null)
                {
                    Console.WriteLine("Không nhận được dữ liệu từ API.");
                    return null;
                }

                Console.WriteLine($"Output từ model: {responseString}");
                var extractedData = ParseOutput(responseString);
                return MapToMotorCatalog(url, extractedData);
            }
            else
            {
                var prompt = $"Hãy trích xuất thông tin động cơ từ văn bản sau và trả về JSON hợp lệ:\n\n{inputText}\n\n" +
                            "Định dạng JSON: { \"Power\": \"\", \"Model\": \"\", \"Voltage\": \"\", \"Speed\": \"\", \"Standard\": \"\", " +
                            "\"Material\": \"\", \"Protection\": \"\", \"Shaft Diameter\": \"\", \"FrameSize\": \"\", \"MountingType\": \"\", " +
                            "\"Footprint\": \"\", \"Technology\": \"\" }";

                var requestBody = new
                {
                    inputs = prompt,
                    parameters = new { max_length = 512 }
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var responseString = await CallHuggingFaceApi(jsonContent);
                if (responseString == null)
                {
                    Console.WriteLine("Không nhận được dữ liệu từ HuggingFace API.");
                    return null;
                }

                Console.WriteLine($"Output từ model: {responseString}");
                var extractedData = ParseOutput(responseString);
                return MapToMotorCatalog(url, extractedData);
            }
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
            if (ApiConfig.UseCustomApi)
            {
                // Xử lý trường hợp text là một string chứa JSON
                if (text.StartsWith("\"") && text.EndsWith("\""))
                {
                    // Bỏ dấu ngoặc kép ở đầu và cuối
                    text = text.Substring(1, text.Length - 2);
                }

                // Parse từng cặp key-value
                var pairs = text.Split(',');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim().Trim('"');
                        var value = parts[1].Trim().Trim('"');
                        result[key] = value;
                    }
                }
            }
            else
            {
                // Parse từ HuggingFace API response
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
                    result[key] = ExtractValue(key);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi parse output từ model: {ex.Message}");
        }

        return result;
    }

    private async Task<string?> CallCustomApi(StringContent jsonContent)
    {
        try
        {
            using var response = await _httpClient.PostAsync(ApiConfig.ApiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Lỗi gọi API: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
            {
                Console.WriteLine("API trả về dữ liệu rỗng.");
                return null;
            }

            var responseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
            return responseObj?["result"];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
            return null;
        }
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

    private MotorCatalog MapToMotorCatalog(string url, Dictionary<string, string> extractedData)
    {
        return new MotorCatalog
        {
            Id = ObjectId.GenerateNewId().ToString(),
            motor_id = url,
            brand = "Unknown",
            category = "Motor",
            current_380v = "0",
            current_400v = "0",
            current_415v = "0",
            current_lrc = "0",
            efficiency_1_2 = "0",
            efficiency_3_4 = "0",
            efficiency_full = "0",
            frame_size = extractedData.GetValueOrDefault("FrameSize", extractedData.GetValueOrDefault("Model", "Unknown")),
            full_load_rpm = extractedData.GetValueOrDefault("Speed", "0"),
            image_url = "",
            motor_type = extractedData.GetValueOrDefault("Technology", extractedData.GetValueOrDefault("Standard", "Unknown")),
            output_hp = "0",
            output_kw = extractedData.GetValueOrDefault("Power", "0"),
            power_factor_1_2 = "0",
            power_factor_3_4 = "0",
            power_factor_full = "0",
            product_name = extractedData.GetValueOrDefault("Model", "Unknown"),
            source_page = url,
            torque_break_down = "0",
            torque_full = "0",
            torque_locked_rotor = "0",
            torque_pull_up = "0",
            torque_rotor_gd2 = "0",
            url = url,
            weight_kg = "0"
        };
    }
}
