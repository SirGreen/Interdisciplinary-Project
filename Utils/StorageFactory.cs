namespace DADN.Utils
{
    public static class StorageFactory
    {
        public static IStorageService CreateStorage(string type, IHttpContextAccessor httpContextAccessor)
        {
            return type switch
            {
                "MongoDB" => new MongoStorageService(), // MongoDB (Sẽ làm sau)
                _ => new LocalStorageService(httpContextAccessor) // Mặc định là Local Storage
            };
        }
    }
}
