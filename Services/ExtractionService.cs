using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ExtractionService : IExtractionService
{
    private readonly HttpClient _httpClient;

    public ExtractionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private MotorCatalog CreateDefaultMotorCatalog(string url, string errorMessage)
    {
        return new MotorCatalog
        {
            Id = errorMessage,
            URL = url,
            Technology = "N/A",
            Power = "N/A",
            Model = "N/A",
            FrameSize = "N/A",
            Poles = "N/A",
            Standard = "N/A",
            Voltage = "N/A",
            MountingType = "N/A",
            Material = "N/A",
            Protection = "N/A",
            ShaftDiameter = "N/A",
            Footprint = "N/A"
        };
    }

    public async Task<MotorCatalog> ExtractDataFromWebAsync(string url)
    {
        var requestUrl = $"http://your-ai-api.com/extract?url={url}";

        using var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            return CreateDefaultMotorCatalog(url, "Lỗi khi gọi API"); // Trả về dữ liệu mặc định
        }

        var responseString = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseString))
        {
            return CreateDefaultMotorCatalog(url, "API trả về dữ liệu rỗng");
        }

        try
        {
            var extractedData = JsonConvert.DeserializeObject<MotorCatalog>(responseString);
            return extractedData ?? CreateDefaultMotorCatalog(url, "Dữ liệu API không đúng định dạng");
        }
        catch (JsonException)
        {
            return CreateDefaultMotorCatalog(url, "Lỗi khi parse JSON từ API");
        }
    }
}
