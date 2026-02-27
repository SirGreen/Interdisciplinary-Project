namespace DADN.Services
{
    public static class ApiConfig
    {
        public static string ApiUrl { get; set; } = "https://61e9-34-150-176-139.ngrok-free.app/predict";
        public static bool UseCustomApi { get; set; } = true; // Flag để dễ dàng chuyển đổi giữa API cũ và mới
    }
}