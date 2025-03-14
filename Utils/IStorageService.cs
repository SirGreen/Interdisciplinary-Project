namespace DADN.Utils
{
    public interface IStorageService
    {
        void Save(string key, string jsonData);
        string? Get(string key);
        void Delete(string key);
    }
}
