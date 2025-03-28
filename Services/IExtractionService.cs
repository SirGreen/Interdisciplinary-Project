using System.Threading.Tasks;

public interface IExtractionService
{
    Task<MotorCatalog> ExtractDataFromWebAsync(string url);
}
