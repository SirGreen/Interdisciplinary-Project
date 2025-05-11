namespace DADN.Services
{
    public static class ApiConfig
    {
        public static string ApiUrl { get; set; } = "https://0a3a-35-231-33-112.ngrok-free.app/predict";
        public static bool UseCustomApi { get; set; } = true; // Flag để dễ dàng chuyển đổi giữa API cũ và mới
    }
}