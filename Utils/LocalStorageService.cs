namespace DADN.Utils
{
    public class LocalStorageService : IStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Save(string key, string jsonData)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Response.Cookies.Append(key, jsonData);
            }
        }

        public string? Get(string key)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Cookies.ContainsKey(key))
            {
                return context.Request.Cookies[key];
            }
            return null;
        }

        public void Delete(string key)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Response.Cookies.Delete(key);
            }
        }
    }
}
